using Facebook;
using FDM90.Models;
using FDM90.Models.Helpers;
using FDM90.Repository;
using FDM90.Singleton;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FDM90.Handlers
{
    public class FacebookHandler : IFacebookHandler
    {
        private IReadSpecific<FacebookCredentials> _facebookReadRepo;
        private IRepository<FacebookCredentials> _facebookRepo;
        private IUserHandler _userHandler;

        public FacebookHandler():this(new FacebookRepository(), new UserHandler())
        {

        }

        public FacebookHandler(IRepository<FacebookCredentials> facebookRepo, IUserHandler userHandler)
        {
            _facebookRepo = facebookRepo;
            _facebookReadRepo = (IReadSpecific<FacebookCredentials>)facebookRepo;
            _userHandler = userHandler;
        }

        public FacebookCredentials GetLogInDetails(Guid userId)
        {
            //returning username, password, name of page?
            var result = _facebookReadRepo.ReadSpecific(userId.ToString());

            if(result == null || result.PermanentAccessToken == null)
            {
                result = result == null ? new FacebookCredentials() : result;
                result.PermanentAccessToken = GetLoginUrl();
            }

            //login
             return result;
        }

        public string GetLoginUrl()
        {
            var fbClient = new FacebookClient();

            dynamic login = fbClient.GetLoginUrl(new {
                client_id = ConfigSingleton.FacebookClientId,

                redirect_uri = "http://localhost:1900/Pages/Content/Facebook.aspx",

                response_type = "code",

                scope = "manage_pages,read_insights"
            });

            return login.AbsoluteUri;
        }

        public FacebookCredentials SaveLogInDetails(FacebookCredentials credentials)
        {
            try
            {
                _facebookRepo.Create(credentials);

                _userHandler.UpdateUserMediaActivation(new User(credentials.UserId), "Facebook");

                credentials.PermanentAccessToken = GetLoginUrl();
            }
            catch (Exception ex)
            {

            }

            return credentials;
        }

        public string SetAccessToken(string shortTermToken, Guid userId, string pageName)
        {
            var fbClient = new FacebookClient();

            //generate longer live token
            dynamic result = fbClient.Post("oauth/access_token", new
            {
                client_id = ConfigSingleton.FacebookClientId,
                client_secret = ConfigSingleton.FacebookClientSecret,
                redirect_uri = "http://localhost:1900/Pages/Content/Facebook.aspx",
                code = shortTermToken
            });

            //get user id
            dynamic userFacebookId = fbClient.Get(FacebookHelper.UrlBuilder(FacebookParameters.Field, "", 
                                                new string[] { FacebookHelper.AccessToken }) + "=" + result.access_token);
            //get permanent token
            dynamic permanentTokenResponse = fbClient.Get(FacebookHelper.UrlBuilder(FacebookParameters.Account, userFacebookId.id,
                                                new string[] { result.access_token }));

            JsonArray permanentData = permanentTokenResponse.data;
            var pToken = permanentData.OfType<JsonObject>().Where(page => page["name"].ToString() == pageName.Trim()).First();

            //save token to user
            _facebookRepo.Update(new FacebookCredentials() { UserId = UserSingleton.Instance.CurrentUser.UserId, PermanentAccessToken = pToken["access_token"].ToString() });

            return pToken["access_token"].ToString();
        }

        public FacebookData GetInitialFacebookData(string accessToken)
        {
            var fbClient = new FacebookClient(accessToken);

            dynamic facebookData = fbClient.Get(FacebookHelper.UrlBuilder(FacebookParameters.Field, "", new string[] 
                            { FacebookHelper.Id, FacebookHelper.Name, FacebookHelper.FanCount, FacebookHelper.TalkingAboutCount, FacebookHelper.Posts }));

            FacebookData data = JsonHelper.Parse(facebookData, new FacebookData());

            data.AccessToken = accessToken;

            return data;
        }

        public FacebookData GetLikeFacebookData(FacebookData currentData)
        {
            var fbClient = new FacebookClient(currentData.AccessToken);

            dynamic facebookData = fbClient.Get(FacebookHelper.UrlBuilder(FacebookParameters.Field, "", new string[]
                            { FacebookHelper.Id, FacebookHelper.Name, FacebookHelper.FanCount, FacebookHelper.TalkingAboutCount }));

            return JsonHelper.Parse(facebookData, currentData);
        }

        public FacebookData GetPostDetails(FacebookData currentData)
        {
            var fbClient = new FacebookClient(currentData.AccessToken);
            List<FacebookPostData> updatedPosts = new List<FacebookPostData>();
            foreach (FacebookPostData post in currentData.Posts)
            {
                dynamic postData = fbClient.Get(FacebookHelper.UrlBuilder(FacebookParameters.Insight, post.Id, new string[]
                                { FacebookHelper.PostDetails }));

                updatedPosts.Add(JsonHelper.Parse(postData.data, post));
            }

            currentData.Posts = updatedPosts;

            return currentData;
        }
    }
}