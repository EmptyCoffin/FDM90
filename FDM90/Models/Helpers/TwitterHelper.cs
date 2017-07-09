using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FDM90.Models.Helpers
{
    public class TwitterHelper
    {
        public static string BaseUrl = "https://api.twitter.com/1.1/";
        public static string Authentication = "oauth/authenticate?force_login=true";
        public static string StatusLimit = "statuses";
        public static string RetweetUrl = "/statuses/retweets/:id";
        public static string UserTimelineUrl = "/statuses/user_timeline";

        public static string UrlBuilder(TwitterParameters param)
        {
            string url = string.Empty;

            switch(param)
            {
                case TwitterParameters.Login:
                    url += BaseUrl + Authentication;
                    break;
            }

            return url;
        }
    }

    public enum TwitterParameters
    {
        Login
    }
}