using FDM90.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDM90.Handlers
{
    public interface IFacebookHandler : IMediaHandler
    {
        FacebookCredentials GetLogInDetails(Guid userId);
        FacebookCredentials SaveLogInDetails(Guid userId, string pageName);
        Task SetAccessToken(string shortTermToken, Guid userId, string pageName);
        FacebookData GetFacebookData(Guid userId);
        void PostData(Dictionary<string, string> postParameters, Guid accessToken);
    }
}
