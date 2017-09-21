using FDM90.Handlers;
using FDM90.Models;
using FDM90.Singleton;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FDM90.Pages.Content
{
    [ExcludeFromCodeCoverage]
    public partial class Facebook : System.Web.UI.Page
    {
        IFacebookHandler _facebookHandler;
        static FacebookCredentials facebookCreds;
        static FacebookData _facebookData;
        private string _likeDefault = "Likes: ";
        private string _postDefault = "Your Posts: ";
        private string _talkingDefault = "People Talking: ";
        private string _newLikesDefault = "Number of new likes: ";
        private string[] imageSuffixes = new string[] { "jpg", "png" };

        public Facebook() : this(new FacebookHandler())
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
                if (!UserSingleton.Instance.CurrentUser.Facebook)
                {
                    if (!string.IsNullOrWhiteSpace(Request.QueryString["code"]))
                    {
                        Task refreshTask = _facebookHandler.SetAccessToken(Request.QueryString["code"],
                                                                    facebookCreds.UserId, facebookCreds.PageName);

                        refreshTask.ContinueWith((response) =>
                        {
                            if (string.IsNullOrWhiteSpace((response as Task<string>)?.Result))
                            {
                                GetFacebookData(false);
                                facebookDetailsErrorLabel.Text = "";
                            }
                            else
                            {
                                detailsPanel.Visible = true;
                                facebookDetailsErrorLabel.Text = (response as Task<string>).Result;
                            }
                        });

                        UserSingleton.Instance.CurrentUser.Facebook = true;
                        GetFacebookData(true);
                    }
                }
                else
                {
                    facebookCreds = _facebookHandler.GetLogInDetails(UserSingleton.Instance.CurrentUser.UserId);
                    inputPageName.Text = facebookCreds.PageName;

                    if (!string.IsNullOrWhiteSpace(facebookCreds.PermanentAccessToken) && !facebookCreds.PermanentAccessToken.StartsWith("https://www."))
                    {
                        GetFacebookData(true);
                    }
                }
            }
        }

        private void GetFacebookData(bool updateUi)
        {
            _facebookData = _facebookHandler.GetFacebookData(facebookCreds.UserId);

            if (updateUi)
            {
                facebookUpdateTimer_Tick(new object(), new EventArgs());
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

            if (likesDetails.Visible)
            {
                newLikeLabel.Text = _newLikesDefault + _facebookData.NewLikeCount.ToString();
                likeListView.DataSource = _facebookData.PageLikes.Values;
                likeListView.DataBind();
                likesDetails.Visible = true;
            }
        }

        protected void postsButton_Click(object sender, EventArgs e)
        {
            posts.Visible = !posts.Visible;

            if (posts.Visible)
            {
                postList.DataSource = _facebookData.Posts;
                postList.DataBind();
            }
        }

        protected void facebookUpdateTimer_Tick(object sender, EventArgs e)
        {
            facebookData.Visible = _facebookData != null;

            if (_facebookData != null)
            {
                likesButton.Text = _likeDefault + _facebookData.FanCount;
                peopleTalkingLabel.Text = _talkingDefault + _facebookData.TalkingAboutCount.ToString();
                postsButton.Text = _postDefault + string.Format("({0})", _facebookData.Posts.Count);
                postList.DataSource = _facebookData.Posts;
                postList.DataBind();
            }
        }

        protected void PostButton_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> facebookParameters = new Dictionary<string, string>();
            facebookParameters.Add("message", FacebookPostText.Text);

            if (FacebookPostAttachement.HasFile)
            {
                if (imageSuffixes.Contains(FacebookPostAttachement.FileName.Substring(FacebookPostAttachement.FileName.LastIndexOf('.') + 1)))
                {
                    FacebookPostAttachement.SaveAs(ConfigSingleton.Instance.FileSaveLocation + FacebookPostAttachement.FileName);
                    facebookParameters.Add("picture", ConfigSingleton.Instance.FileSaveLocation + FacebookPostAttachement.FileName);
                }
            }

            _facebookHandler.PostData(facebookParameters, UserSingleton.Instance.CurrentUser.UserId);

            GetFacebookData(true);
        }

        protected void postList_ItemEditing(object sender, ListViewEditEventArgs e)
        {
            postList.EditIndex = e.NewEditIndex;
            FacebookPostData edittingPost = null;
            var label = (postList.Items[e.NewEditIndex].FindControl("PostIdLabel")) as Label;
            if (label != null && _facebookData.Posts.Any(x => x.Id.Equals(label.Text)))
                edittingPost = _facebookData.Posts.First(x => x.Id.Equals(label.Text));

            postList.DataSource = _facebookData.Posts;
            postList.DataBind();

            (postList.Items[postList.EditIndex].FindControl("MessagePostTextBox") as TextBox).Text = edittingPost.Message.ToString();
        }

        protected void postList_ItemCanceling(object sender, ListViewCancelEventArgs e)
        {
            postList.EditIndex = -1;
            postList.DataSource = _facebookData.Posts;
            postList.DataBind();
        }

        protected void postList_ItemUpdating(object sender, ListViewUpdateEventArgs e)
        {
            Dictionary<string, string> facebookParameters = new Dictionary<string, string>();
            facebookParameters.Add("id", (postList.Items[postList.EditIndex].FindControl("PostIdLabel") as Label).Text);
            facebookParameters.Add("message", (postList.Items[postList.EditIndex].FindControl("MessagePostTextBox") as TextBox).Text);

            _facebookHandler.PostData(facebookParameters, UserSingleton.Instance.CurrentUser.UserId);
            postList.EditIndex = -1;
            GetFacebookData(true);
        }

        protected void postList_ItemDeleting(object sender, ListViewDeleteEventArgs e)
        {
            Dictionary<string, string> facebookParameters = new Dictionary<string, string>();
            facebookParameters.Add("id", (postList.Items[e.ItemIndex].FindControl("PostIdLabel") as Label).Text);

            _facebookHandler.PostData(facebookParameters, UserSingleton.Instance.CurrentUser.UserId);
            GetFacebookData(true);
        }
    }
}