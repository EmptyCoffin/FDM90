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
                            {FacebookHelper.PostDetails}),
                        currentData.AccessToken);

                updatedPosts.Add(JsonHelper.Parse(postData.data, post));
            }

            currentData.Posts = updatedPosts;

            return currentData;
        }

        public string GetGoalInfo(Guid userId, DateTime startDate, DateTime endDate)
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
            int limit = 25;
            while (data.Posts.Last() == null || data.Posts.Last().CreatedTime > startDate)
            {
                dynamic facebookData =
                    _facebookClientWrapper.GetData(FacebookHelper.UrlBuilder(FacebookParameters.Field, "", new string[]
                        {
                        FacebookHelper.Posts
                        }), userPermanentAcessToken, new { limit = limit.ToString(), offset = data.Posts.Count });

                data = JsonHelper.Parse(facebookData, new FacebookData());

                limit += 25;
            }

            // influence
            while (data.PageLikes.Values == null || data.PageLikes.Values.Last().EndTime > startDate)
            {
                dynamic facebookData =
                    _facebookClientWrapper.GetData(FacebookHelper.UrlBuilder(FacebookParameters.Insight, "", new string[]
                        {
                        FacebookHelper.PageLikes
                        }), userPermanentAcessToken, new { limit = limit.ToString(), offset = data.Posts.Count });

                data = JsonHelper.Parse(facebookData, new FacebookData());

                limit += 25;
            }

            // check within date
            foreach (FacebookPostData post in data.Posts.Where(post => post.CreatedTime > startDate && post.CreatedTime < endDate))
            {
                dynamic postData =
                    _facebookClientWrapper.GetData(
                        FacebookHelper.UrlBuilder(FacebookParameters.Insight, post.Id, new string[]
                            {FacebookHelper.PostDetails}),
                        userPermanentAcessToken);

                FacebookPostData postDataParsed = JsonHelper.Parse(postData.data, post);

                int weekNumber = calendar.GetWeekOfYear(postDataParsed.CreatedTime, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);
                JObject week = new JObject();
                // add to object / update object
                JToken weekExisting;
                JToken existingValue;
                if (facebookTargets.TryGetValue("Week" + weekNumber.ToString(), out weekExisting) && week.TryGetValue("Exposure", out existingValue))
                {
                    week.GetValue("Exposure").Replace(int.Parse(existingValue.ToString()) + postDataParsed.TotalReach.Values[0].Value);
                }
                else
                {
                    week.Add("Exposure", postDataParsed.TotalReach.Values[0].Value);
                }

                if (facebookTargets.TryGetValue("Week" + weekNumber.ToString(), out weekExisting))
                {
                    facebookTargets.GetValue("Week" + weekNumber).Replace(int.Parse(weekExisting.ToString()) + postDataParsed.TotalReach.Values[0].Value);
                }
                else
                {
                    facebookTargets.Add("Week" + weekNumber, week);
                }
            }

            return facebookTargets.ToString();
        }

        private DateTime GetEndDateOfPreviousWeek(DateTime startDate)
        {
            return startDate.AddDays(-((int)startDate.DayOfWeek == 0 ? 7 : (int)startDate.DayOfWeek)); 
        }
    }
}