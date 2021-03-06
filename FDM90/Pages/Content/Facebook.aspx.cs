﻿using FDM90.Handlers;
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
                if (UserSingleton.Instance.CurrentUser == null) Response.Redirect("~/Pages/Content/Home.aspx");

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
                                signInArea.Visible = false;
                                facebookDetailsErrorLabel.Text = "";
                            }
                            else
                            {
                                signInArea.Visible = true;
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
                        signInArea.Visible = false;
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

        protected void postsButton_Click(object sender, EventArgs e)
        {
            postList.DataSource = _facebookData.Posts;
            postList.DataBind();
        }

        protected void facebookUpdateTimer_Tick(object sender, EventArgs e)
        {
            facebookData.Visible = _facebookData != null;

            if (_facebookData != null)
            {
                numberOfPageLikes.Text = _facebookData.FanCount.ToString();
                numberOfNewLikes.Text = _facebookData.PageLikes.Values.Where(x => x.EndTime.Date >= DateTime.Now.AddDays(-7).Date && x.EndTime.Date <= DateTime.Now.Date).Sum(s => s.Value).ToString();
                numberOfTalkingAbout.Text = _facebookData.PageStories.Values.Where(x => x.EndTime.Date >= DateTime.Now.AddDays(-7).Date && x.EndTime.Date <= DateTime.Now.Date).Sum(s => s.Value).ToString();

                var postInThePastSevenDays = _facebookData.Posts.Where(x => x.CreatedTime.Date >= DateTime.Now.AddDays(-7).Date && x.CreatedTime.Date <= DateTime.Now.Date);
                if (postInThePastSevenDays.Count() > 0)
                {
                    numberOfPostLikes.Text = postInThePastSevenDays.Sum(s => s.Likes?.Count).ToString();
                    numberOfPostComments.Text = postInThePastSevenDays.Sum(s => s.Comments?.Count).ToString();
                    mostInteractedDay.Text = postInThePastSevenDays.GroupBy(x => x.CreatedTime.Date)
                                                    .Select(x => new { EngagementCount = x.Sum(y => y.Likes?.Count) + x.Sum(y => y.Comments?.Count), Date = x.Key })
                                                                    .OrderByDescending(x => x.EngagementCount).First().Date.DayOfWeek.ToString();
                    mostInteractedHour.Text = postInThePastSevenDays.GroupBy(x => x.CreatedTime.Hour)
                                                    .Select(x => new { EngagementCount = x.Sum(y => y.Likes?.Count) + x.Sum(y => y.Comments?.Count), Hour = x.Key })
                                                            .OrderByDescending(x => x.EngagementCount).First().Hour + ":00";
                }

                postList.DataSource = _facebookData.Posts.OrderByDescending(x => x.CreatedTime);
                postList.DataBind();
            }
        }

        protected void PostButton_Click(object sender, EventArgs e)
        {
            string errorMessage = _facebookHandler.CheckPostText(FacebookPostText.Text, _facebookHandler.MediaName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                PostFacebookError.Visible = true;
                PostFacebookError.Text = errorMessage;
                return;
            }
            else
            {
                PostFacebookError.Visible = false;
                PostFacebookError.Text = string.Empty;
            }

            Dictionary<string, string> facebookParameters = new Dictionary<string, string>();
            facebookParameters.Add("message", FacebookPostText.Text);

            if (FacebookPostAttachement.HasFile)
            {
                if (imageSuffixes.Contains(FacebookPostAttachement.FileName.Substring(FacebookPostAttachement.FileName.LastIndexOf('.') + 1)))
                {
                    string fileName = UserSingleton.Instance.CurrentUser.UserId.ToString() + "_" + DateTime.Now.ToString().Replace('/','-').Replace(':','-') 
                                                + FacebookPostAttachement.FileName.Substring(FacebookPostAttachement.FileName.LastIndexOf('.'));
                    FacebookPostAttachement.SaveAs(Server.MapPath(ConfigSingleton.Instance.FileSaveLocation + fileName));
                    facebookParameters.Add("picture", ConfigSingleton.Instance.FileSaveLocation + fileName);
                }
            }

            _facebookHandler.PostData(facebookParameters, UserSingleton.Instance.CurrentUser.UserId);
            FacebookPostText.Text = string.Empty;
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
            string errorMessage = 
                    _facebookHandler.CheckPostText((postList.Items[postList.EditIndex].FindControl("PostIdLabel") as Label).Text, _facebookHandler.MediaName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                (postList.Items[postList.EditIndex].FindControl("EditingErrorLabel") as Label).Visible = true;
                (postList.Items[postList.EditIndex].FindControl("EditingErrorLabel") as Label).Text = errorMessage;
                return;
            }
            else
            {
                (postList.Items[postList.EditIndex].FindControl("EditingErrorLabel") as Label).Visible = false;
                (postList.Items[postList.EditIndex].FindControl("EditingErrorLabel") as Label).Text = string.Empty;
            }


            Dictionary<string, string> facebookParameters = new Dictionary<string, string>();
            facebookParameters.Add("id", (postList.Items[postList.EditIndex].FindControl("PostIdLabel") as Label).Text);
            facebookParameters.Add("message", (postList.Items[postList.EditIndex].FindControl("MessagePostTextBox") as TextBox).Text);

            _facebookHandler.PostData(facebookParameters, UserSingleton.Instance.CurrentUser.UserId);
            (postList.Items[postList.EditIndex].FindControl("MessagePostTextBox") as TextBox).Text = string.Empty;
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