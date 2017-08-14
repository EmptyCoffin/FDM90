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
        private IReadSpecific<TwitterCredentials> _twitterReadRepo;
        private IRepository<TwitterCredentials> _twitterRepo;
        private IUserHandler _userHandler;
        private ITwitterClientWrapper _twitterClientWrapper;

        public TwitterHandler() : this(new TwitterRepository(), new UserHandler(), new TwitterClientWrapper())
        {

        }

        public TwitterHandler(IRepository<Models.TwitterCredentials> twitterRepo, IUserHandler userHandler, ITwitterClientWrapper twitterClientWrapper)
        {
            _twitterRepo = twitterRepo;
            _twitterReadRepo = (IReadSpecific<Models.TwitterCredentials>)twitterRepo;
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
            var twitterDetails = _twitterReadRepo.ReadSpecific(userId.ToString());

            var data = TwitterData.Parse(twitterDetails.TwitterData, new TwitterData());
            int screenNameFollowerCount = data.NumberOfFollowers;
            // get exposure - followers and followers of those retweeted/favorited
            foreach (var tweetDate in data.Tweets.Where(w => w.FavoriteCount > 0 && w.RetweetCount > 0).GroupBy(x => x.CreatedAt.Date))
            {
                var numberOfMessages = tweetDate.Count();
                var numberOfFollowers = screenNameFollowerCount;

                foreach (Tweet tweet in tweetDate.Where(y => y.RetweetCount > 0))
                {
                    // get retweeters followers
                    numberOfFollowers = numberOfFollowers + tweet.RetweetedUsers.Sum(x => x.NumberOfFollowers);
                }

                double estimatedExposure = numberOfFollowers / 10;

                twitterTargets = JsonHelper.AddWeekValue(twitterTargets, "Exposure", tweetDate.First().CreatedAt, (int)estimatedExposure);
            }

            // get influence - followers of those retweeted/favorited
            foreach (var tweetDate in data.Tweets.Where(w => w.FavoriteCount > 0 && w.RetweetCount > 0).GroupBy(x => x.CreatedAt.Date))
            {
                var numberOfMessages = tweetDate.Count();
                var numberOfFollowers = 0;

                foreach (Tweet tweet in tweetDate.Where(y => y.RetweetCount > 0 || y.FavoriteCount > 0))
                {
                    // get retweeters followers
                    numberOfFollowers = numberOfFollowers + tweet.RetweetedUsers.Sum(x => x.NumberOfFollowers);
                }

                double estimatedInfluence = numberOfFollowers / 10;

                twitterTargets = JsonHelper.AddWeekValue(twitterTargets, "Influence", tweetDate.First().CreatedAt, (int)estimatedInfluence);
            }

            // get engagement - replies/mentions, direct messages, retweets, hashtags mentions, favorited
            foreach (var datedTweets in data.Tweets.Where(x => x.RetweetCount > 0 || x.FavoriteCount > 0).GroupBy(x => x.CreatedAt.Date))
            {
                var numberOfEngagements = 0;

                foreach (Tweet tweet in datedTweets)
                {
                    // get retweeters followers
                    numberOfEngagements = numberOfEngagements + tweet.FavoriteCount + tweet.RetweetCount;
                }

                twitterTargets = JsonHelper.AddWeekValue(twitterTargets, "Engagement", datedTweets.First().CreatedAt, (int)numberOfEngagements);
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
            var tweets = _twitterClientWrapper.GetTweets(twitterDetails).Result;

            if (tweets.Count(tweet => dates.Contains(tweet.CreatedAt.Date)) > 0)
            {
                tweets.RemoveAll(remove => !dates.Contains(remove.CreatedAt.Date));

                data = new TwitterData(tweets, tweets.OrderBy(x => x.CreatedAt.Date).First().User.FollowersCount);

                foreach (Tweet tweet in data.Tweets)
                {
                    if (tweet.RetweetCount > 0)
                    {
                        tweet.RetweetedUsers = GetRetweeterFollowers(tweet.StatusID, twitterDetails);
                    }
                }
            }

            return data == null ? new TwitterData(new List<Status>(),
                tweets.OrderBy(x => x.CreatedAt.Date).First().User.FollowersCount)
                : data;
        }

        public TwitterData GetTweets(string userId)
        {
            TwitterCredentials creds = _twitterReadRepo.ReadSpecific(userId.ToString());
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

            _userHandler.UpdateUserMediaActivation(new Models.User(Guid.Parse(userId)), MediaName);

            return Task.Factory.StartNew(() => GetMediaData(creds.UserId, DateHelper.GetDates(DateTime.Now.AddMonths(-1).Date, DateTime.Now.Date)));
        }

        public void GetMediaData(Guid userId, DateTime[] dates)
        {
            TwitterCredentials creds = _twitterReadRepo.ReadSpecific(userId.ToString());

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
    }
}