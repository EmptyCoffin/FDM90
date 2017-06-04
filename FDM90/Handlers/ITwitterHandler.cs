using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDM90.Handlers
{
    public interface ITwitterHandler : IMediaHandler
    {
        string GetRedirectUrl();
        void SaveUserDetails(string verifyString, string userId);
        string GetTweets(string userId);
    }
}
