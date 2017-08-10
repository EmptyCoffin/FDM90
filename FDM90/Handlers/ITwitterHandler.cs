using FDM90.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDM90.Handlers
{
    public interface ITwitterHandler : IMediaHandler
    {
        Task SaveUserDetails(string oauthToken, string oauthTokenSecret,string screenName, string userId);
        TwitterData GetTweets(string userId);
    }
}
