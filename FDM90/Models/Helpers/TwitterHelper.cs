using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FDM90.Models.Helpers
{
    public class TwitterHelper
    {
        public static string StatusLimit = "statuses";
        public static string RetweetUrl = "/statuses/retweets/:id";
        public static string UserTimelineUrl = "/statuses/user_timeline";
    }
}