using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDM90.Models.Helpers
{
    public interface ITwitterClientWrapper
    {
        Task<List<Status>> GetTweets(TwitterCredentials twitterDetails);
        Task<List<Status>> GetRetweeterFollowers(TwitterCredentials twitterDetails, ulong statusId);
        Task<Status> PostTweet(TwitterCredentials twitterDetails, Dictionary<string, string> postParameters);
    }
}
