using FDM90.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDM90.Handlers
{
    public interface IFacebookHandler
    {
        FacebookCredentials GetLogInDetails(Guid userId);
        FacebookCredentials SaveLogInDetails(FacebookCredentials credentials);
        string SetAccessToken(string shortTermToken, Guid userId, string pageName);
        FacebookData GetInitialFacebookData(string accessToken);
        FacebookData GetPostDetails(FacebookData currentData);
    }
}
