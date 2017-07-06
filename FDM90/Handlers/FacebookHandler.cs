using Facebook;
using FDM90.Models;
using FDM90.Models.Helpers;
using FDM90.Repository;
using FDM90.Singleton;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FDM90.Handlers
{
    public class FacebookHandler : IFacebookHandler
    {
        private IReadSpecific<FacebookCredentials> _facebookReadRepo;
        private IRepository<FacebookCredentials> _facebookRepo;
        private IUserHandler _userHandler;
        private IFacebookClientWrapper _facebookClientWrapper;
        static DateTimeFormatInfo dateInfo = DateTimeFormatInfo.CurrentInfo;
        Calendar calendar = dateInfo.Calendar;

        public string MediaName
        {
            get
            {
                return "Facebook";
            }
        }

        public FacebookHandler() : this(new FacebookRepository(), new UserHandler(), new FacebookClientWrapper())
        {

        }

        public FacebookHandler(IRepository<FacebookCredentials> facebookRepo, IUserHandler userHandler,
            IFacebookClientWrapper facebookClientWrapper)
        {
            _facebookRepo = facebookRepo;
            _facebookReadRepo = (IReadSpecific<FacebookCredentials>)facebookRepo;
            _userHandler = userHandler;
            _facebookClientWrapper = facebookClientWrapper;
        }

        public FacebookCredentials GetLogInDetails(Guid userId)
        {
            var result = _facebookReadRepo.ReadSpecific(userId.ToString()) ?? new FacebookCredentials();

            result.PermanentAccessToken = result.PermanentAccessToken ?? _facebookClientWrapper.GetLoginUrl();

            //login
            return result;
        }

        public FacebookCredentials SaveLogInDetails(Guid userId, string pageName)
        {
            FacebookCredentials credentials = new FacebookCredentials(userId, pageName);
            try
            {
                _facebookRepo.Create(credentials);

                _userHandler.UpdateUserMediaActivation(new User(credentials.UserId), "Facebook");

                credentials.PermanentAccessToken = _facebookClientWrapper.GetLoginUrl();
            }
            catch (Exception ex)
            {

            }

            return credentials;
        }

        public string SetAccessToken(string shortTermToken, Guid userId, string pageName)
        {
            var permanentTokenString = _facebookClientWrapper.GetPermanentAccessToken(shortTermToken, pageName);

            //save token to user
            _facebookRepo.Update(new FacebookCredentials()
            {
                UserId = userId,
                PermanentAccessToken = permanentTokenString
            });

            // trigger get info
            Task.Factory.StartNew(() => GetMediaData(userId, GetDates(DateTime.Now.AddMonths(-6).Date, DateTime.Now.AddDays(-7))));

            return permanentTokenString;
        }

        public void GetMediaData(Guid userId, DateTime[] dates)
        {
            var currentData = _facebookReadRepo.ReadSpecific(userId.ToString());

            dynamic facebookData =
                _facebookClientWrapper.GetData(FacebookHelper.UrlBuilder(FacebookParameters.Field, "", new string[]
                {
                    FacebookHelper.Id, FacebookHelper.Name, FacebookHelper.FanCount, FacebookHelper.TalkingAboutCount,
                    FacebookHelper.Posts
                }), currentData.PermanentAccessToken);

            FacebookData data = JsonHelper.Parse(facebookData, new FacebookData());

            while((facebookData.posts.paging != null && facebookData.posts.paging.next != null) 
                                && data.Posts.OrderBy(x => x.CreatedTime).First().CreatedTime > dates.OrderBy(x => x.Date).First())
            {
                facebookData.posts = _facebookClientWrapper.GetData(facebookData.posts.paging.next, currentData.PermanentAccessToken);
                for (int i = 0; i < facebookData.posts.data.Count; i++)
                {
                    data.Posts.Add(JsonHelper.Parse(facebookData.posts.data[i], new FacebookPostData()));
                }
            }

            data.Posts.RemoveAll(remove => !dates.Contains(remove.CreatedTime.Date));

            data = GetPostDetails(data);

            dynamic facebookLikeData =
                    _facebookClientWrapper.GetData(FacebookHelper.UrlBuilder(FacebookParameters.Insight, "", new string[]
                        {
                            FacebookHelper.PageLikes
                        }), currentData.PermanentAccessToken);

            data.PageLikes = ((FacebookData)JsonHelper.Parse(facebookLikeData.data, new FacebookData())).PageLikes;

            while ((facebookLikeData.paging != null && facebookLikeData.paging?.previous != null) 
                                && data.PageLikes.Values.OrderBy(x => x.EndTime).First().EndTime > dates.OrderBy(x => x.Date).First())
            {
                facebookLikeData = _facebookClientWrapper.GetData(facebookLikeData.paging.previous, currentData.PermanentAccessToken);
                for (int i = 0; i < facebookLikeData.data.Count; i++)
                {
                    data.PageLikes.Values.Add(JsonHelper.Parse(facebookLikeData.data[0].values[i], new FacebookInsightValueData()));
                }
            }

            data.PageLikes.Values.RemoveAll(remove => !dates.Contains(remove.EndTime.Date));

            dynamic storiesData =
                    _facebookClientWrapper.GetData(FacebookHelper.UrlBuilder(FacebookParameters.Insight, "", new string[]
                        {
                            FacebookHelper.PageStories
                        }), currentData.PermanentAccessToken);

            data.PageStories = ((FacebookData)JsonHelper.Parse(storiesData.data, new FacebookData())).PageStories;

            while ((facebookLikeData.paging != null && facebookLikeData.paging?.previous != null) 
                                    && data.PageStories.Values.OrderBy(x => x.EndTime).First().EndTime > dates.OrderBy(x => x.Date).First())
            {
                storiesData =
                _facebookClientWrapper.GetData(storiesData.paging.previous, currentData.PermanentAccessToken);

                for (int i = 0; i < storiesData.data[0].values.Count; i++)
                {
                    data.PageStories.Values.Add(JsonHelper.Parse(storiesData.data[0].values[i], new FacebookInsightValueData()));
                }
            }

            data.PageStories.Values.RemoveAll(remove => !dates.Contains(remove.EndTime.Date));

            if (!string.IsNullOrWhiteSpace(currentData.FacebookData))
            {
                FacebookData existingData = JsonConvert.DeserializeObject<FacebookData>(currentData.FacebookData);
                existingData.Update(data);
                _facebookRepo.Update(new FacebookCredentials() { UserId = userId, FacebookData = JsonConvert.SerializeObject(existingData) });
            }
            else
            {
                _facebookRepo.Update(new FacebookCredentials() { UserId = userId, FacebookData = JsonConvert.SerializeObject(data) });
            }
        }

        private FacebookData GetPostDetails(FacebookData currentData)
        {
            List<FacebookPostData> updatedPosts = new List<FacebookPostData>();

            foreach (FacebookPostData post in currentData.Posts)
            {
                dynamic postData =
                    _facebookClientWrapper.GetData(
                        FacebookHelper.UrlBuilder(FacebookParameters.Insight, post.Id, new string[]
                            {FacebookHelper.PostReach, FacebookHelper.PostEngagedUsers}),
                        currentData.AccessToken);

                updatedPosts.Add(JsonHelper.Parse(postData.data, post));
            }

            currentData.Posts = updatedPosts;

            return currentData;
        }

        public IJEnumerable<JToken> GetGoalInfo(Guid userId, DateTime[] dates)
        {
            FacebookCredentials facebookCreds = _facebookReadRepo.ReadSpecific(userId.ToString());

            // exposure - fan reach group by week
            JObject facebookTargets = new JObject();
            FacebookData data = JsonConvert.DeserializeObject<FacebookData>(facebookCreds.FacebookData);

            // check within date
            foreach (FacebookPostData post in data.Posts.Where(post => dates.Contains(post.CreatedTime.Date)))
            {
                int weekNumber = calendar.GetWeekOfYear(post.CreatedTime, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);
                JObject week = new JObject();
                // add to object / update object 
                JToken weekExisting;

                if (!facebookTargets.TryGetValue("Week" + weekNumber.ToString(), out weekExisting))
                {
                    facebookTargets.Add("Week" + weekNumber, week);
                }

                JToken existingValue;
                if (((JObject)facebookTargets.GetValue("Week" + weekNumber)).TryGetValue("Exposure", out existingValue))
                {
                    ((JObject)facebookTargets.GetValue("Week" + weekNumber)).GetValue("Exposure").Replace(int.Parse(existingValue.ToString()) + post.TotalReach.Values[0].Value);
                }
                else
                {
                    ((JObject)facebookTargets.GetValue("Week" + weekNumber)).Add("Exposure", post.TotalReach.Values[0].Value);
                }

                // engagement
                if (!facebookTargets.TryGetValue("Week" + weekNumber.ToString(), out weekExisting))
                {
                    facebookTargets.Add("Week" + weekNumber, week);
                }

                JToken existingEngagementValue;
                if (((JObject)facebookTargets.GetValue("Week" + weekNumber)).TryGetValue("Engagement", out existingEngagementValue))
                {
                    ((JObject)facebookTargets.GetValue("Week" + weekNumber)).GetValue("Engagement").Replace(int.Parse(existingEngagementValue.ToString()) + post.EngagedUsers.Values[0].Value);
                }
                else
                {
                    ((JObject)facebookTargets.GetValue("Week" + weekNumber)).Add("Engagement", post.EngagedUsers.Values[0].Value);
                }
            }

            foreach (FacebookInsightValueData insightValue in data.PageLikes.Values.Where(like => dates.Contains(like.EndTime.Date)))
            {
                int weekNumber = calendar.GetWeekOfYear(insightValue.EndTime, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);
                JObject week = new JObject();
                // add to object / update object
                JToken weekExisting;

                if (!facebookTargets.TryGetValue("Week" + weekNumber.ToString(), out weekExisting))
                {
                    facebookTargets.Add("Week" + weekNumber, week);
                }

                JToken existingValue;
                if (((JObject)facebookTargets.GetValue("Week" + weekNumber)).TryGetValue("Influence", out existingValue))
                {
                    ((JObject)facebookTargets.GetValue("Week" + weekNumber)).GetValue("Influence").Replace(int.Parse(existingValue.ToString()) + insightValue.Value);
                }
                else
                {
                    ((JObject)facebookTargets.GetValue("Week" + weekNumber)).Add("Influence", insightValue.Value);
                }
            }

            foreach (FacebookInsightValueData insightValue in data.PageStories.Values.Where(story => dates.Contains(story.EndTime.Date)))
            {
                int weekNumber = calendar.GetWeekOfYear(insightValue.EndTime, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);
                JObject week = new JObject();
                // add to object / update object
                JToken weekExisting;

                if (!facebookTargets.TryGetValue("Week" + weekNumber.ToString(), out weekExisting))
                {
                    facebookTargets.Add("Week" + weekNumber, week);
                }

                JToken existingValue;
                if (((JObject)facebookTargets.GetValue("Week" + weekNumber)).TryGetValue("Influence", out existingValue))
                {
                    ((JObject)facebookTargets.GetValue("Week" + weekNumber)).GetValue("Influence").Replace(int.Parse(existingValue.ToString()) + insightValue.Value);
                }
                else
                {
                    ((JObject)facebookTargets.GetValue("Week" + weekNumber)).Add("Influence", insightValue.Value);
                }
            }

            return facebookTargets.Values();
        }

        private DateTime[] GetDates(DateTime startDate, DateTime endDate)
        {
            List<DateTime> dateList = new List<DateTime>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date < DateTime.Now.AddDays(-7).Date)
                    dateList.Add(date);
            }
            return dateList.ToArray();
        }

        //private DateTime GetEndDateOfPreviousWeek(DateTime startDate)
        //{
        //    return startDate.AddDays(-((int)startDate.DayOfWeek == 0 ? 7 : (int)startDate.DayOfWeek)); 
        //}
    }
}