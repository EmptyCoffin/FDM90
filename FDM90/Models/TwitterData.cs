using LinqToTwitter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FDM90.Models
{
    public class TwitterData
    {
        public TwitterData(List<Status> tweets, int numberOfFollowers)
        {
            Tweets = ConvertedTweets(tweets);
            NumberOfFollowers = numberOfFollowers;
            numberOfRetweets = Tweets.Sum(x => x.RetweetCount);
            numberOfFavorited = Tweets.Sum(x => x.FavoriteCount);
        }

        public int NumberOfFollowers { get; set; }

        public int NumberOfRetweets { get { return numberOfRetweets;  } }

        [NonSerialized]
        private int numberOfRetweets;

        public int NumberOfFavorited { get { return numberOfFavorited; } }

        [NonSerialized]
        private int numberOfFavorited;

        public List<Tweet> Tweets { get; set; }

        public TwitterData Update(List<Status> newTweets)
        {
            foreach (Tweet newTweet in ConvertedTweets(newTweets))
            {
                if (Tweets.First(current => current.StatusID == newTweet.StatusID) != null)
                {
                    Tweets[Tweets.FindIndex(x => x.StatusID == newTweet.StatusID)] = newTweet;
                }
                else
                {
                    Tweets.Add(newTweet);
                }
            }

            if (newTweets.Count > 0)
            {
                NumberOfFollowers = newTweets.OrderBy(x => x.CreatedAt.Date).First().User.FollowersCount;
                numberOfRetweets = Tweets.Sum(x => x.RetweetCount);
                numberOfFavorited = Tweets.Sum(x => x.FavoriteCount);
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
                    Retweeted = tweet.Retweeted,
                    Favorited = tweet.Favorited,
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
        public string ScreenName { get; set; }
        public int NumberOfFollowers { get; set; }
    }
}