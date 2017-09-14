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
        private IReadAll<FacebookCredentials> _facebookReadAllRepo;
        private IReadSpecific<FacebookCredentials> _facebookReadSpecificRepo;
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
            _facebookReadAllRepo = (IReadAll<FacebookCredentials>)facebookRepo;
            _facebookReadSpecificRepo = (IReadSpecific<FacebookCredentials>)facebookRepo;
            _userHandler = userHandler;
            _facebookClientWrapper = facebookClientWrapper;
        }

        public FacebookCredentials GetLogInDetails(Guid userId)
        {
            return _facebookReadSpecificRepo.ReadSpecific(new FacebookCredentials() { UserId = userId });
        }

        public FacebookCredentials SaveLogInDetails(Guid userId, string pageName)
        {
            FacebookCredentials credentials = new FacebookCredentials(userId, pageName);

            _facebookRepo.Create(credentials);

            _userHandler.UpdateUserMediaActivation(new User(credentials.UserId), MediaName);

            credentials.PermanentAccessToken = _facebookClientWrapper.GetLoginUrl();

            return credentials;
        }

        public Task SetAccessToken(string shortTermToken, Guid userId, string pageName)
        {
            var permanentTokenString = _facebookClientWrapper.GetPermanentAccessToken(shortTermToken, pageName);

            if (permanentTokenString.Contains(' '))
                return Task.Factory.StartNew(() => 
                {
                    _facebookRepo.Delete(new FacebookCredentials(userId, pageName));
                    return permanentTokenString;
                });

            //save token to user
            _facebookRepo.Update(new FacebookCredentials()
            {
                UserId = userId,
                PermanentAccessToken = permanentTokenString
            });

            // trigger get info
            return Task.Factory.StartNew(() => GetMediaData(userId, DateHelper.GetDates(DateTime.Now.AddMonths(-1).Date, DateTime.Now.Date)));
        }

        public void GetMediaData(Guid userId, DateTime[] dates)
        {
            var currentData = _facebookReadSpecificRepo.ReadSpecific(new FacebookCredentials() { UserId = userId });

            if (string.IsNullOrWhiteSpace(currentData.PermanentAccessToken)) return;

            dynamic basicData =
                _facebookClientWrapper.GetData(FacebookHelper.UrlBuilder(FacebookParameters.Field, "", new string[]
                {
                    FacebookHelper.Id, FacebookHelper.Name, FacebookHelper.FanCount, FacebookHelper.TalkingAboutCount,
                }), currentData.PermanentAccessToken);

            FacebookData data = JsonHelper.Parse(basicData, new FacebookData());

            data = GetMetricData(dates, currentData.PermanentAccessToken, data);

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

        private FacebookData GetMetricData(DateTime[] dates, string accessToken, FacebookData data)
        {
            dynamic postData =
                _facebookClientWrapper.GetData(FacebookHelper.UrlBuilder(FacebookParameters.Field, "", new string[]
                {
                    FacebookHelper.Posts
                }), accessToken);

            data.Posts = new List<FacebookPostData>();

            foreach (var post in postData.posts.data)
            {
                data.Posts.Add(JsonHelper.Parse(post, new FacebookPostData()));
            }

            while ((postData.posts.paging != null && postData.posts.paging.next != null)
                                && data.Posts.OrderBy(x => x.CreatedTime).First().CreatedTime > dates.OrderBy(x => x.Date).First())
            {
                postData.posts = _facebookClientWrapper.GetData(postData.posts.paging.next, accessToken);
                for (int i = 0; i < postData.posts.data.Count; i++)
                {
                    data.Posts.Add(JsonHelper.Parse(postData.posts.data[i], new FacebookPostData()));
                }
            }

            data.Posts.RemoveAll(remove => !dates.Select(x => x.Date).Contains(remove.CreatedTime.Date));

            data = GetPostDetails(data, accessToken);

            dynamic facebookLikeData =
                    _facebookClientWrapper.GetData(FacebookHelper.UrlBuilder(FacebookParameters.Insight, "", new string[]
                        {
                            FacebookHelper.PageLikes
                        }), accessToken);

            data.PageLikes = ((FacebookData)JsonHelper.Parse(facebookLikeData.data, new FacebookData())).PageLikes;

            while ((facebookLikeData.paging != null && facebookLikeData.paging?.previous != null)
                                && data.PageLikes.Values.OrderBy(x => x.EndTime).First().EndTime > dates.OrderBy(x => x.Date).First())
            {
                facebookLikeData = _facebookClientWrapper.GetData(facebookLikeData.paging.previous, accessToken);
                for (int i = 0; i < facebookLikeData.data[0].values.Count; i++)
                {
                    data.PageLikes.Values.Add(JsonHelper.Parse(facebookLikeData.data[0].values[i], new FacebookInsightValueData()));
                }
            }

            data.PageLikes.Values.RemoveAll(remove => !dates.Select(x => x.Date).Contains(remove.EndTime.Date));

            dynamic storiesData =
                    _facebookClientWrapper.GetData(FacebookHelper.UrlBuilder(FacebookParameters.Insight, "", new string[]
                        {
                            FacebookHelper.PageStories
                        }), accessToken);

            data.PageStories = ((FacebookData)JsonHelper.Parse(storiesData.data, new FacebookData())).PageStories;

            while ((facebookLikeData.paging != null && facebookLikeData.paging?.previous != null)
                                    && data.PageStories.Values.OrderBy(x => x.EndTime).First().EndTime > dates.OrderBy(x => x.Date).First())
            {
                storiesData =
                _facebookClientWrapper.GetData(storiesData.paging.previous, accessToken);

                for (int i = 0; i < storiesData.data[0].values.Count; i++)
                {
                    data.PageStories.Values.Add(JsonHelper.Parse(storiesData.data[0].values[i], new FacebookInsightValueData()));
                }
            }

            data.PageStories.Values.RemoveAll(remove => !dates.Select(x => x.Date).Contains(remove.EndTime.Date));
            return data;
        }

        private FacebookData GetPostDetails(FacebookData currentData, string accessToken)
        {
            List<FacebookPostData> updatedPosts = new List<FacebookPostData>();

            foreach (FacebookPostData post in currentData.Posts)
            {
                dynamic postData =
                    _facebookClientWrapper.GetData(
                        FacebookHelper.UrlBuilder(FacebookParameters.Insight, post.Id, new string[]
                            {FacebookHelper.PostReach, FacebookHelper.PostEngagedUsers}),
                        accessToken);

                updatedPosts.Add(JsonHelper.Parse(postData.data, post));
            }

            currentData.Posts = updatedPosts;

            return currentData;
        }

        public IJEnumerable<JToken> GetCampaignInfo(Guid userId, DateTime[] dates)
        {
            FacebookCredentials facebookCreds = _facebookReadSpecificRepo.ReadSpecific(new FacebookCredentials() { UserId = userId });

            // exposure - fan reach group by week
            JObject facebookTargets = new JObject();
            FacebookData data = JsonConvert.DeserializeObject<FacebookData>(facebookCreds.FacebookData);

            // check within date
            foreach (FacebookPostData post in data.Posts.Where(post => dates.Contains(post.CreatedTime.Date)))
            {
                facebookTargets = JsonHelper.AddWeekValue(facebookTargets, "Exposure", post.CreatedTime.Date, post.TotalReach.Values[0].Value);

                facebookTargets = JsonHelper.AddWeekValue(facebookTargets, "Influence", post.CreatedTime.Date, post.Likes?.Count);
                facebookTargets = JsonHelper.AddWeekValue(facebookTargets, "Influence", post.CreatedTime.Date, post.Comments?.Count);
                facebookTargets = JsonHelper.AddWeekValue(facebookTargets, "Influence", post.CreatedTime.Date, post.Shares?.Count);

                facebookTargets = JsonHelper.AddWeekValue(facebookTargets, "Engagement", post.CreatedTime.Date, post.EngagedUsers.Values[0].Value);
            }

            foreach (FacebookInsightValueData insightValue in data.PageLikes.Values.Where(like => dates.Contains(like.EndTime.Date)))
            {
                facebookTargets = JsonHelper.AddWeekValue(facebookTargets, "Influence", insightValue.EndTime, insightValue.Value);
            }

            foreach (FacebookInsightValueData insightValue in data.PageStories.Values.Where(story => dates.Contains(story.EndTime.Date)))
            {
                facebookTargets = JsonHelper.AddWeekValue(facebookTargets, "Influence", insightValue.EndTime, insightValue.Value);
            }

            return facebookTargets.Values();
        }

        public FacebookData GetFacebookData(Guid userId)
        {
            FacebookCredentials creds = _facebookReadSpecificRepo.ReadSpecific(new FacebookCredentials() { UserId = userId });

            FacebookData todaysData = creds == null || string.IsNullOrWhiteSpace(creds.PermanentAccessToken) ? null :
                                        GetMetricData(new DateTime[] { DateTime.Now.Date }, creds.PermanentAccessToken, new FacebookData());

            return creds == null || string.IsNullOrWhiteSpace(creds.FacebookData) ? todaysData : JsonConvert.DeserializeObject<FacebookData>(creds.FacebookData).Update(todaysData);
        }

        public void PostData(Dictionary<string, string> postParameters, Guid userId)
        {
            _facebookClientWrapper.PostData(postParameters, _facebookReadSpecificRepo.ReadSpecific(new FacebookCredentials() { UserId = userId }).PermanentAccessToken);
        }

        public List<Task> DailyUpdate()
        {
            List<Task> tasks = new List<Task>();
            foreach (FacebookCredentials facebookCreds in _facebookReadAllRepo.ReadAll())
            {
                 tasks.Add(Task.Factory.StartNew(() =>
                        GetMediaData(facebookCreds.UserId, new[] { DateTime.Now.AddDays(-8) })));
            }
            return tasks;
        }
    }
}