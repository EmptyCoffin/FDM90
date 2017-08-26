using FDM90.Handlers;
using FDM90.Singleton;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FDM90.Pages.Content
{
    public partial class LinkedIn : System.Web.UI.Page
    {
        private ILinkedInHandler _linkedInHandler;

        public LinkedIn():this(new LinkedInHandler())
        {

        }

        public LinkedIn(ILinkedInHandler linkedInHandler)
        {
            _linkedInHandler = linkedInHandler;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (!UserSingleton.Instance.CurrentUser.LinkedIn)
                {
                    if (!string.IsNullOrWhiteSpace(Request.QueryString["code"]))
                    {
                        Task refreshTask = _linkedInHandler.SetAccessToken(UserSingleton.Instance.CurrentUser.UserId, Request.QueryString["code"]);

                        refreshTask.ContinueWith((response) => GetLinkedInData(false));

                        UserSingleton.Instance.CurrentUser.LinkedIn = true;
                        GetLinkedInData(true);
                    }
                    else
                    {
                        Response.Redirect(_linkedInHandler.GetLoginUrl());
                    }
                }
                else
                {
                    GetLinkedInData(true);
                }
            }
        }

        public void GetLinkedInData(bool updateUi)
        {

        }
    }
}