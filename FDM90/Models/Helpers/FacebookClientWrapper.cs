using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Facebook;
using FDM90.Singleton;

namespace FDM90.Models.Helpers
{
    public class FacebookClientWrapper : IFacebookClientWrapper
    {
        private string _accessToken = string.Empty;
        private FacebookClient _fbClient => string.IsNullOrEmpty(_accessToken) ? new FacebookClient() :  new FacebookClient(_accessToken);

        public string GetLoginUrl()
        {
            dynamic login = _fbClient.GetLoginUrl(new
            {
                client_id = ConfigSingleton.FacebookClientId,

                redirect_uri = "http://localhost:1900/Pages/Content/Facebook.aspx",

                response_type = "code",

                scope = "manage_pages,read_insights"
            });

            return login.AbsoluteUri;
        }

        public string GetPermanentAccessToken(string shortTermToken, string pageName)
        {
            //generate longer live token
            dynamic result = _fbClient.Post(FacebookHelper.PostAuthParameter, new
            {
                client_id = ConfigSingleton.FacebookClientId,
                client_secret = ConfigSingleton.FacebookClientSecret,
                redirect_uri = "http://localhost:1900/Pages/Content/Facebook.aspx",
                code = shortTermToken
            });

            //get user id
            dynamic userFacebookId = _fbClient.Get(FacebookHelper.UrlBuilder(FacebookParameters.Id, "",
                                                new string[] { FacebookHelper.AccessToken }) + "=" + result.access_token);
            //get permanent token
            dynamic permanentTokenResponse = _fbClient.Get(FacebookHelper.UrlBuilder(FacebookParameters.Account, userFacebookId.id,
                                                new string[] { result.access_token }));

            JsonArray permanentData = permanentTokenResponse.data;
            var pToken = permanentData.OfType<JsonObject>().FirstOrDefault(page => page["name"].ToString() == pageName.Trim());

            return pToken == null ? "Page Name Doesn't Exist or is Misspelt" : pToken["access_token"].ToString();
        }

        public dynamic GetData(string url, string accessToken)
        {
            _accessToken = accessToken;

            return _fbClient.Get(url);
        }

        public dynamic GetData(string url, string accessToken, object parameters)
        {
            _accessToken = accessToken;

            return _fbClient.Get(url, parameters);
        }
    }
}