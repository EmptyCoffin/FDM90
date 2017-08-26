using FDM90.Singleton;
using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace FDM90.Models.Helpers
{
    public class TwitterClientWrapper : ITwitterClientWrapper
    {

        public async Task<List<Status>> GetTweets(TwitterCredentials twitterDetails)
        {
            TwitterContext context = await SetContext(twitterDetails);

            CheckRateLimit(context, TwitterHelper.StatusLimit, TwitterHelper.UserTimelineUrl);

            return (from tweet in context.Status
                    where tweet.Type == StatusType.User
                       && tweet.ScreenName == twitterDetails.ScreenName
                       && tweet.Count == 200
                    select tweet)
                .ToListAsync().Result.Where(x => !x.Text.StartsWith("RT @", StringComparison.CurrentCulture)).ToList();
        }

        public async Task<Status> PostTweet(TwitterCredentials twitterDetails, Dictionary<string, string> postParameters)
        {
            TwitterContext context = await SetContext(twitterDetails);

            while (true)
            {
                try
                {
                    if (postParameters.ContainsKey("picture"))
                    {
                        var media = await context.UploadMediaAsync(File.ReadAllBytes(postParameters["picture"]),
                                        "image/" + postParameters["picture"].Substring(postParameters["picture"].LastIndexOf('.') + 1), "tweet_image");

                        return context.TweetAsync(postParameters["message"], new ulong[] { media.MediaID }).Result;
                    }
                    else
                    {
                        return context.TweetAsync(postParameters["message"]).Result;
                    }
                }
                catch (Exception ex)
                {
                    Thread.Sleep((int)TimeSpan.FromMinutes(5).TotalMilliseconds);
                }
            }
        }

        public async Task<List<Status>> GetRetweeterFollowers(TwitterCredentials twitterDetails, ulong statusId)
        {
            TwitterContext context = await SetContext(twitterDetails);

            CheckRateLimit(context, TwitterHelper.StatusLimit, TwitterHelper.RetweetUrl);

            // for loop ? 
            return (from tweet in context.Status
                    where tweet.Type == StatusType.Retweets
                        && tweet.ID == statusId
                       && tweet.Count == 200
                    select tweet)
                .ToListAsync().Result;
        }

        private static void CheckRateLimit(TwitterContext context, string rateLimitKey, string resourceKey)
        {
            var helpResult =
                (from help in context.Help
                 where help.Type == HelpType.RateLimits

                 select help)
                .SingleOrDefault();

            while (helpResult.RateLimits[rateLimitKey].First(x => x.Resource == resourceKey).Remaining == 0)
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var test = epoch.AddSeconds(helpResult.RateLimits[rateLimitKey].First(x => x.Resource == resourceKey).Reset);

                if (TimeZoneInfo.Local.IsDaylightSavingTime(test))
                    test = test.AddHours(1);

                var aotherTest = test.Subtract(DateTime.Now).Minutes + 1;

                Thread.Sleep((int)TimeSpan.FromMinutes(test.Subtract(DateTime.Now).Minutes + 1).TotalMilliseconds);
                helpResult =
                (from help in context.Help
                 where help.Type == HelpType.RateLimits

                 select help)
                .SingleOrDefault();
            }
        }

        private static async Task<TwitterContext> SetContext(TwitterCredentials twitterDetails)
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
            return context;
        }
    }
}