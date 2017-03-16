using FDM90.Singleton;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FDM90.Pages.Content
{
    public partial class Home : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if(UserSingleton.Instance.CurrentUser != null)
            {
                facebookSetUpButton.Visible = !UserSingleton.Instance.CurrentUser.FacebookLinked;
            }
        }

        protected void facebookSetUpButton_Click(object sender, EventArgs e)
        {
            Response.Redirect("Facebook.aspx");
        }
    }
}