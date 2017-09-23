using FDM90.Models;
using FDM90.Models.Helpers;
using FDM90.Repository;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Data;

namespace FDM90.Handlers
{
    public class CampaignHandler : ICampaignHandler
    {
        private IRepository<Campaign> _campaignRepo;
        private IReadMultipleSpecific<Campaign> _campaignReadMultipleRepo;
        private IReadAll<Campaign> _campaignReadRepo;
        private IReadSpecific<Campaign> _campaignReadSpecificRepo;
        private IFacebookHandler _facebookHandler;
        private ITwitterHandler _twitterHandler;
        private IUserHandler _userHandler;
        private List<IMediaHandler> _mediaHandlers = new List<IMediaHandler>();

        public CampaignHandler() : this(new CampaignRepository(), new FacebookHandler(), new TwitterHandler(), new UserHandler())
        {

        }

        public CampaignHandler(IRepository<Campaign> campaignRepo, IFacebookHandler facebookHandler, ITwitterHandler twitterHandler, IUserHandler userHandler)
        {
            _campaignRepo = campaignRepo;
            _campaignReadMultipleRepo = (IReadMultipleSpecific<Campaign>)campaignRepo;
            _campaignReadRepo = (IReadAll<Campaign>)campaignRepo;
            _campaignReadSpecificRepo = (IReadSpecific<Campaign>)campaignRepo;
            _facebookHandler = facebookHandler;
            _twitterHandler = twitterHandler;
            _userHandler = userHandler;
            _mediaHandlers.AddRange(new IMediaHandler[] { _facebookHandler, _twitterHandler });
        }

        public Task CreateCampaign(User user, string name, string startDate, string endDate, string targets)
        {
            Campaign newCampaign = new Campaign() {
                UserId = user.UserId,
                CampaignName = name,
                StartDate = DateTime.Parse(startDate),
                EndDate = DateTime.Parse(endDate),
                Targets = targets
            };

            _campaignRepo.Create(newCampaign);
            user.Campaigns++;
            _userHandler.UpdateUser(user);
            return UpdateCampaigns(user.UserId, newCampaign);
        }

        public Task UpdateCampaigns(Guid userId, Campaign newCampaign)
        {
            DateTimeFormatInfo dateInfo = DateTimeFormatInfo.CurrentInfo;
            Calendar calendar = dateInfo.Calendar;

            var existingCampaigns = GetUserCampaigns(userId);
            var user = _userHandler.GetUser(userId.ToString());
            int firstWeekNumber = calendar.GetWeekOfYear(newCampaign.StartDate, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);
            int lastWeekNumber = calendar.GetWeekOfYear(newCampaign.EndDate, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);
            int currentWeekNumber = calendar.GetWeekOfYear(DateTime.Now, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);

            JObject newProgress = new JObject();
            List<Task> tasks = new List<Task>();

            // here when first week only less than current week
            if (firstWeekNumber < currentWeekNumber)
            {

                foreach (Campaign campaign in existingCampaigns.Where(x => x.StartDate <= newCampaign.StartDate && !string.IsNullOrEmpty(x.Progress)))
                {
                    // get weeks for new campaign
                    JObject progress = JObject.Parse(campaign.Progress);

                    foreach (JProperty media in progress.Properties())
                    {
                        JObject newMediaProgress = new JObject();

                        foreach (JProperty week in media.Values())
                        {
                            if (int.Parse(week.Name.Substring(4)) >= firstWeekNumber && int.Parse(week.Name.Substring(4)) <= lastWeekNumber)
                            {
                                newMediaProgress.Add(week.Name, week.Value);
                            }
                        }
                        newProgress.Add(media.Name, newMediaProgress);
                    }
                }

                foreach (IMediaHandler mediaHandler in _mediaHandlers.Where(x =>
                                             bool.Parse(user.GetType().GetProperties().Where(y => y.Name == x.MediaName).First().GetValue(user).ToString())))
                {
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        List<DateTime> dates = new List<DateTime>();

                        if ((JObject)newProgress[mediaHandler.MediaName] == null)
                        {
                            newProgress.Add(mediaHandler.MediaName, new JObject());
                        }

                        for (int i = firstWeekNumber; i < currentWeekNumber; i++)
                        {
                            if ((JObject)newProgress[mediaHandler.MediaName]["Week" + i] == null)
                            {
                                dates.AddRange(DateHelper.GetDatesFromWeekNumber(i));
                            }
                        }

                        if (dates.Count() > 0)
                        {
                            foreach (JObject newWeek in mediaHandler.GetCampaignInfo(userId, dates.ToArray()))
                            {
                                if ((JObject)newProgress[mediaHandler.MediaName][newWeek.Path] == null)
                                    ((JObject)newProgress[mediaHandler.MediaName]).Add(newWeek.Path, newWeek);
                            }
                        }
                    }));
                }
            }
            else
            {
                tasks.Add(Task.FromResult<object>(null));
            }

            return Task.Factory.ContinueWhenAll(tasks.ToArray(), taskReturned =>
            {
                if (newProgress != null)
                {
                    newCampaign.Progress = newProgress.ToString();
                    _campaignRepo.Update(newCampaign);
                }
            });
        }

        public Task<bool> DailyUpdate()
        {
            List<Task<bool>> tasks = new List<Task<bool>>();
            foreach(var userCampaigns in _campaignReadRepo.ReadAll().GroupBy(x => x.UserId))
            {
                User user = _userHandler.GetUser(userCampaigns.First().UserId.ToString());

                UpdateMedias(user).Wait();

                // new task for each user
                tasks.Add(Task.Factory.StartNew(() => CampaignsUpdate(user, userCampaigns).Result));
            }

            return Task.Factory.ContinueWhenAll(tasks.ToArray(), taskReturned => { return taskReturned[0].Result; });
        }

        private Task UpdateMedias(User user)
        {
            List<Task> tasks = new List<Task>();

            foreach (IMediaHandler mediaHandler in _mediaHandlers.Where(x =>
                             bool.Parse(user.GetType().GetProperties().Where(y => y.Name == x.MediaName).First().GetValue(user).ToString())))
            {
                tasks.Add(Task.Factory.StartNew(() => mediaHandler.GetMediaData(user.UserId, DateHelper.GetDates(DateTime.Now.AddDays(-8).Date, DateTime.Now.AddDays(-1).Date))));
            }

            return Task.Factory.ContinueWhenAll(tasks.ToArray(), taskReturned => { return taskReturned[0]; });
        }

        private Task<bool> CampaignsUpdate(User user, IGrouping<Guid, Campaign> userCampaigns)
        {
            List<Task<JObject>> tasks = new List<Task<JObject>>();
            var campaigns = userCampaigns;
            JObject newDayProgress = new JObject();

            // check if any campaigns have date valid
            if (campaigns.Any(x => x.EndDate <= DateTime.Now.Date.AddDays(7)))
            {
                // call media get info
                foreach (IMediaHandler mediaHandler in _mediaHandlers.Where(x =>
                                             bool.Parse(user.GetType().GetProperties().Where(y => y.Name == x.MediaName).First().GetValue(user).ToString())))
                {
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        if ((JObject)newDayProgress[mediaHandler.MediaName] == null)
                        {
                            newDayProgress.Add(mediaHandler.MediaName, new JObject());
                        }

                        foreach (JObject newDay in mediaHandler.GetCampaignInfo(user.UserId, new DateTime[] { DateTime.Now.AddDays(-7) }))
                        {
                            ((JObject)newDayProgress[mediaHandler.MediaName]).Add(newDay.Path, newDay);
                        }

                        return newDayProgress;
                    }));
                }
            }

            return Task.Factory.ContinueWhenAll(tasks.ToArray(), taskReturned =>
            {
                var newProgress = taskReturned[0].Result;

                var campaignsToUpdate = campaigns.Where(x => x.StartDate.AddDays(7) <= DateTime.Now.Date && x.EndDate.AddDays(7) >= DateTime.Now.Date);

                foreach (var campaign in campaignsToUpdate)
                {
                    foreach (JObject newMedia in newProgress.Values())
                    {
                        JObject existingProgress = JObject.Parse(campaign.Progress);
                        if (existingProgress[newMedia.Path] == null)
                        {
                            existingProgress.Add(newMedia.Path, new JObject());
                        }

                        JToken existingValue;
                        if ((((JObject)existingProgress[newMedia.Path]).TryGetValue(newMedia.Properties().First().Name, out existingValue)))
                        {
                            foreach (JProperty newProperties in ((JObject)newMedia.Properties().First().Value).Properties())
                            {
                                JToken existingMetric;
                                if (((JObject)((JObject)existingProgress[newMedia.Path]).GetValue(newMedia.Properties().First().Name))
                                            .TryGetValue(newProperties.Name, out existingMetric))
                                {
                                    ((JObject)((JObject)existingProgress[newMedia.Path]).GetValue(newMedia.Properties().First().Name))
                                                .GetValue(newProperties.Name).Replace(int.Parse(existingMetric.ToString())
                                                        + int.Parse(newMedia[newMedia.Properties().First().Name][newProperties.Name].ToString()));
                                }
                                else
                                {
                                    ((JObject)((JObject)existingProgress[newMedia.Path]).GetValue(newMedia.Properties().First().Name))
                                                .Add(newProperties.Name, int.Parse(newMedia[newMedia.Properties().First().Name][newProperties.Name].ToString()));
                                }
                            }
                        }
                        else
                        {
                            ((JObject)existingProgress[newMedia.Path]).Add(newMedia.Properties().First().Name, newMedia.Properties().First().Value);
                        }

                        campaign.Progress = existingProgress.ToString();
                    }
                }


                // update all valid campaigns progress
                foreach (var campaign in campaignsToUpdate)
                    _campaignRepo.Update(campaign);

                return campaignsToUpdate.Count() > 0;
            });
        }

        public List<Campaign> GetUserCampaigns(Guid userId)
        {
            return _campaignReadMultipleRepo.ReadMultipleSpecific(userId.ToString()).ToList();
        }

        public DataTable GenerateCampaignDataTable(Campaign campaign)
        {
            DataTable campaignDataTable = new DataTable();
            campaignDataTable.Columns.Add("Source", typeof(string));
            campaignDataTable.Columns.Add("Week", typeof(string));
            campaignDataTable.Columns.Add("Metric", typeof(string));
            campaignDataTable.Columns.Add("Target", typeof(int));
            campaignDataTable.Columns.Add("Progress", typeof(int));
            campaignDataTable.Columns.Add("AccumulatedProgress", typeof(int));
            JObject progress = JObject.Parse(campaign.Progress);
            JObject target = JObject.Parse(campaign.Targets);

            foreach (JProperty media in progress.Children())
            {
                foreach (JProperty week in media.Values().OrderBy(o => o.Path.Substring(4)))
                {
                    foreach (JProperty metric in week.Values())
                    {
                        DataRow row = campaignDataTable.NewRow();
                        row[0] = media.Name;
                        row[1] = week.Name.Substring(4);
                        row[2] = metric.Name;
                        row[3] = target[media.Name][metric.Name];
                        row[4] = metric.Value;

                        if (campaignDataTable.AsEnumerable().Where(w => w[0].ToString() == media.Name && w[2].ToString() == metric.Name).Count() == 0)
                        {
                            row[5] = metric.Value;
                        }
                        else
                        {
                            row[5] = int.Parse(metric.Value.ToString())
                                        + int.Parse(campaignDataTable.AsEnumerable().Where(w => w[0].ToString() == media.Name && w[2].ToString() == metric.Name).Last()[5].ToString());
                        }

                        campaignDataTable.Rows.Add(row);
                    }
                }
            }

            foreach (var groupRow in campaignDataTable.AsEnumerable().GroupBy(g => new { Week = g[1], Metric = g[2] }).OrderBy(o => o.Key.Week))
            {
                DataRow row = campaignDataTable.NewRow();
                row[0] = "Overall";
                row[1] = groupRow.First()[1];
                row[2] = groupRow.First()[2];

                int accumulatedTarget = 0;

                foreach (JProperty value in target.Children())
                {
                    accumulatedTarget += int.Parse(value.Value[groupRow.First()[2]].ToString());
                }

                row[3] = accumulatedTarget;

                row[4] = groupRow.Sum(x => int.Parse(x[4].ToString()));

                var overallEntry = campaignDataTable.AsEnumerable().Where(w => w[0].ToString() == "Overall"
                                        && w[2].ToString() == groupRow.First()[2].ToString()
                                            && w[1].ToString() == (int.Parse(groupRow.First()[1].ToString()) - 1).ToString());

                if (overallEntry.Count() != 0 && (int)overallEntry.First()[5] > groupRow.Sum(x => int.Parse(x[5].ToString())))
                {
                    row[5] = (int)overallEntry.First()[5] + groupRow.Sum(x => int.Parse(x[5].ToString()));
                }
                else
                {
                    row[5] = groupRow.Sum(x => int.Parse(x[5].ToString()));
                }

                campaignDataTable.Rows.Add(row);

            }

            return campaignDataTable;
        }

        public void DeleteForUser(Guid userId)
        {
            _campaignRepo.Delete(new Campaign() { UserId = userId });
        }

        public void RemoveMediaAfterDelete(Guid userId, string mediaName)
        {
            var userCampaigns = GetUserCampaigns(userId);

            foreach(Campaign campaign in userCampaigns)
            {
                var alterTargets = JObject.Parse(campaign.Targets);
                alterTargets.Remove(mediaName);
                campaign.Targets = alterTargets.ToString();

                var alterProgress = JObject.Parse(campaign.Progress);
                alterProgress.Remove(mediaName);
                campaign.Progress = alterProgress.ToString();

                _campaignRepo.Update(campaign);
            }
        }

        public void DeleteCampaign(Campaign deletingCampaign)
        {
            _campaignRepo.Delete(deletingCampaign);
        }
    }
}