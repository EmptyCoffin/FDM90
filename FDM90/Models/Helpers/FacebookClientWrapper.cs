using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Facebook;
using FDM90.Singleton;
using System.IO;
using System.Dynamic;
using System.Diagnostics.CodeAnalysis;

namespace FDM90.Models.Helpers
{
    [ExcludeFromCodeCoverage]
    public class FacebookClientWrapper : IFacebookClientWrapper
    {
        private string _accessToken = string.Empty;
        private FacebookClient _fbClient => string.IsNullOrEmpty(_accessToken) ? new FacebookClient() :  new FacebookClient(_accessToken);

        public string GetLoginUrl()
        {
            dynamic login = _fbClient.GetLoginUrl(new
            {
                client_id = ConfigSingleton.Instance.FacebookClientId,

                redirect_uri = "http://localhost:1900/Pages/Content/Facebook.aspx",

                response_type = "code",

                scope = "manage_pages,publish_pages,read_insights,publish_actions,publish_pages"
            });

            return login.AbsoluteUri;
        }

        public string GetPermanentAccessToken(string shortTermToken, string pageName)
        {
            //generate longer live token
            dynamic result = _fbClient.Post(FacebookHelper.PostAuthParameter, new
            {
                client_id = ConfigSingleton.Instance.FacebookClientId,
                client_secret = ConfigSingleton.Instance.FacebookClientSecret,
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

        public dynamic PostData(Dictionary<string, string> postParameters, string accessToken)
        {
            _accessToken = accessToken;

            dynamic pageId = _fbClient.Get(FacebookHelper.UrlBuilder(FacebookParameters.Id, "", new string[] { FacebookHelper.Id }));
            string postPath = "feed";
            dynamic parameters = new ExpandoObject();

            if(postParameters.ContainsKey("picture"))
            {
                var mediaObject = new FacebookMediaStream();
                mediaObject.ContentType = "image/" + postParameters["picture"].Substring(postParameters["picture"].LastIndexOf('.') + 1);
                mediaObject.FileName = postParameters["picture"].Substring(postParameters["picture"].LastIndexOf('\\') + 1);
                mediaObject.SetValue(File.OpenRead(postParameters["picture"]));
                postPath = "photos";
                parameters.source = mediaObject;
            }

            if (postParameters.ContainsKey("id"))
            {
                parameters.id = postParameters["id"];
            }

            if(postParameters.ContainsKey("message"))
            {
                parameters.message = postParameters["message"];
            }

            if(postParameters.ContainsKey("id"))
            {
                if(postParameters.ContainsKey("message"))
                {
                    return _fbClient.Post(parameters.id, parameters);
                }
                else
                {
                    return _fbClient.Delete(parameters.id);
                }
            }
            else
            {
                return _fbClient.Post(pageId.id + "/" + postPath, parameters);
            }
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