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

namespace FDM90.Handlers
{
    public class FacebookHandler : IFacebookHandler
    {
        private IReadSpecific<FacebookCredentials> _facebookReadRepo;
        private IRepository<FacebookCredentials> _facebookRepo;
        private IUserHandler _userHandler;
        private IFacebookClientWrapper _facebookClientWrapper;

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

            return permanentTokenString;
        }

        public FacebookData GetInitialFacebookData(string accessToken)
        {
            dynamic facebookData =
                _facebookClientWrapper.GetData(FacebookHelper.UrlBuilder(FacebookParameters.Field, "", new string[]
                {
                    FacebookHelper.Id, FacebookHelper.Name, FacebookHelper.FanCount, FacebookHelper.TalkingAboutCount,
                    FacebookHelper.Posts
                }), accessToken);

            FacebookData data = JsonHelper.Parse(facebookData, new FacebookData());

            data.AccessToken = accessToken;

            return data;
        }

        //public FacebookData GetLikeFacebookData(FacebookData currentData)
        //{
        //    dynamic facebookData =
        //        _facebookClientWrapper.GetData(FacebookHelper.UrlBuilder(FacebookParameters.Field, "", new string[]
        //            {
        //                FacebookHelper.Id, FacebookHelper.Name, FacebookHelper.FanCount,
        //                FacebookHelper.TalkingAboutCount
        //            }),
        //            currentData.AccessToken);

        //    return JsonHelper.Parse(facebookData, currentData);
        //}

        public FacebookData GetPostDetails(FacebookData currentData)
        {
            List<FacebookPostData> updatedPosts = new List<FacebookPostData>();

            foreach (FacebookPostData post in currentData.Posts)
            {
                dynamic postData =
                    _facebookClientWrapper.GetData(
                        FacebookHelper.UrlBuilder(FacebookParameters.Insight, post.Id, new string[]
                            {FacebookHelper.PostReach}),
                        currentData.AccessToken);

                updatedPosts.Add(JsonHelper.Parse(postData.data, post));
            }

            currentData.Posts = updatedPosts;

            return currentData;
        }

        public IJEnumerable<JToken> GetGoalInfo(Guid userId, DateTime startDate, DateTime endDate)
        {
            var userPermanentAcessToken = _facebookReadRepo.ReadSpecific(userId.ToString()).PermanentAccessToken;

            DateTimeFormatInfo dateInfo = DateTimeFormatInfo.CurrentInfo;
            Calendar calendar = dateInfo.Calendar;
            int currentWeekNumber = calendar.GetWeekOfYear(DateTime.Now, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);

            // check start date isn't in this week
            int startDateWeekNumber = calendar.GetWeekOfYear(startDate, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);

            startDate = currentWeekNumber == startDateWeekNumber ? GetEndDateOfPreviousWeek(startDate) : startDate;

            // exposure - fan reach group by week
            JObject facebookTargets = new JObject();
            FacebookData data = new FacebookData();

            dynamic facebookPostData =
                    _facebookClientWrapper.GetData(FacebookHelper.UrlBuilder(FacebookParameters.Field, "", new string[]
                        {
                        FacebookHelper.Posts
                        }), userPermanentAcessToken);

                data = JsonHelper.Parse(facebookPostData, data);

                while(facebookPostData.paging != null && facebookPostData.paging.next != null)
                {
                    dynamic facebookNextData =
                        _facebookClientWrapper.GetData(facebookPostData.paging.next, userPermanentAcessToken);

                    for (int i = 0; i < facebookNextData.data.Count; i++)
                    {
                        data.Posts.Add(JsonHelper.Parse(facebookNextData.data[i], new FacebookPostData()));
                    }

                    var dateList = data.Posts.Select(s => s.CreatedTime).ToList();
                    dateList.Sort((a, b) => b.CompareTo(a));

                    if (dateList.Last() < startDate)
                        break;
                }

            // influence
                dynamic facebookData =
                    _facebookClientWrapper.GetData(FacebookHelper.UrlBuilder(FacebookParameters.Insight, "", new string[]
                        {
                        FacebookHelper.PageLikes
                        }), userPermanentAcessToken);

                data.PageLikes = ((FacebookData)JsonHelper.Parse(facebookData.data, new FacebookData())).PageLikes;

            while (facebookData.paging != null && facebookData.paging.previous != null)
            {
                facebookData =
                    _facebookClientWrapper.GetData(facebookData.paging.previous, userPermanentAcessToken);

                for (int i = 0; i < facebookData.data[0].values.Count; i++)
                {
                    data.PageLikes.Values.Add(JsonHelper.Parse(facebookData.data[0].values[i], new FacebookInsightValueData()));
                }

                var dateList = data.PageLikes.Values.Select(s => s.EndTime).ToList();
                dateList.Sort((a, b) => b.CompareTo(a));

                if (dateList.Last() < startDate)
                    break;
            }

            dynamic storiesData =
                _facebookClientWrapper.GetData(FacebookHelper.UrlBuilder(FacebookParameters.Insight, "", new string[]
                    {
                        FacebookHelper.PageStories
                    }), userPermanentAcessToken);

            data.PageStories = ((FacebookData)JsonHelper.Parse(storiesData.data, new FacebookData())).PageStories;

            while (storiesData.paging != null && storiesData.paging.previous != null)
            {
                storiesData =
                    _facebookClientWrapper.GetData(storiesData.paging.previous, userPermanentAcessToken);

                for (int i = 0; i < storiesData.data[0].values.Count; i++)
                {
                    data.PageStories.Values.Add(JsonHelper.Parse(storiesData.data[0].values[i], new FacebookInsightValueData()));
                }

                var dateList = data.PageStories.Values.Select(s => s.EndTime).ToList();
                dateList.Sort((a, b) => b.CompareTo(a));

                if (dateList.Last() < startDate)
                    break;
            }

            // check within date
            foreach (FacebookPostData post in data.Posts.Where(post => post.CreatedTime > startDate && post.CreatedTime < endDate))
            {
                dynamic postData =
                    _facebookClientWrapper.GetData(
                        FacebookHelper.UrlBuilder(FacebookParameters.Insight, post.Id, new string[]
                            {FacebookHelper.PostReach}),
                        userPermanentAcessToken);

                post.TotalReach = ((FacebookPostData)JsonHelper.Parse(postData.data, new FacebookPostData())).TotalReach;

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
                dynamic facebookPostEngagementData =
                    _facebookClientWrapper.GetData(FacebookHelper.UrlBuilder(FacebookParameters.Insight, post.Id, new string[]
                        {
                            FacebookHelper.PostEngagedUsers
                        }), userPermanentAcessToken);

                post.EngagedUsers = ((FacebookPostData)JsonHelper.Parse(facebookPostEngagementData.data, new FacebookPostData())).EngagedUsers;

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

            foreach (FacebookInsightValueData insightValue in data.PageLikes.Values.Where(like => like.EndTime > startDate && like.EndTime < endDate))
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

            foreach (FacebookInsightValueData insightValue in data.PageStories.Values.Where(story => story.EndTime > startDate && story.EndTime < endDate))
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

        private DateTime GetEndDateOfPreviousWeek(DateTime startDate)
        {
            return startDate.AddDays(-((int)startDate.DayOfWeek == 0 ? 7 : (int)startDate.DayOfWeek)); 
        }
    }
}