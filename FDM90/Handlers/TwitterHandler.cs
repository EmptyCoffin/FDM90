using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;
using FDM90.Repository;
using FDM90.Models;
using LinqToTwitter;
using FDM90.Singleton;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using FDM90.Models.Helpers;

namespace FDM90.Handlers
{
    public class TwitterHandler : ITwitterHandler
    {
        private IReadAll<TwitterCredentials> _twitterReadAllRepo;
        private IReadSpecific<TwitterCredentials> _twitterReadSpecificRepo;
        private IRepository<TwitterCredentials> _twitterRepo;
        private IUserHandler _userHandler;
        private ITwitterClientWrapper _twitterClientWrapper;

        public TwitterHandler() : this(new TwitterRepository(), new UserHandler(), new TwitterClientWrapper())
        {

        }

        public TwitterHandler(IRepository<Models.TwitterCredentials> twitterRepo, IUserHandler userHandler, ITwitterClientWrapper twitterClientWrapper)
        {
            _twitterRepo = twitterRepo;
            _twitterReadAllRepo = (IReadAll<TwitterCredentials>)twitterRepo;
            _twitterReadSpecificRepo = (IReadSpecific<TwitterCredentials>)twitterRepo;
            _userHandler = userHandler;
            _twitterClientWrapper = twitterClientWrapper;
        }


        public string MediaName
        {
            get
            {
                return "Twitter";
            }
        }

        public IJEnumerable<JToken> GetCampaignInfo(Guid userId, DateTime[] dates)
        {
            JObject twitterTargets = new JObject();

            // get user twitter
            var twitterDetails = _twitterReadSpecificRepo.ReadSpecific(new TwitterCredentials() { UserId = userId });

            var data = TwitterData.Parse(twitterDetails.TwitterData, new TwitterData());
            data.Tweets = data.Tweets.OrderBy(x => x.CreatedAt).ToList();
            int screenNameFollowerCount = data.NumberOfFollowers;
            // get exposure - followers and followers of those retweeted/favorited
            foreach (var tweetDate in data.Tweets.GroupBy(x => x.CreatedAt.Date))
            {
                var userFollowers = screenNameFollowerCount * tweetDate.Count();
                int retweetFavoriteUserFollowers = 0;
                int retweetFavoriteCount = 0;

                foreach (Tweet tweet in tweetDate.Where(y => y.RetweetCount > 0 || y.FavoriteCount > 0))
                {
                    // get retweeters followers
                    retweetFavoriteUserFollowers += tweet.RetweetedUsers.Sum(x => x.NumberOfFollowers);
                    retweetFavoriteCount += tweet.FavoriteCount + tweet.RetweetCount;
                }

                double estimatedExposure = (userFollowers + retweetFavoriteUserFollowers) / 10;

                twitterTargets = JsonHelper.AddWeekValue(twitterTargets, "Exposure", tweetDate.First().CreatedAt, (int)estimatedExposure);

                if (retweetFavoriteUserFollowers > 0)
                {
                    // get influence - followers of those retweeted/favorited
                    double estimatedInfluence = retweetFavoriteUserFollowers / 10;

                    twitterTargets = JsonHelper.AddWeekValue(twitterTargets, "Influence", tweetDate.First().CreatedAt, (int)estimatedInfluence);
                }

                if(retweetFavoriteUserFollowers > 0)
                {
                    // get engagement - replies/mentions, direct messages, retweets, hashtags mentions, favorited
                    twitterTargets = JsonHelper.AddWeekValue(twitterTargets, "Engagement", tweetDate.First().CreatedAt, (int)retweetFavoriteCount);
                }

                if(data.NumberOfFollowersByDate.ContainsKey(tweetDate.First().CreatedAt.Date))
                {
                    if (data.NumberOfFollowersByDate.ContainsKey(tweetDate.First().CreatedAt.AddDays(-1).Date))
                    {
                        twitterTargets = JsonHelper.AddWeekValue(twitterTargets, "Acquisition", tweetDate.First().CreatedAt,
                                    (data.NumberOfFollowersByDate[tweetDate.First().CreatedAt.Date] - data.NumberOfFollowersByDate[tweetDate.First().CreatedAt.AddDays(-1).Date]));
                    }
                    else
                    {
                        twitterTargets = JsonHelper.AddWeekValue(twitterTargets, "Acquisition", tweetDate.First().CreatedAt, 0);
                    }
                }
            }

            return twitterTargets.Values();
        }

        private List<TwitterUser> GetRetweeterFollowers(ulong statusId, TwitterCredentials creds)
        {
            // retweeter ids
            var retweeters = _twitterClientWrapper.GetRetweeterFollowers(creds, statusId).Result;

            List<TwitterUser> retweeterUsers = new List<TwitterUser>();

            foreach (var status in retweeters)
            {
                if (status.User != null)
                {
                    retweeterUsers.Add(new TwitterUser()
                    {
                        NumberOfFollowers = status.User.FollowersCount
                    });
                }
            }

            return retweeterUsers;
        }

        private TwitterData GetTwitterData(TwitterCredentials twitterDetails, DateTime[] dates)
        {
            TwitterData data = null;
            var tweets = _twitterClientWrapper.GetTweets(new TwitterCredentials() {
                ScreenName = twitterDetails.ScreenName,
                AccessToken = EncryptionHelper.DecryptString(twitterDetails.AccessToken),
                AccessTokenSecret = EncryptionHelper.DecryptString(twitterDetails.AccessTokenSecret)
            }).Result;

            if (tweets.Count(tweet => dates.Select(x => x.Date).Contains(tweet.CreatedAt.Date)) > 0)
            {
                tweets.RemoveAll(remove => !dates.Select(x => x.Date).Contains(remove.CreatedAt.Date));

                data = new TwitterData(tweets, tweets.OrderBy(x => x.CreatedAt.Date).First().User.FollowersCount);

                foreach (Tweet tweet in data.Tweets)
                {
                    if (tweet.RetweetCount > 0)
                    {
                        tweet.RetweetedUsers = GetRetweeterFollowers(tweet.StatusID, new TwitterCredentials()
                        {
                            ScreenName = twitterDetails.ScreenName,
                            AccessToken = EncryptionHelper.DecryptString(twitterDetails.AccessToken),
                            AccessTokenSecret = EncryptionHelper.DecryptString(twitterDetails.AccessTokenSecret)
                        });
                    }
                }
            }

            return data == null ? new TwitterData(new List<Status>(),
                tweets.OrderBy(x => x.CreatedAt.Date).First().User.FollowersCount)
                : data;
        }

        public TwitterData GetTweets(string userId)
        {
            TwitterCredentials creds = _twitterReadSpecificRepo.ReadSpecific(new TwitterCredentials() { UserId = Guid.Parse(userId) });
            TwitterData todaysData = GetTwitterData(creds, new DateTime[] { DateTime.Now.Date });

            return !string.IsNullOrWhiteSpace(creds.TwitterData) ? TwitterData.Parse(creds.TwitterData, new TwitterData()).Update(todaysData) : todaysData;
        }

        public Task SaveUserDetails(string accessToken, string accessTokenSecret, string screenName, string userId)
        {
            TwitterCredentials creds = new TwitterCredentials()
            {
                UserId = Guid.Parse(userId),
                AccessToken = accessToken,
                AccessTokenSecret = accessTokenSecret,
                ScreenName = screenName
            };

            _twitterRepo.Create(creds);

            _userHandler.UpdateUserMediaActivation(new Models.User(Guid.Parse(userId)), MediaName, true);

            return Task.Factory.StartNew(() => GetMediaData(creds.UserId, DateHelper.GetDates(DateTime.Now.AddMonths(-1).Date, DateTime.Now.Date)));
        }

        public void GetMediaData(Guid userId, DateTime[] dates)
        {
            TwitterCredentials creds = _twitterReadSpecificRepo.ReadSpecific(new TwitterCredentials() { UserId = userId });

            if (creds == null) return;

            TwitterData data = GetTwitterData(creds, dates);

            if (!string.IsNullOrWhiteSpace(creds.TwitterData))
            {
                TwitterData existingData = TwitterData.Parse(creds.TwitterData, new TwitterData());
                existingData.Update(data);
                creds.TwitterData = JsonConvert.SerializeObject(existingData);
            }
            else
            {
                creds.TwitterData = JsonConvert.SerializeObject(data);
            }

            _twitterRepo.Update(creds);
        }

        public void PostData(Dictionary<string, string> postParameters, Guid userId)
        {
            TwitterCredentials creds = _twitterReadSpecificRepo.ReadSpecific(new TwitterCredentials() { UserId = userId });

            _twitterClientWrapper.PostTweet(new TwitterCredentials()
            {
                ScreenName = creds.ScreenName,
                AccessToken = EncryptionHelper.DecryptString(creds.AccessToken),
                AccessTokenSecret = EncryptionHelper.DecryptString(creds.AccessTokenSecret)
            }, postParameters);
        }

        public List<Task> DailyUpdate()
        {
            List<Task> tasks = new List<Task>();

            foreach (TwitterCredentials twitterCreds in _twitterReadAllRepo.ReadAll())
            {
                tasks.Add(Task.Factory.StartNew(() =>
                       GetMediaData(twitterCreds.UserId, new[] { DateTime.Now.AddDays(-8) })));
            }
            return tasks;
        }

        public Models.User DeleteMedia(Guid userId)
        {
            _twitterRepo.Delete(new TwitterCredentials() { UserId = userId });
            return _userHandler.UpdateUserMediaActivation(new Models.User(userId), MediaName, false);
        }
    }
}