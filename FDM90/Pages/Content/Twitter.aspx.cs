using FDM90.Handlers;
using FDM90.Singleton;
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
        private static readonly HttpClient client = new HttpClient();

        public Twitter():this(new TwitterHandler())
        {

        }

        public Twitter(ITwitterHandler twitterHandler)
        {
            _twitterHandler = twitterHandler;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if(!Page.IsPostBack)
            {
                if (!UserSingleton.Instance.CurrentUser.Twitter)
                {
                    if (!string.IsNullOrWhiteSpace(Request.QueryString["oauth_verifier"]))
                    {
                        _twitterHandler.SaveUserDetails(Request.QueryString["oauth_verifier"], UserSingleton.Instance.CurrentUser.UserId.ToString());
                        UserSingleton.Instance.CurrentUser.Twitter = true;
                        _twitterHandler.GetTweets(UserSingleton.Instance.CurrentUser.UserId.ToString());
                    }
                    else
                    {
                        Response.Redirect(_twitterHandler.GetRedirectUrl());
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