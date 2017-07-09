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

namespace FDM90.Handlers
{
    public class GoalHandler : IGoalHandler
    {
        private IRepository<Goal> _goalRepo;
        private IReadMultipleSpecific<Goal> _goalReadMultipleRepo;
        private IFacebookHandler _facebookHandler;
        private ITwitterHandler _twitterHandler;
        private IUserHandler _userHandler;
        private List<IMediaHandler> _mediaHandlers = new List<IMediaHandler>();
        public Task updateGoalsTask;

        public GoalHandler() : this(new GoalRepository(), new FacebookHandler(), new TwitterHandler(), new UserHandler())
        {

        }

        public GoalHandler(IRepository<Goal> goalRepo, IFacebookHandler facebookHandler, ITwitterHandler twitterHandler, IUserHandler userHandler)
        {
            _goalRepo = goalRepo;
            _goalReadMultipleRepo = (IReadMultipleSpecific<Goal>)goalRepo;
            _facebookHandler = facebookHandler;
            _twitterHandler = twitterHandler;
            _userHandler = userHandler;
            _mediaHandlers.AddRange(new IMediaHandler[] { _facebookHandler, twitterHandler });
        }

        public void CreateGoal(Guid userId, int goals, string name, string startDate, string endDate, string targets)
        {
            Goal newGoal = new Goal() {
                UserId = userId,
                GoalName = name,
                StartDate = DateTime.Parse(startDate),
                EndDate = DateTime.Parse(endDate),
                Targets = targets
            };

            _goalRepo.Create(newGoal);
            _userHandler.UpdateUser(new User() { UserId = userId, Goals = goals + 1 });
            updateGoalsTask = UpdateGoals(userId, newGoal);
        }

        public Task UpdateGoals(Guid userId, Goal newGoal)
        {
            DateTimeFormatInfo dateInfo = DateTimeFormatInfo.CurrentInfo;
            Calendar calendar = dateInfo.Calendar;

            var existingGoals = GetUserGoals(userId);
            var user = _userHandler.GetUser(userId.ToString());
            int firstWeekNumber = calendar.GetWeekOfYear(newGoal.StartDate, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);
            int lastWeekNumber = calendar.GetWeekOfYear(newGoal.EndDate, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);
            int currentWeekNumber = calendar.GetWeekOfYear(DateTime.Now, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);

            JObject newProgress = new JObject();
            foreach (Goal goal in existingGoals.Where(x => x.StartDate <= newGoal.StartDate && !string.IsNullOrEmpty(x.Progress)))
            {
                // get weeks for new goal
                JObject progress = JObject.Parse(goal.Progress);
                
                foreach(JProperty media in progress.Properties())
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

            List<Task> tasks = new List<Task>();

            // here when first week only less than current week, but not if we have info
            if (firstWeekNumber < currentWeekNumber && newProgress.First?.Children().Values().Count() != currentWeekNumber - firstWeekNumber)
            {
                DateTime[] dates = DateHelper.GetDates(newGoal.StartDate.AddDays(newProgress.First != null ? newProgress.First.Children().Values().Count() * 7 : 0), newGoal.EndDate, false);

                foreach (IMediaHandler mediaHandler in _mediaHandlers.Where(x =>
                                             bool.Parse(user.GetType().GetProperties().Where(y => y.Name == x.MediaName).First().GetValue(user).ToString())))
                {
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        if ((JObject)newProgress[mediaHandler.MediaName] == null)
                        {
                            newProgress.Add(mediaHandler.MediaName, new JObject());
                        }

                        if (dates.Count() > 0)
                        {
                            foreach (JObject newWeek in mediaHandler.GetGoalInfo(userId, dates))
                            {
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
                newGoal.Progress = newProgress.ToString();
                _goalRepo.Update(newGoal);
            });

        }

        public Task<bool> DailyUpdate()
        {
            List<Task<bool>> tasks = new List<Task<bool>>();
            foreach(var userGoals in _goalRepo.ReadAll().GroupBy(x => x.UserId))
            {
                User user = _userHandler.GetUser(userGoals.First().UserId.ToString());

                UpdateMedias(user).Wait();

                // new task for each user
                tasks.Add(Task.Factory.StartNew(() => GoalsUpdate(user, userGoals).Result));
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

        private Task<bool> GoalsUpdate(User user, IGrouping<Guid, Goal> userGoals)
        {
            List<Task<JObject>> tasks = new List<Task<JObject>>();
            var goals = userGoals;
            JObject newDayProgress = new JObject();

            // check if any goals have date valid
            if (goals.Any(x => x.EndDate <= DateTime.Now.Date.AddDays(7)))
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

                        foreach (JObject newDay in mediaHandler.GetGoalInfo(user.UserId, new DateTime[] { DateTime.Now.AddDays(-7) }))
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

                var goalsToUpdate = goals.Where(x => x.StartDate.AddDays(7) <= DateTime.Now.Date && x.EndDate.AddDays(7) >= DateTime.Now.Date);

                foreach (var goal in goalsToUpdate)
                {
                    foreach (JObject newMedia in newProgress.Values())
                    {
                        JObject existingProgress = JObject.Parse(goal.Progress);
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

                        goal.Progress = existingProgress.ToString();
                    }
                }


                // update all valid goals progress
                foreach (var goal in goalsToUpdate)
                    _goalRepo.Update(goal);

                return goalsToUpdate.Count() > 0;
            });
        }

        public IEnumerable<Goal> GetUserGoals(Guid userId)
        {
            return _goalReadMultipleRepo.ReadMultipleSpecific(userId.ToString());
        }

        List<Goal> IGoalHandler.GetUserGoals(Guid userId)
        {
            return _goalReadMultipleRepo.ReadMultipleSpecific(userId.ToString()).ToList();
        }
    }
}