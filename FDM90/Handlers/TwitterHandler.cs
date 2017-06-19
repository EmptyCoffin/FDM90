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

namespace FDM90.Handlers
{
    public class TwitterHandler : ITwitterHandler
    {
        private IReadSpecific<TwitterCredentials> _twitterReadRepo;
        private IRepository<TwitterCredentials> _twitterRepo;
        private IUserHandler _userHandler;
        static DateTimeFormatInfo dateInfo = DateTimeFormatInfo.CurrentInfo;
        Calendar calendar = dateInfo.Calendar;

        public TwitterHandler() : this(new TwitterRepository(), new UserHandler())
        {

        }

        public TwitterHandler(IRepository<Models.TwitterCredentials> twitterRepo, IUserHandler userHandler)
        {
            _twitterRepo = twitterRepo;
            _twitterReadRepo = (IReadSpecific<Models.TwitterCredentials>)twitterRepo;
            _userHandler = userHandler;
        }


        public string MediaName
        {
            get
            {
                return "Twitter";
            }
        }

        public IJEnumerable<JToken> GetGoalInfo(Guid userId, DateTime startDate, DateTime endDate)
        {
            JObject twitterTargets = new JObject();
            DateTimeFormatInfo dateInfo = DateTimeFormatInfo.CurrentInfo;
            Calendar calendar = dateInfo.Calendar;

            // get user twitter
            var twitterDetails = _twitterReadRepo.ReadSpecific(userId.ToString());

            Task<List<Status>>[] tasks = new Task<List<Status>>[1];

            tasks[0] = Task.Factory.StartNew(() => GetTwitterData(twitterDetails, startDate, endDate).Result);

            return Task.Factory.ContinueWhenAll(tasks, data =>
            {
                var tweets = data[0].Result;
                int screenNameFollowerCount = tweets.Where(x => x.ScreenName == twitterDetails.ScreenName).First().User.FollowersCount;
                // get exposure - followers and followers of those retweeted/favorited
                foreach (var tweetDate in tweets.Where(w => w.CreatedAt.Date.AddDays(7) < DateTime.Now.Date && !w.Retweeted && !w.Favorited).GroupBy(x => x.CreatedAt.Date))
                {
                    var numberOfMessages = tweetDate.Count();
                    var numberOfFollowers = screenNameFollowerCount;

                    foreach (Status tweet in tweetDate.Where(y => y.RetweetCount > 0))
                    {
                        // get retweeters followers
                        numberOfFollowers = numberOfFollowers + GetRetweeterFollowers(tweet.StatusID, twitterDetails).Result;
                    }

                    int weekNumber = calendar.GetWeekOfYear(tweetDate.First().CreatedAt, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);
                    JObject week = new JObject();
                    // add to object / update object 
                    JToken weekExisting;

                    double estimatedExposure = (((double)numberOfFollowers * numberOfMessages) * (numberOfMessages / (double)numberOfFollowers));

                    if (!twitterTargets.TryGetValue("Week" + weekNumber.ToString(), out weekExisting))
                    {
                        twitterTargets.Add("Week" + weekNumber, week);
                    }

                    JToken existingValue;
                    if (((JObject)twitterTargets.GetValue("Week" + weekNumber)).TryGetValue("Exposure", out existingValue))
                    {
                        ((JObject)twitterTargets.GetValue("Week" + weekNumber)).GetValue("Exposure").Replace(int.Parse(existingValue.ToString()) + (int)estimatedExposure);
                    }
                    else
                    {
                        ((JObject)twitterTargets.GetValue("Week" + weekNumber)).Add("Exposure", (int)estimatedExposure);
                    }
                }

                // get influence - followers of those retweeted/favorited
                foreach (var tweetDate in tweets.Where(w => w.CreatedAt.Date.AddDays(7) < DateTime.Now.Date && !w.Retweeted && !w.Favorited && w.FavoriteCount > 0 && w.RetweetCount > 0).GroupBy(x => x.CreatedAt.Date))
                {
                    var numberOfMessages = tweetDate.Count();
                    var numberOfFollowers = 0;

                    foreach (Status tweet in tweetDate.Where(y => y.RetweetCount > 0 || y.FavoriteCount > 0))
                    {
                        // get retweeters followers
                        numberOfFollowers = numberOfFollowers + GetRetweeterFollowers(tweet.StatusID, twitterDetails).Result;
                    }

                    int weekNumber = calendar.GetWeekOfYear(tweetDate.First().CreatedAt, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);
                    JObject week = new JObject();
                    // add to object / update object 
                    JToken weekExisting;

                    double estimatedInfluence = (((double)numberOfFollowers * numberOfMessages) * (numberOfMessages / (double)numberOfFollowers));

                    if (!twitterTargets.TryGetValue("Week" + weekNumber.ToString(), out weekExisting))
                    {
                        twitterTargets.Add("Week" + weekNumber, week);
                    }

                    JToken existingValue;
                    if (((JObject)twitterTargets.GetValue("Week" + weekNumber)).TryGetValue("Influence", out existingValue))
                    {
                        ((JObject)twitterTargets.GetValue("Week" + weekNumber)).GetValue("Influence").Replace(int.Parse(existingValue.ToString()) + (int)estimatedInfluence);
                    }
                    else
                    {
                        ((JObject)twitterTargets.GetValue("Week" + weekNumber)).Add("Influence", (int)estimatedInfluence);
                    }
                }

                // get engagement - replies/mentions, direct messages, retweets, hashtags mentions, favorited
                foreach (var datedTweets in tweets.Where(x => x.RetweetCount > 0 || x.FavoriteCount > 0).GroupBy(x => x.CreatedAt.Date))
                {
                    int weekNumber = calendar.GetWeekOfYear(datedTweets.First().CreatedAt, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);

                    var numberOfEngagements = 0;

                    foreach (Status tweet in datedTweets)
                    {
                        // get retweeters followers
                        numberOfEngagements = numberOfEngagements + tweet.FavoriteCount ?? 0 + tweet.RetweetCount;
                    }

                    JObject week = new JObject();
                    // add to object / update object 
                    JToken weekExisting;

                    if (!twitterTargets.TryGetValue("Week" + weekNumber.ToString(), out weekExisting))
                    {
                        twitterTargets.Add("Week" + weekNumber, week);
                    }

                    JToken existingValue;
                    if (((JObject)twitterTargets.GetValue("Week" + weekNumber)).TryGetValue("Engagement", out existingValue))
                    {
                        ((JObject)twitterTargets.GetValue("Week" + weekNumber)).GetValue("Engagement").Replace(int.Parse(existingValue.ToString()) + numberOfEngagements);
                    }
                    else
                    {
                        ((JObject)twitterTargets.GetValue("Week" + weekNumber)).Add("Engagement", numberOfEngagements);
                    }
                }
                return twitterTargets.Values();
            }).Result;
        }

        private async Task<int> GetRetweeterFollowers(ulong statusId, TwitterCredentials twitterDetails)
        {
            var auth = new SingleUserAuthorizer()
            {
                CredentialStore = new SingleUserInMemoryCredentialStore()
                {
                    ConsumerKey = ConfigSingleton.TwitterConsumerKey,
                    ConsumerSecret = ConfigSingleton.TwitterConsumerSecret,
                    OAuthToken = twitterDetails.AccessToken,
                    OAuthTokenSecret = twitterDetails.AccessTokenSecret
                }
            };

            await auth.AuthorizeAsync();

            TwitterContext context = new TwitterContext(auth);
            // retweeter ids
            var retweeters = await (from tweet in context.Status
                                    where tweet.Type == StatusType.Retweets
                                        && tweet.ID == statusId
                                       && tweet.Count == 200
                                    select tweet)
                .ToListAsync();

            int followerCount = 0;

            foreach (var status in retweeters)
            {
                if (status.User != null)
                {
                    followerCount += status.User.FollowersCount;
                }
            }

            return followerCount;
        }

        private async Task<List<Status>> GetTwitterData(TwitterCredentials twitterDetails, DateTime startDate, DateTime endDate)
        {
            var auth = new SingleUserAuthorizer()
            {
                CredentialStore = new SingleUserInMemoryCredentialStore()
                {
                    ConsumerKey = ConfigSingleton.TwitterConsumerKey,
                    ConsumerSecret = ConfigSingleton.TwitterConsumerSecret,
                    OAuthToken = twitterDetails.AccessToken,
                    OAuthTokenSecret = twitterDetails.AccessTokenSecret
                }
            };

            await auth.AuthorizeAsync();

            TwitterContext context = new TwitterContext(auth);
            var tweets = await (from tweet in context.Status
                                where tweet.Type == StatusType.User
                                   && tweet.ScreenName == twitterDetails.ScreenName
                                   && tweet.Count == 200
                                select tweet)
                            .ToListAsync();

            startDate = startDate.AddDays(-7).Date.Equals(DateTime.Now.Date) ? startDate : startDate.AddDays(-7);

            return tweets.Where(x => x.CreatedAt < endDate && x.CreatedAt > startDate).ToList();
        }

        public string GetTweets(string userId)
        {
            return string.Empty;
        }

        public void SaveUserDetails(string accessToken, string accessTokenSecret, string screenName, string userId)
        {
            _twitterRepo.Create(new TwitterCredentials() {
                UserId = Guid.Parse(userId),
                AccessToken = accessToken,
                AccessTokenSecret = accessTokenSecret,
                ScreenName = screenName
            });

            _userHandler.UpdateUserMediaActivation(new Models.User(Guid.Parse(userId)), "Twitter");
        }
    }
}