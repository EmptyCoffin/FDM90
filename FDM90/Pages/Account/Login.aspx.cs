using FDM90.Handlers;
using FDM90.Models;
using FDM90.Singleton;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Web.Security;

namespace FDM90.Pages.Account
{
    [ExcludeFromCodeCoverage]
    public partial class Login : System.Web.UI.Page
    {
        private IUserHandler _userHandler;

        public Login():this(new UserHandler()) { }

        public Login(IUserHandler userHandler)
        {
            _userHandler = userHandler;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if(!Page.IsPostBack)
            {

            }
        }

        protected void LoginButton_Click(object sender, EventArgs e)
        {
            errorLabel.Text = "";
            var loginResult = _userHandler.LoginUser(new User(userNameTextBox.Text, passwordTextBox.Text));

            if (!loginResult.UserId.Equals(Guid.Empty))
            {
                UserSingleton.Instance.CurrentUser = loginResult;
                FormsAuthentication.SetAuthCookie(UserSingleton.Instance.CurrentUser.UserName, false);
                System.Threading.Thread.Sleep(3 * 1000);
                Response.Redirect("~/Pages/Content/Home.aspx");
            }
            else
            {
                errorLabel.Text = loginResult.UserName;
            }
        }
    }
}