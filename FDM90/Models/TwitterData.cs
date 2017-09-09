using LinqToTwitter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace FDM90.Models
{
    public class TwitterData
    {
        public TwitterData()
        {

        }

        public TwitterData(List<Status> tweets, int numberOfFollowers)
        {
            Tweets = ConvertedTweets(tweets);
            NumberOfFollowers = numberOfFollowers;
        }

        public int NumberOfFollowers { get; set; }

        [JsonIgnore()]
        public int NumberOfRetweets { get { return Tweets.Sum(x => x.RetweetCount);  } }

        [JsonIgnore()]
        public int NumberOfFavorited { get { return Tweets.Sum(x => x.FavoriteCount); } }

        public List<Tweet> Tweets { get; set; }

        public static T Parse<T>(dynamic json, T data)
        {
            if (!string.IsNullOrWhiteSpace(json.ToString()))
            {
                JObject jsonObject = JObject.Parse(json.ToString());

                foreach (PropertyInfo property in data.GetType().GetProperties())
                {
                    JsonIgnoreAttribute jsonIgnore = (JsonIgnoreAttribute)property.GetCustomAttribute(typeof(JsonIgnoreAttribute));

                    if(jsonIgnore == null)
                    {
                        if(property.PropertyType.Namespace.Contains("Collection"))
                        {
                            IList listInstance = (IList)Activator.CreateInstance(property.PropertyType);

                            foreach(var value in jsonObject[property.Name])
                            {
                                listInstance.Add(Parse(value, Activator.CreateInstance(property.PropertyType.GetGenericArguments().Single())));
                            }

                            property.SetValue(data, listInstance);
                        }
                        else
                        {
                            if(jsonObject[property.Name] != null)
                                property.SetValue(data, Convert.ChangeType((jsonObject[property.Name]), property.PropertyType));
                        }
                    }
                }
            }
            return data;
        }

        public TwitterData Update(TwitterData newTweets)
        {
            if (newTweets.NumberOfFollowers != 0 && newTweets.NumberOfFollowers != NumberOfFollowers)
                NumberOfFollowers = newTweets.NumberOfFollowers;

            if (newTweets.Tweets != null)
            {
                foreach (Tweet newTweet in newTweets.Tweets)
                {
                    if (Tweets.Count(current => current.StatusID == newTweet.StatusID) > 0)
                    {
                        Tweets[Tweets.FindIndex(x => x.StatusID == newTweet.StatusID)] = newTweet;
                    }
                    else
                    {
                        Tweets.Add(newTweet);
                    }
                }
            }

            return this;
        }

        private List<Tweet> ConvertedTweets(List<Status> tweets)
        {
            List<Tweet> convertedTweets = new List<Tweet>();

            foreach (Status tweet in tweets)
            {
                convertedTweets.Add(new Tweet()
                {
                    CreatedAt = tweet.CreatedAt,
                    ScreenName = tweet.ScreenName,
                    FavoriteCount = (int)tweet.FavoriteCount,
                    RetweetCount = tweet.RetweetCount,
                    StatusID = tweet.StatusID,
                    Text = tweet.Text,
                    Retweeted = tweet.RetweetCount > 0,
                    Favorited = tweet.FavoriteCount > 0,
                    RetweetedUsers = new List<TwitterUser>()
                });
            }

            return convertedTweets;
        }
    }

    public class Tweet
    {
        public DateTime CreatedAt { get; set; }
        public string ScreenName { get; set; }
        public int FavoriteCount { get; set; }
        public int RetweetCount { get; set; }
        public ulong StatusID { get; set; }
        public string Text { get; set; }
        public bool Retweeted { get; set; }
        public bool Favorited { get; set; }
        public List<TwitterUser> RetweetedUsers {get;set;}
    }

    public class TwitterUser
    {
        public int NumberOfFollowers { get; set; }
    }
}