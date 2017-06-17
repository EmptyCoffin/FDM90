using FDM90.Models;
using FDM90.Repository;
using FDM90.Singleton;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace FDM90.Handlers
{
    public class TwitterHandler : ITwitterHandler
    {
        private static IAuthenticationContext _authenticationContext;
        private IAuthenticatedUser _authenticatedUser;
        private IReadSpecific<Models.TwitterCredentials> _twitterReadRepo;
        private IRepository<Models.TwitterCredentials> _twitterRepo;
        private IUserHandler _userHandler;

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

        public string GetRedirectUrl()
        {
            var appCreds = new ConsumerCredentials(ConfigSingleton.TwitterConsumerKey, ConfigSingleton.TwitterConsumerSecret);

            // Specify the url you want the user to be redirected to
            _authenticationContext = AuthFlow.InitAuthentication(appCreds, "http://localhost:1900/Pages/Content/Twitter.aspx");

            return _authenticationContext.AuthorizationURL;
        }

        public void SaveUserDetails(string verifyString, string userId)
        {
            var userCreds = AuthFlow.CreateCredentialsFromVerifierCode(verifyString, _authenticationContext);

            // Do whatever you want with the user now!
            _authenticatedUser = Tweetinvi.User.GetAuthenticatedUser(userCreds);

            _twitterRepo.Create(new Models.TwitterCredentials() { UserId = Guid.Parse(userId), AccessToken = _authenticatedUser.Credentials.AccessToken, AccessTokenSecret = _authenticatedUser.Credentials.AccessTokenSecret });

            _userHandler.UpdateUserMediaActivation(new Models.User(Guid.Parse(userId)), "Twitter");
        }

        public string GetTweets(string userId)
        {
            var twitterDetails = _twitterReadRepo.ReadSpecific(userId);

            Auth.SetUserCredentials(ConfigSingleton.TwitterConsumerKey, ConfigSingleton.TwitterConsumerSecret, twitterDetails.AccessToken, twitterDetails.AccessTokenSecret);

            var homeTimelineParameters = new HomeTimelineParameters
            {
                MaximumNumberOfTweetsToRetrieve = 200
            };

            var tweets = Timeline.GetHomeTimeline(homeTimelineParameters);

            return "Test";
        }

        public IJEnumerable<JToken> GetGoalInfo(Guid userId, DateTime startDate, DateTime endDate)
        {
            JObject twitterTargets = new JObject();
            DateTimeFormatInfo dateInfo = DateTimeFormatInfo.CurrentInfo;
            Calendar calendar = dateInfo.Calendar;

            // get user twitter
            var twitterDetails = _twitterReadRepo.ReadSpecific(userId.ToString());

            Auth.SetUserCredentials(ConfigSingleton.TwitterConsumerKey, ConfigSingleton.TwitterConsumerSecret, twitterDetails.AccessToken, twitterDetails.AccessTokenSecret);
            var user = Tweetinvi.User.GetAuthenticatedUser();

            // get exposure - followers and followers of those retweeted/favorited
            var tweet2s = user.GetUserTimeline(250).Where(x => !x.Text.StartsWith("RT @"));
            foreach (var tweetDate in tweet2s.Where(w => w.CreatedAt.Date < DateTime.Now.Date).GroupBy(x => x.CreatedAt))
            {
                var numberOfMessages = tweetDate.Count();
                var numberOfFollowers = user.FollowersCount;

                foreach(ITweet tweet in tweetDate.Where(y => y.RetweetCount > 0))
                {
                    // get retweeters followers
                    // numberOfFollowers + 
                }

                int weekNumber = calendar.GetWeekOfYear(tweetDate.First().CreatedAt, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);
                JObject week = new JObject();
                // add to object / update object 
                JToken weekExisting;

                var estimatedExposure = ((numberOfFollowers * numberOfMessages) * (numberOfMessages / numberOfFollowers));

                if (!twitterTargets.TryGetValue("Week" + weekNumber.ToString(), out weekExisting))
                {
                    twitterTargets.Add("Week" + weekNumber, week);
                }

                JToken existingValue;
                if (((JObject)twitterTargets.GetValue("Week" + weekNumber)).TryGetValue("Exposure", out existingValue))
                {
                    ((JObject)twitterTargets.GetValue("Week" + weekNumber)).GetValue("Exposure").Replace(int.Parse(existingValue.ToString()) + estimatedExposure);
                }
                else
                {
                    ((JObject)twitterTargets.GetValue("Week" + weekNumber)).Add("Exposure", estimatedExposure);
                }
            }

            // get influence - followers of those retweeted/favorited
            foreach (var tweetDate in tweet2s.Where(w => w.CreatedAt.Date < DateTime.Now.Date).GroupBy(x => x.CreatedAt))
            {
                var numberOfMessages = tweetDate.Count();
                var numberOfFollowers = 0;

                foreach (ITweet tweet in tweetDate.Where(y => y.RetweetCount > 0))
                {
                    // get retweeters followers
                    // numberOfFollowers + 
                }

                int weekNumber = calendar.GetWeekOfYear(tweetDate.First().CreatedAt, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);
                JObject week = new JObject();
                // add to object / update object 
                JToken weekExisting;

                var estimatedInfluence = ((numberOfFollowers * numberOfMessages) * (numberOfMessages / numberOfFollowers));

                if (!twitterTargets.TryGetValue("Week" + weekNumber.ToString(), out weekExisting))
                {
                    twitterTargets.Add("Week" + weekNumber, week);
                }

                JToken existingValue;
                if (((JObject)twitterTargets.GetValue("Week" + weekNumber)).TryGetValue("Influence", out existingValue))
                {
                    ((JObject)twitterTargets.GetValue("Week" + weekNumber)).GetValue("Influence").Replace(int.Parse(existingValue.ToString()) + estimatedInfluence);
                }
                else
                {
                    ((JObject)twitterTargets.GetValue("Week" + weekNumber)).Add("Influence", estimatedInfluence);
                }
            }

            // get engagement - replies/mentions, direct messages, retweets, hashtags mentions, favorited
            foreach (var datedTweets in tweet2s.Where(x => x.RetweetCount > 0 || x.FavoriteCount > 0).GroupBy(x => x.CreatedAt))
            {
                int weekNumber = calendar.GetWeekOfYear(datedTweets.First().CreatedAt, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);

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
                    ((JObject)twitterTargets.GetValue("Week" + weekNumber)).GetValue("Engagement").Replace(int.Parse(existingValue.ToString()) + datedTweets.Count());
                }
                else
                {
                    ((JObject)twitterTargets.GetValue("Week" + weekNumber)).Add("Engagement", datedTweets.Count());
                }
            }

            return twitterTargets.Values();
        }
    }
}