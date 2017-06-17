using FDM90.Handlers;
using FDM90.Singleton;
using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FDM90.Pages.Content
{
    public partial class Twitter : System.Web.UI.Page
    {
        ITwitterHandler _twitterHandler;
        private AspNetAuthorizer _auth;

        private static readonly HttpClient client = new HttpClient();

        public Twitter():this(new TwitterHandler())
        {

        }

        public Twitter(ITwitterHandler twitterHandler)
        {
            _twitterHandler = twitterHandler;
        }

        protected async void Page_Load(object sender, EventArgs e)
        {
            _auth = new AspNetAuthorizer
            {
                CredentialStore = new SessionStateCredentialStore
                {
                    ConsumerKey = ConfigSingleton.TwitterConsumerKey,
                    ConsumerSecret = ConfigSingleton.TwitterConsumerSecret
                },
                GoToTwitterAuthorization =
                    twitterUrl => Response.Redirect(twitterUrl, false)
            };

            if (!Page.IsPostBack)
            {
                if (!UserSingleton.Instance.CurrentUser.Twitter)
                {
                    if (!string.IsNullOrWhiteSpace(Request.QueryString["oauth_token"]))
                    {
                        await _auth.CompleteAuthorizeAsync(Request.Url);
                        var credentials = _auth.CredentialStore;
                        _twitterHandler.SaveUserDetails(credentials.OAuthToken, credentials.OAuthTokenSecret, credentials.ScreenName, UserSingleton.Instance.CurrentUser.UserId.ToString());
                        UserSingleton.Instance.CurrentUser.Twitter = true;
                        _twitterHandler.GetTweets(UserSingleton.Instance.CurrentUser.UserId.ToString());
                    }
                    else
                    {
                        await _auth.BeginAuthorizeAsync(Request.Url);
                    }
                }
                else
                {
                    _twitterHandler.GetTweets(UserSingleton.Instance.CurrentUser.UserId.ToString());
                }
            }
        }
    }
}