using FDM90.Models;
using FDM90.Repository;
using FDM90.Singleton;
using System;
using System.Collections.Generic;
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
    }
}