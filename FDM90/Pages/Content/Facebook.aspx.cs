using FDM90.Handlers;
using FDM90.Models;
using FDM90.Singleton;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FDM90.Pages.Content
{
    public partial class Facebook : System.Web.UI.Page
    {
        IFacebookHandler _facebookHandler;
        FacebookCredentials facebookCreds;
        static FacebookData _facebookData;

        public Facebook():this(new FacebookHandler())
        {

        }

        public Facebook(IFacebookHandler facebookHandler)
        {
            _facebookHandler = facebookHandler;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                facebookCreds = _facebookHandler.GetLogInDetails(UserSingleton.Instance.CurrentUser.UserId);
                inputPageName.Text = facebookCreds.PageName;

                if (!string.IsNullOrWhiteSpace(facebookCreds.PermanentAccessToken) && !facebookCreds.PermanentAccessToken.StartsWith("https://www."))
                {
                    facebookData.Visible = true;
                    _facebookData = _facebookHandler.GetFacebookData(facebookCreds.UserId);
                    likesButton.Text += _facebookData.FanCount;
                    peopleTalkingLabel.Text += _facebookData.TalkingAboutCount.ToString();
                    postsButton.Text += string.Format("({0})", _facebookData.Posts.Count);
                }
                else if (!string.IsNullOrWhiteSpace(Request.QueryString["code"]))
                {
                    facebookCreds.PermanentAccessToken = _facebookHandler.SetAccessToken(Request.QueryString["code"],
                                                                facebookCreds.UserId, facebookCreds.PageName);
                }
            }
        }

        protected void facebookLoginButton_Click(object sender, EventArgs e)
        {
            facebookCreds = _facebookHandler.SaveLogInDetails(UserSingleton.Instance.CurrentUser.UserId, inputPageName.Text);
            Response.Redirect(facebookCreds.PermanentAccessToken);
        }

        protected void DetailsButton_Click(object sender, EventArgs e)
        {
            detailsPanel.Visible = !detailsPanel.Visible;
        }

        protected void likesButton_Click(object sender, EventArgs e)
        {
            likesDetails.Visible = !likesDetails.Visible;

            if(likesDetails.Visible)
            {
                newLikeLabel.Text += _facebookData.NewLikeCount.ToString();
                likeListView.DataSource = _facebookData.PageLikes.Values;
                likeListView.DataBind();
                likesDetails.Visible = true;
            }
        }

        protected void postsButton_Click(object sender, EventArgs e)
        {
            posts.Visible = !posts.Visible;

            if(posts.Visible)
            {
                postList.DataSource = _facebookData.Posts;
                postList.DataBind();
            }
        }
    }
}