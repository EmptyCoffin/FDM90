using FDM90.Handlers;
using FDM90.Singleton;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FDM90.Pages.Account
{
    [ExcludeFromCodeCoverage]
    public partial class UserProfile : System.Web.UI.Page
    {
        private ICampaignHandler _campaignHandler;
        private ISchedulerHandler _schedulerHandler;
        private IFacebookHandler _facebookHandler;
        private ITwitterHandler _twitterHandler;
        private IUserHandler _userHandler;
        private List<IMediaHandler> _mediaHandlers = new List<IMediaHandler>();

        public UserProfile() : this(new FacebookHandler(), new TwitterHandler(), new UserHandler(), new SchedulerHandler(), new CampaignHandler())
        {

        }

        public UserProfile(IFacebookHandler facebookHandler, ITwitterHandler twitterHandler, IUserHandler userHandler, ISchedulerHandler scheduleHandler, ICampaignHandler campaignHandler)
        {
            _schedulerHandler = scheduleHandler;
            _campaignHandler = campaignHandler;
            _facebookHandler = facebookHandler;
            _twitterHandler = twitterHandler;
            _userHandler = userHandler;
            _mediaHandlers.AddRange(new IMediaHandler[] { _facebookHandler, _twitterHandler });
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (UserSingleton.Instance.CurrentUser == null) Response.Redirect("~/Pages/Content/Home.aspx");

                List<string> implementedMedias = UserSingleton.Instance.CurrentUser.GetIntegratedMediaChannels();
                implementedMedias.Add("User Account");

                DeleteItemDropDownList.DataSource = implementedMedias;
                DeleteItemDropDownList.DataBind();
            }
        }

        protected void DeleteButton_Click(object sender, EventArgs e)
        {
            if(UserSingleton.Instance.CurrentUser.GetIntegratedMediaChannels().Contains(DeleteItemDropDownList.SelectedValue))
            {
                // delete social media
                UserSingleton.Instance.CurrentUser = _mediaHandlers.First(x => x.MediaName == DeleteItemDropDownList.SelectedValue).DeleteMedia(UserSingleton.Instance.CurrentUser.UserId);
                _campaignHandler.RemoveMediaAfterDelete(UserSingleton.Instance.CurrentUser.UserId, DeleteItemDropDownList.SelectedValue);
            }
            else
            {
                // delete user
                foreach(var mediaHandler in _mediaHandlers.Where(x => UserSingleton.Instance.CurrentUser.GetIntegratedMediaChannels().Contains(x.MediaName)))
                {
                    mediaHandler.DeleteMedia(UserSingleton.Instance.CurrentUser.UserId);
                }

                _schedulerHandler.DeleteScheduledPostForUser(UserSingleton.Instance.CurrentUser.UserId);
                _campaignHandler.DeleteForUser(UserSingleton.Instance.CurrentUser.UserId);

                _userHandler.DeleteUser(UserSingleton.Instance.CurrentUser.UserId);

                Response.Redirect("~/Pages/Account/Logout.aspx");
            }
        }
    }
}