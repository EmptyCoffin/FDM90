using FDM90.Handlers;
using FDM90.Models;
using FDM90.Singleton;
using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FDM90.Pages.Content
{
    public partial class Twitter : System.Web.UI.Page
    {
        ITwitterHandler _twitterHandler;
        private AspNetAuthorizer _auth;
        private static TwitterData _data;
        private string _numberOfFollowersDefault = "Number of Followers: ";
        private string _numberOfRetweetsDefault = "Number of Retweets: ";
        private string _numberOfFavoriteDefault = "Number of Favorited: ";
        private string[] imageSuffixes = new string[] { "jpg", "png" };

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
            if (!Page.IsPostBack)
            {
                if (!UserSingleton.Instance.CurrentUser.Twitter)
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

                    if (!string.IsNullOrWhiteSpace(Request.QueryString["oauth_token"]))
                    {
                        await _auth.CompleteAuthorizeAsync(Request.Url);
                        var credentials = _auth.CredentialStore;
                        Task refreshTask = _twitterHandler.SaveUserDetails(credentials.OAuthToken, credentials.OAuthTokenSecret,
                                                                                credentials.ScreenName, UserSingleton.Instance.CurrentUser.UserId.ToString());
                        refreshTask.ContinueWith((response) => GetTwitterData(false));

                        UserSingleton.Instance.CurrentUser.Twitter = true;
                        GetTwitterData(true);
                    }
                    else
                    {
                        await _auth.BeginAuthorizeAsync(Request.Url);
                    }
                }
                else
                {
                    GetTwitterData(true);
                }
            }
        }

        private void GetTwitterData(bool updateUi)
        {
            _data = _twitterHandler.GetTweets(UserSingleton.Instance.CurrentUser.UserId.ToString());

            if(updateUi)
            {
                twitterUpdateTimer_Tick(new object(), new EventArgs());
            }
        }

        protected void twitterUpdateTimer_Tick(object sender, EventArgs e)
        {
            numberOfFollowers.Text = _numberOfFollowersDefault + _data.NumberOfFollowers.ToString();
            numberOfRetweets.Text = _numberOfRetweetsDefault + _data.NumberOfRetweets.ToString();
            numberOfFavorite.Text = _numberOfFavoriteDefault + _data.NumberOfFavorited.ToString();

            tweetList.DataSource = _data.Tweets;
            tweetList.DataBind();
        }

        protected void PostButton_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> twitterParameters = new Dictionary<string, string>();
            twitterParameters.Add("message", TwitterPostText.Text);

            if (TwitterPostAttachement.HasFile)
            {
                TwitterPostAttachement.SaveAs(ConfigSingleton.FileSaveLocation + TwitterPostAttachement.FileName);

                if (imageSuffixes.Contains(TwitterPostAttachement.FileName.Substring(TwitterPostAttachement.FileName.LastIndexOf('.') + 1)))
                {
                    twitterParameters.Add("picture", ConfigSingleton.FileSaveLocation + TwitterPostAttachement.FileName);
                }
            }

            _twitterHandler.PostData(twitterParameters, UserSingleton.Instance.CurrentUser.UserId);
        }
    }
}