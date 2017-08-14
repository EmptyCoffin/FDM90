using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FDM90.Singleton;

namespace FDM90.Models.Helpers
{
    public class FacebookHelper
    {
        public static string BaseUrl = "https://graph.facebook.com/v2.8/";
        public static string IdParameter = "me?";
        public static string InsightIdParameter = "me";
        public static string FieldParameter = "me?fields=";
        public static string InsightParameter = "/insights/";
        public static string AccountParameter = "/accounts?access_token=";
        public static string PostAuthParameter = "oauth/access_token";

        public static string AccessToken = "access_token";
        public static string Id = "id";
        public static string Name = "name";
        public static string FanCount = "fan_count";
        public static string TalkingAboutCount = "talking_about_count";
        public static string PageFansCity = "page_fans_city";
        public static string Posts = "posts{id,message,story,created_time,picture,likes,comments,shares}";
        public static string PostReach = "post_impressions_organic_unique";
        public static string PostNegativity = "post_negative_feedback";
        public static string PageLikes = "page_fan_adds";
        public static string PostEngagedUsers = "post_engaged_users";
        public static string PageStories = "page_stories/day";


        public static string UrlBuilder(FacebookParameters parameter, string id, string[] fields)
        {
            string url = string.Empty;

            switch(parameter)
            {
                case FacebookParameters.Id:
                    url = BaseUrl + IdParameter + string.Join(",", fields);
                    break;

                case FacebookParameters.Field:
                    url = BaseUrl + FieldParameter + string.Join(",", fields);
                    break;

                case FacebookParameters.Insight:
                    url = BaseUrl + (string.IsNullOrWhiteSpace(id) ? InsightIdParameter : id) + InsightParameter + string.Join(",", fields);
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
        Id,
        Field,
        Insight,
        Account
    }
}