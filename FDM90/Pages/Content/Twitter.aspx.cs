using FDM90.Handlers;
using FDM90.Models;
using FDM90.Singleton;
using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    [ExcludeFromCodeCoverage]
    public partial class Twitter : System.Web.UI.Page
    {
        ITwitterHandler _twitterHandler;
        private AspNetAuthorizer _auth;
        private static TwitterData _data;
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
                if (UserSingleton.Instance.CurrentUser == null) Response.Redirect("~/Pages/Content/Home.aspx");

                if (!UserSingleton.Instance.CurrentUser.Twitter)
                {
                    _auth = new AspNetAuthorizer
                    {
                        CredentialStore = new SessionStateCredentialStore
                        {
                            ConsumerKey = ConfigSingleton.Instance.TwitterConsumerKey,
                            ConsumerSecret = ConfigSingleton.Instance.TwitterConsumerSecret
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
            numberOfFollowers.Text = _data.NumberOfFollowers.ToString();
            numberOfRetweets.Text = _data.NumberOfRetweets.ToString();
            numberOfFavorite.Text =  _data.NumberOfFavorited.ToString();

            int? y = null;
            var numberOfFollowersValues = _data.NumberOfFollowersByDate.Where(x => x.Key.Date >= DateTime.Now.AddDays(-7).Date && x.Key.Date <= DateTime.Now.Date).OrderBy(x => x.Key).ToList();

            for (int i = 0; i < numberOfFollowersValues.Count; i++)
            {
                if (y == null)
                {
                    y = 0;
                }
                else
                {
                    y = numberOfFollowersValues[i].Value - numberOfFollowersValues[i - 1].Value;
                }

            }

            numberOfNewFollowers.Text = y.ToString();
            tweetList.DataSource = _data.Tweets.OrderByDescending(x => x.CreatedAt);
            tweetList.DataBind();
        }

        protected void PostButton_Click(object sender, EventArgs e)
        {
            string errorMessage = _twitterHandler.CheckPostText(TwitterPostText.Text, _twitterHandler.MediaName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                PostTwitterError.Visible = true;
                PostTwitterError.Text = errorMessage;
                return;
            }
            else
            {
                PostTwitterError.Visible = false;
                PostTwitterError.Text = string.Empty;
            }


            Dictionary<string, string> twitterParameters = new Dictionary<string, string>();
            twitterParameters.Add("message", TwitterPostText.Text);

            //if (TwitterPostAttachement.HasFile)
            //{
            //    if (imageSuffixes.Contains(TwitterPostAttachement.FileName.Substring(TwitterPostAttachement.FileName.LastIndexOf('.') + 1)))
            //    {
            //        TwitterPostAttachement.SaveAs(ConfigSingleton.Instance.FileSaveLocation + TwitterPostAttachement.FileName);
            //        twitterParameters.Add("picture", ConfigSingleton.Instance.FileSaveLocation + TwitterPostAttachement.FileName);
            //    }
            //}

            _twitterHandler.PostData(twitterParameters, UserSingleton.Instance.CurrentUser.UserId);
            TwitterPostText.Text = string.Empty;
            GetTwitterData(true);
        }

        protected void tweetList_ItemDeleting(object sender, ListViewDeleteEventArgs e)
        {
            Dictionary<string, string> twitterParameters = new Dictionary<string, string>();
            twitterParameters.Add("id", (tweetList.Items[e.ItemIndex].FindControl("StatusIdLabel") as Label).Text);

            _twitterHandler.PostData(twitterParameters, UserSingleton.Instance.CurrentUser.UserId);
            GetTwitterData(true);
        }
    }
}