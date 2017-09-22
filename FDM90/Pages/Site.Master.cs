using FDM90.Singleton;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace FDM90.Pages
{
    [ExcludeFromCodeCoverage]
    public partial class Site : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            List<HtmlGenericControl> tabs = new List<HtmlGenericControl>()
            {
                (HtmlGenericControl)Page.Master.FindControl("HomeTab"),
                (HtmlGenericControl)Page.Master.FindControl("FacebookTab"),
                (HtmlGenericControl)Page.Master.FindControl("TwitterTab"),
                (HtmlGenericControl)Page.Master.FindControl("LogInWelcomeMessageTab"),
                (HtmlGenericControl)Page.Master.FindControl("RegistrationLogoutTab"),
                (HtmlGenericControl)Page.Master.FindControl("CampaignsTab"),
                (HtmlGenericControl)Page.Master.FindControl("SchedulerTab")
            };
            
            var pageName = Page.AppRelativeVirtualPath.Substring(Page.AppRelativeVirtualPath.LastIndexOf('/') + 1, 
                            Page.AppRelativeVirtualPath.LastIndexOf('.') - (Page.AppRelativeVirtualPath.LastIndexOf('/') + 1));

            if(tabs.First(x => x.ID.ToLower().Contains(pageName.ToLower())).Attributes["class"] != null)
            {
                tabs.First(x => x.ID.ToLower().Contains(pageName.ToLower())).Attributes.Add("class", "active " 
                                                    + tabs.First(x => x.ID.ToLower().Contains(pageName.ToLower())).Attributes["class"].ToString());
            }
            else
            {
                tabs.First(x => x.ID.ToLower().Contains(pageName.ToLower())).Attributes.Add("class", "active");
            }

            tabs.First(x => !x.ID.ToLower().Contains(pageName.ToLower())).Attributes.Remove("class");

            if (UserSingleton.Instance.CurrentUser != null)
            {
                Page.Master.FindControl("Facebook").Visible = UserSingleton.Instance.CurrentUser.Facebook;
                Page.Master.FindControl("Twitter").Visible = UserSingleton.Instance.CurrentUser.Twitter;
                Page.Master.FindControl("Campaigns").Visible = UserSingleton.Instance.CurrentUser.Campaigns > 0;
                Page.Master.FindControl("Scheduler").Visible = UserSingleton.Instance.CurrentUser.Facebook || UserSingleton.Instance.CurrentUser.Twitter;

                Label welcomeLabel = (Label)Page.Master.FindControl("welcomeMessage");
                welcomeLabel.Visible = true;
                welcomeLabel.Text = "Welcome " + UserSingleton.Instance.CurrentUser.UserName;
                Page.Master.FindControl("LogOut").Visible = true;
                Page.Master.FindControl("LogIn").Visible = false;
                Page.Master.FindControl("SignUp").Visible = false;
            }
            else
            {
                Page.Master.FindControl("Facebook").Visible = false;
                Page.Master.FindControl("Twitter").Visible = false;
                Page.Master.FindControl("Campaigns").Visible = false;
                Page.Master.FindControl("Scheduler").Visible = false;
                Page.Master.FindControl("LogOut").Visible = false;
                Page.Master.FindControl("LogIn").Visible = true;
                Page.Master.FindControl("SignUp").Visible = true;
            }
        }
    }
}
