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
        private IFacebookClientWrapper _facebookClientWrapper;

        public FacebookHandler() : this(new FacebookRepository(), new UserHandler(), new FacebookClientWrapper())
        {

        }

        public FacebookHandler(IRepository<FacebookCredentials> facebookRepo, IUserHandler userHandler,
            IFacebookClientWrapper facebookClientWrapper)
        {
            _facebookRepo = facebookRepo;
            _facebookReadRepo = (IReadSpecific<FacebookCredentials>) facebookRepo;
            _userHandler = userHandler;
            _facebookClientWrapper = facebookClientWrapper;
        }

        public FacebookCredentials GetLogInDetails(Guid userId)
        {
            var result = _facebookReadRepo.ReadSpecific(userId.ToString());

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

        public FacebookData GetLikeFacebookData(FacebookData currentData)
        {
            dynamic facebookData =
                _facebookClientWrapper.GetData(FacebookHelper.UrlBuilder(FacebookParameters.Field, "", new string[]
                    {
                        FacebookHelper.Id, FacebookHelper.Name, FacebookHelper.FanCount,
                        FacebookHelper.TalkingAboutCount
                    }),
                    currentData.AccessToken);

            return JsonHelper.Parse(facebookData, currentData);
        }

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
    }
}