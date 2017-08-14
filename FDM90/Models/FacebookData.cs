using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace FDM90.Models
{
    public class FacebookData
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("fan_count")]
        public int FanCount { get; set; }
        [JsonProperty("new_like_count")]
        public int NewLikeCount { get; set; }
        [JsonProperty("talking_about_count")]
        public int TalkingAboutCount { get; set; }
        [JsonProperty("posts")]
        public List<FacebookPostData> Posts { get; set; }
        [JsonProperty("page_fan_adds")]
        public FacebookInsightsData PageLikes { get; set; }
        [JsonProperty("page_stories")]
        public FacebookInsightsData PageStories { get; set; }

        public FacebookData Update(FacebookData newData)
        {
            FanCount = newData.FanCount == 0 ? FanCount : newData.FanCount;
            NewLikeCount = newData.NewLikeCount;
            TalkingAboutCount = newData.TalkingAboutCount;

            foreach(FacebookPostData post in newData.Posts)
            {
                if (Posts.FirstOrDefault(current => current.Id == post.Id) != null)
                {
                    Posts[Posts.FindIndex(x => x.Id == post.Id)] = post;
                }
                else
                {
                    Posts.Add(post);
                }
            }

            foreach (FacebookInsightValueData like in newData.PageLikes.Values)
            {
                if (PageLikes.Values.FirstOrDefault(current => current.EndTime == like.EndTime) != null)
                {
                    PageLikes.Values[PageLikes.Values.FindIndex(x => x.EndTime == like.EndTime)] = like;
                }
                else
                {
                    PageLikes.Values.Add(like);
                }
            }

            foreach (FacebookInsightValueData like in newData.PageStories.Values)
            {
                if (PageStories.Values.FirstOrDefault(current => current.EndTime == like.EndTime) != null)
                {
                    PageStories.Values[PageStories.Values.FindIndex(x => x.EndTime == like.EndTime)] = like;
                }
                else
                {
                    PageStories.Values.Add(like);
                }
            }

            return this;
        }
    }

    public class FacebookPostData
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("story")]
        public string Story { get; set; }
        [JsonProperty("created_time")]
        public DateTime CreatedTime { get; set; }
        [JsonProperty("post_impressions_organic_unique")]
        public FacebookInsightsData TotalReach { get; set; }
        [JsonProperty("post_engaged_users")]
        public FacebookInsightsData EngagedUsers { get; set; }
        [JsonProperty("post_negative_feedback")]
        public FacebookInsightsData NegativeFeedback { get; set; }
        [JsonProperty("picture")]
        public string PictureUrl { get; set; }
        [JsonProperty("likes")]
        public List<FacebookFanData> Likes { get; set; }
        [JsonProperty("comments")]
        public List<FacebookCommentsData> Comments { get; set; }
        [JsonProperty("shares")]
        public FacebookSharesData Shares { get; set; }

    }

    public class FacebookCommentsData
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("created_time")]
        public DateTime CreatedTime { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("from")]
        public FacebookFanData From { get; set; }

    }

    public class FacebookFanData
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class FacebookInsightsData
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("period")]
        public string Period { get; set; }
        [JsonProperty("values")]
        public List<FacebookInsightValueData> Values { get; set; }
    }

    public class FacebookInsightValueData
    {
        [JsonProperty("end_time")]
        public DateTime EndTime { get; set; }
        [JsonProperty("value")]
        public int Value { get; set; }
    }

    public class FacebookSharesData
    {
        [JsonProperty("count")]
        public int Count { get; set; }
    }
}