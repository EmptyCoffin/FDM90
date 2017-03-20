using FDM90.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FDM90.Pages.Account
{
    public partial class Registration : System.Web.UI.Page
    {
        private IUserHandler _userHandler;

        public Registration():this(new UserHandler()) { }

        public Registration(IUserHandler userHandler)
        {
            _userHandler = userHandler;
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void registerButton_Click(object sender, EventArgs e)
        {
            _userHandler.RegisterUser(userNameTextBox.Text, inputEmailAddress.Text, passwordTextBox.Text);
            Response.Redirect("~/Pages/Content/Home.aspx");
        }
    }
}