using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FDM90.Models;
using System.Linq;
using System.Collections.Generic;
using LinqToTwitter;

namespace FDM90UnitTests
{
    [TestClass]
    public class TwitterDataUnitTest
    {
        [TestMethod]
        public void ParseString_GivenTwitterDataJson_ReturnsTrueIfValuesMatch()
        {
            // act
            TwitterData result = TwitterData.Parse(GetTwitterDataString(), new TwitterData());

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(50, result.NumberOfFollowers);
            Assert.AreEqual(4, result.Tweets.Count);
            Assert.IsTrue(result.Tweets.Any(x => x.CreatedAt != null));
            Assert.IsTrue(result.Tweets.Any(x => x.StatusID != 0));
            Assert.IsTrue(result.NumberOfFollowersByDate.Count > 0);
        }

        [TestMethod]
        public void ReadOnlyVariables_GivenTwitterDataJson_ReturnsTrueIfValuesArePopulatedCorrectly()
        {
            // act
            TwitterData result = TwitterData.Parse(GetTwitterDataString(), new TwitterData());

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(32, result.NumberOfRetweets);
            Assert.AreEqual(20, result.NumberOfFavorited);
        }

        [TestMethod]
        public void Update_GivenTwitterDataJson_ReturnsTrueIfNewTweetsAreAdded()
        {
            // arrange
            TwitterData newData = new TwitterData() {
                Tweets = new List<Tweet>()
                {
                    new Tweet()
                    {
                        CreatedAt = DateTime.Now.Date,
                        Text = "This is a new tweet",
                        FavoriteCount = 0,
                        RetweetCount = 0,
                        StatusID = 1543511348
                    }
                }
            };

            // act
            TwitterData result = TwitterData.Parse(GetTwitterDataString(), new TwitterData()).Update(newData);

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(32, result.NumberOfRetweets);
            Assert.AreEqual(20, result.NumberOfFavorited);
        }

        [TestMethod]
        public void Update_GivenTwitterDataJson_ReturnsTrueIfOldTweetsAreUpdated()
        {
            // arrange
            TwitterData newData = new TwitterData()
            {
                Tweets = new List<Tweet>()
                {
                    new Tweet()
                    {
                        CreatedAt = DateTime.Now.Date,
                        Text = "This Is Test Tweet 1",
                        FavoriteCount = 20,
                        RetweetCount = 8,
                        StatusID = 1254987456
                    }
                }
            };

            // act
            TwitterData result = TwitterData.Parse(GetTwitterDataString(), new TwitterData()).Update(newData);

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Tweets.Count);
            Assert.AreEqual(newData.Tweets[0].RetweetCount, result.Tweets.First(x => x.StatusID == newData.Tweets[0].StatusID).RetweetCount);
            Assert.AreEqual(newData.Tweets[0].FavoriteCount, result.Tweets.First(x => x.StatusID == newData.Tweets[0].StatusID).FavoriteCount);
        }

        [TestMethod]
        public void Update_GivenNumberOfFollowersGreaterOrSmallerThanCurrent_ReturnsTrueIfNumberOfFollowersIsUpdated()
        {

            // arrange
            TwitterData smallerData = new TwitterData()
            {
                NumberOfFollowers = 40
            };

            TwitterData greaterData = new TwitterData()
            {
                NumberOfFollowers = 67
            };

            // act & assert
            TwitterData result = TwitterData.Parse(GetTwitterDataString(), new TwitterData());

            Assert.IsNotNull(result);
            Assert.AreEqual(50, result.NumberOfFollowers);

            result = result.Update(smallerData);

            Assert.IsNotNull(result);
            Assert.AreEqual(40, result.NumberOfFollowers);

            result = result.Update(greaterData);

            Assert.IsNotNull(result);
            Assert.AreEqual(67, result.NumberOfFollowers);
        }

        [TestMethod]
        public void ParseString_GivenTwitterUserDataJson_ReturnsTrueIfValuesMatch()
        {
            // act
            TwitterUser result = TwitterData.Parse((dynamic)"{\"NumberOfFollowers\":181}", new TwitterUser());

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(181, result.NumberOfFollowers);
        }

        [TestMethod]
        public void Constructor_ParseListOfStatus_ReturnsTrueIfValuesPopulatedProperly()
        {
            // arrange
            List<Status> statuses = new List<Status>()
            {
                new Status()
                {
                    CreatedAt= DateTime.Now.Date,
                    ScreenName = "TestScreenName",
                    FavoriteCount= 10,
                    RetweetCount= 27,
                    StatusID= 123454103,
                    Text = "This Is Test Text 1"
                },
                new Status()
                {
                    CreatedAt= DateTime.Now.Date,
                    ScreenName = "TestScreenName",
                    FavoriteCount= 7,
                    RetweetCount= 0,
                    StatusID= 18135432649,
                    Text = "This Is Test Text 2"
                },
                new Status()
                {
                    CreatedAt= DateTime.Now.Date,
                    ScreenName = "TestScreenName",
                    FavoriteCount= 0,
                    RetweetCount= 54,
                    StatusID= 8461321348,
                    Text = "This Is Test Text 3"
                }
            };

            // act
            TwitterData result = new TwitterData(statuses, 384);

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(384, result.NumberOfFollowers);

            for (int i = 0; i < statuses.Count; i++)
            {
                Assert.AreEqual(statuses[i].StatusID, result.Tweets[i].StatusID);
                Assert.AreEqual(statuses[i].CreatedAt, result.Tweets[i].CreatedAt);
                Assert.AreEqual(statuses[i].ScreenName, result.Tweets[i].ScreenName);
                Assert.AreEqual(statuses[i].FavoriteCount, result.Tweets[i].FavoriteCount);
                Assert.AreEqual(statuses[i].RetweetCount, result.Tweets[i].RetweetCount);
                Assert.AreEqual(statuses[i].Text, result.Tweets[i].Text);
                Assert.AreEqual(statuses[i].RetweetCount > 0, result.Tweets[i].Retweeted);
                Assert.AreEqual(statuses[i].FavoriteCount > 0, result.Tweets[i].Favorited);
            }
        }

        private string GetTwitterDataString()
        {
            return "{\"NumberOfFollowers\":50,\"Tweets\":[{\"CreatedAt\":\"2017-07-28T14:42:51.9829694+01:00\",\"ScreenName\":null,\"FavoriteCount\":3,\"RetweetCount\":6,\"StatusID\":1254987456,\"Text\":\"This Is Test Tweet 1\",\"Retweeted\":true,\"Favorited\":true,\"RetweetedUsers\":[{\"NumberOfFollowers\":181},{\"NumberOfFollowers\":143},{\"NumberOfFollowers\":405},{\"NumberOfFollowers\":166},{\"NumberOfFollowers\":448},{\"NumberOfFollowers\":493}]},{\"CreatedAt\":\"2017-07-24T14:42:51.9829694+01:00\",\"ScreenName\":null,\"FavoriteCount\":8,\"RetweetCount\":16,\"StatusID\":546158431,\"Text\":\"This Is Test Tweet 2\",\"Retweeted\":true,\"Favorited\":true,\"RetweetedUsers\":[{\"NumberOfFollowers\":181},{\"NumberOfFollowers\":143},{\"NumberOfFollowers\":405},{\"NumberOfFollowers\":166},{\"NumberOfFollowers\":448},{\"NumberOfFollowers\":493},{\"NumberOfFollowers\":25},{\"NumberOfFollowers\":235},{\"NumberOfFollowers\":257},{\"NumberOfFollowers\":241},{\"NumberOfFollowers\":342},{\"NumberOfFollowers\":32},{\"NumberOfFollowers\":2},{\"NumberOfFollowers\":422},{\"NumberOfFollowers\":110},{\"NumberOfFollowers\":215}]},{\"CreatedAt\":\"2017-07-30T14:42:51.9829694+01:00\",\"ScreenName\":null,\"FavoriteCount\":1,\"RetweetCount\":2,\"StatusID\":549845579112,\"Text\":\"This Is Test Tweet 3\",\"Retweeted\":true,\"Favorited\":true,\"RetweetedUsers\":[{\"NumberOfFollowers\":181},{\"NumberOfFollowers\":143}]},{\"CreatedAt\":\"2017-08-01T14:42:51.9829694+01:00\",\"ScreenName\":null,\"FavoriteCount\":8,\"RetweetCount\":8,\"StatusID\":32154578612,\"Text\":\"This Is Test Tweet 4\",\"Retweeted\":true,\"Favorited\":true,\"RetweetedUsers\":[{\"NumberOfFollowers\":181},{\"NumberOfFollowers\":143},{\"NumberOfFollowers\":405},{\"NumberOfFollowers\":166},{\"NumberOfFollowers\":448},{\"NumberOfFollowers\":493},{\"NumberOfFollowers\":25},{\"NumberOfFollowers\":235}]}], \"NumberOfFollowersByDate\":{\"2017-09-17T00:00:00+01:00\":15,\"2017-09-16T00:00:00+01:00\":7,\"2017-09-15T00:00:00+01:00\":4,\"2017-09-14T00:00:00+01:00\":120}}";
        }
    }
}
