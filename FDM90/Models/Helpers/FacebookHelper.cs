using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FDM90.Models.Helpers
{
    public class FacebookHelper
    {
        public static string BaseUrl = "https://graph.facebook.com/v2.8/";
        public static string FieldParameter = "me?fields=";
        public static string InsightParameter = "/insights/";
        public static string AccountParameter = "/accounts?access_token=";

        public static string AccessToken = "access_token";
        public static string Id = "id";
        public static string Name = "name";
        public static string FanCount = "fan_count";
        public static string TalkingAboutCount = "talking_about_count";
        public static string PageFansCity = "page_fans_city";
        public static string Posts = "posts{id,message,created_time,picture,likes,comments}";
        public static string PostDetails = "post_fan_reach,post_negative_feedback";

        public static string UrlBuilder(FacebookParameters parameter, string id, string[] fields)
        {
            string url = string.Empty;

            switch(parameter)
            {
                case FacebookParameters.Field:
                    url = BaseUrl + FieldParameter + string.Join(",", fields);
                    break;

                case FacebookParameters.Insight:
                    url = BaseUrl + id + InsightParameter + string.Join(",", fields);
                    break;

                case FacebookParameters.Account:
                    url = BaseUrl + id + AccountParameter + fields[0];
                    break;
            }

            return url;
        }
    }

    public enum FacebookParameters
    {
        Field,
        Insight,
        Account
    }
}