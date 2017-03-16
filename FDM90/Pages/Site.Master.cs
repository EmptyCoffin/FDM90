﻿using FDM90.Singleton;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FDM90.Pages
{
    public partial class Site : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (UserSingleton.Instance.CurrentUser != null)
            {
                Page.Master.FindControl("FacebookLinked").Visible = UserSingleton.Instance.CurrentUser.FacebookLinked;
                Page.Master.FindControl("TwitterLinked").Visible = false;

                Label welcomeLabel = (Label)Page.Master.FindControl("welcomeMessage");
                welcomeLabel.Visible = true;
                welcomeLabel.Text = "Welcome " + UserSingleton.Instance.CurrentUser.UserName;
                Page.Master.FindControl("LogOut").Visible = true;
                Page.Master.FindControl("LogIn").Visible = false;
                Page.Master.FindControl("SignUp").Visible = false;
            }
            else
            {
                Page.Master.FindControl("FacebookLinked").Visible = false;
                Page.Master.FindControl("TwitterLinked").Visible = false;

                Page.Master.FindControl("LogOut").Visible = false;
                Page.Master.FindControl("LogIn").Visible = true;
                Page.Master.FindControl("SignUp").Visible = true;
            }
        }
    }
}
