using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FDM90.Models;
using System.Collections.Generic;
using Facebook;
using FDM90.Models.Helpers;
using System.Linq;

namespace FDM90UnitTests
{
    [TestClass]
    public class FacebookDataUnitTests
    {
        [TestMethod]
        public void Update_GivenTwitterDataJson_ReturnsTrueIfNewValuesAreAdded()
        {
            // arrange
            FacebookData newData = new FacebookData()
            {
                Posts = new List<FacebookPostData>()
                {
                    new FacebookPostData()
                    {
                        CreatedTime = DateTime.Now.Date,
                        Id = "1651065154",
                        Message = "This is a new message 1"
                    },
                    new FacebookPostData()
                    {
                        CreatedTime = DateTime.Now.Date,
                        Id = "2894651813",
                        Message = "This is a new message 2"
                    }
                },
                PageStories = new FacebookInsightsData()
                {
                    Values = new List<FacebookInsightValueData>()
                    {
                        new FacebookInsightValueData()
                        {
                            EndTime = DateTime.Now.AddDays(-1).Date,
                            Value = 10
                        },
                        new FacebookInsightValueData()
                        {
                            EndTime = DateTime.Now.Date,
                            Value = 20
                        }
                    }
                },
                PageLikes = new FacebookInsightsData()
                {
                    Values = new List<FacebookInsightValueData>()
                    {
                        new FacebookInsightValueData()
                        {
                            EndTime = DateTime.Now.AddDays(-1).Date,
                            Value = 6
                        },
                        new FacebookInsightValueData()
                        {
                            EndTime = DateTime.Now.Date,
                            Value = 14
                        }
                    }
                }
            };

            // act
            FacebookData original = JsonHelper.Parse(GetBasicFacebookDataWithPosts(), new FacebookData());
            FacebookData updated = JsonHelper.Parse(GetBasicFacebookDataWithPosts(), new FacebookData()).Update(newData);

            // assert
            Assert.IsNotNull(updated);
            Assert.AreNotEqual(original.Posts.Count, updated.Posts.Count);
            Assert.AreNotEqual(original.PageLikes.Values.Count, updated.PageLikes.Values.Count);
            Assert.AreNotEqual(original.PageStories.Values.Count, updated.PageStories.Values.Count);

            for (int i = 0; i < original.Posts.Count; i++)
            {
                Assert.AreEqual(original.Posts[i].Id, updated.Posts.First(x => x.Id == original.Posts[i].Id).Id);
                Assert.AreEqual(original.Posts[i].CreatedTime, updated.Posts.First(x => x.Id == original.Posts[i].Id).CreatedTime);
                Assert.AreEqual(original.Posts[i].Message, updated.Posts.First(x => x.Id == original.Posts[i].Id).Message);
            }

            for (int i = 0; i < newData.Posts.Count; i++)
            {
                Assert.AreEqual(newData.Posts[i].Id, updated.Posts.First(x => x.Id == newData.Posts[i].Id).Id);
                Assert.AreEqual(newData.Posts[i].CreatedTime, updated.Posts.First(x => x.Id == newData.Posts[i].Id).CreatedTime);
                Assert.AreEqual(newData.Posts[i].Message, updated.Posts.First(x => x.Id == newData.Posts[i].Id).Message);
            }

            for (int i = 0; i < original.PageLikes.Values.Count; i++)
            {
                Assert.AreEqual(original.PageLikes.Values[i].Value, updated.PageLikes.Values.First(x => x.EndTime == original.PageLikes.Values[i].EndTime).Value);
                Assert.AreEqual(original.PageLikes.Values[i].EndTime, updated.PageLikes.Values.First(x => x.EndTime == original.PageLikes.Values[i].EndTime).EndTime);
            }

            for (int i = 0; i < newData.PageLikes.Values.Count; i++)
            {
                Assert.AreEqual(newData.PageLikes.Values[i].Value, updated.PageLikes.Values.First(x => x.EndTime == newData.PageLikes.Values[i].EndTime).Value);
                Assert.AreEqual(newData.PageLikes.Values[i].EndTime, updated.PageLikes.Values.First(x => x.EndTime == newData.PageLikes.Values[i].EndTime).EndTime);
            }

            for (int i = 0; i < original.PageStories.Values.Count; i++)
            {
                Assert.AreEqual(original.PageStories.Values[i].Value, updated.PageStories.Values.First(x => x.EndTime == original.PageStories.Values[i].EndTime).Value);
                Assert.AreEqual(original.PageStories.Values[i].EndTime, updated.PageStories.Values.First(x => x.EndTime == original.PageStories.Values[i].EndTime).EndTime);
            }

            for (int i = 0; i < newData.PageStories.Values.Count; i++)
            {
                Assert.AreEqual(newData.PageStories.Values[i].Value, updated.PageStories.Values.First(x => x.EndTime == newData.PageStories.Values[i].EndTime).Value);
                Assert.AreEqual(newData.PageStories.Values[i].EndTime, updated.PageStories.Values.First(x => x.EndTime == newData.PageStories.Values[i].EndTime).EndTime);
            }
        }

        [TestMethod]
        public void Update_GivenTwitterDataJson_ReturnsTrueIfOldTweetsAreUpdated()
        {
            // arrange
            FacebookData newData = new FacebookData()
            {
                Posts = new List<FacebookPostData>()
                {
                    new FacebookPostData()
                    {
                        CreatedTime = DateTime.Now.Date,
                        Id = "123456789_987564321",
                        Message = "This is a updated message 1"
                    },
                    new FacebookPostData()
                    {
                        CreatedTime = DateTime.Now.Date,
                        Id = "98777654321_987564321",
                        Message = "This is a updated message 2"
                    }
                },
                PageStories = new FacebookInsightsData()
                {
                    Values = new List<FacebookInsightValueData>()
                    {
                        new FacebookInsightValueData()
                        {
                            EndTime = DateTime.Now.AddDays(-4).Date,
                            Value = 53
                        }
                    }
                },
                PageLikes = new FacebookInsightsData()
                {
                    Values = new List<FacebookInsightValueData>()
                    {
                        new FacebookInsightValueData()
                        {
                            EndTime = DateTime.Now.AddDays(-4).Date,
                            Value = 46
                        }
                    }
                }
            };

            // act
            FacebookData original = JsonHelper.Parse(GetBasicFacebookDataWithPosts(), new FacebookData());
            FacebookData updated = JsonHelper.Parse(GetBasicFacebookDataWithPosts(), new FacebookData()).Update(newData);

            // assert
            Assert.IsNotNull(updated);
            Assert.AreEqual(original.Posts.Count, updated.Posts.Count);
            Assert.AreEqual(original.PageLikes.Values.Count, updated.PageLikes.Values.Count);
            Assert.AreEqual(original.PageStories.Values.Count, updated.PageStories.Values.Count);

            for (int i = 0; i < newData.Posts.Count; i++)
            {
                Assert.AreEqual(newData.Posts[i].Id, updated.Posts.First(x => x.Id == newData.Posts[i].Id).Id);
                Assert.AreEqual(newData.Posts[i].CreatedTime, updated.Posts.First(x => x.Id == newData.Posts[i].Id).CreatedTime);
                Assert.AreEqual(newData.Posts[i].Message, updated.Posts.First(x => x.Id == newData.Posts[i].Id).Message);
                Assert.AreNotEqual(original.Posts.First(x => x.Id == newData.Posts[i].Id).CreatedTime, updated.Posts.First(x => x.Id == newData.Posts[i].Id).CreatedTime);
                Assert.AreNotEqual(original.Posts.First(x => x.Id == newData.Posts[i].Id).Message, updated.Posts.First(x => x.Id == newData.Posts[i].Id).Message);
            }

            for (int i = 0; i < newData.PageLikes.Values.Count; i++)
            {
                Assert.AreEqual(newData.PageLikes.Values[i].Value, updated.PageLikes.Values.First(x => x.EndTime == newData.PageLikes.Values[i].EndTime).Value);
                Assert.AreEqual(newData.PageLikes.Values[i].EndTime, updated.PageLikes.Values.First(x => x.EndTime == newData.PageLikes.Values[i].EndTime).EndTime);
                Assert.AreNotEqual(original.PageLikes.Values.First(x => x.EndTime == newData.PageLikes.Values[i].EndTime).Value, updated.PageLikes.Values.First(x => x.EndTime == newData.PageLikes.Values[i].EndTime).Value);
            }

            for (int i = 0; i < newData.PageStories.Values.Count; i++)
            {
                Assert.AreEqual(newData.PageStories.Values[i].Value, updated.PageStories.Values.First(x => x.EndTime == newData.PageStories.Values[i].EndTime).Value);
                Assert.AreEqual(newData.PageStories.Values[i].EndTime, updated.PageStories.Values.First(x => x.EndTime == newData.PageStories.Values[i].EndTime).EndTime);
                Assert.AreNotEqual(original.PageStories.Values.First(x => x.EndTime == newData.PageStories.Values[i].EndTime).Value, updated.PageStories.Values.First(x => x.EndTime == newData.PageStories.Values[i].EndTime).Value);
            }
        }

        public JsonObject GetBasicFacebookDataWithPosts()
        {
            var mainData = new JsonObject();
            mainData.Add("id", "1233456789");
            mainData.Add("name", "Test Name");
            mainData.Add("fan_count", 951);
            mainData.Add("talking_about_count", 159);

            // post data
            var postData = new JsonObject();
            postData.Add("id", "123456789_987564321");
            postData.Add("message", "This Is A Test Message1");
            postData.Add("created_time", DateTime.Now.AddDays(-9).Date);

            var postData2 = new JsonObject();
            postData2.Add("id", "987654321_987564321");
            postData2.Add("message", "This Is A Test Message2");
            postData2.Add("created_time", DateTime.Now.AddDays(-10).Date);

            var postData3 = new JsonObject();
            postData3.Add("id", "98777654321_987564321");
            postData3.Add("message", "This Is A Test Message3");
            postData3.Add("created_time", DateTime.Now.AddMonths(-2).Date);

            var dataData = new JsonObject();
            JsonArray posts = new JsonArray() { postData, postData2, postData3 };

            dataData.Add("data", posts);

            var nextData = new JsonObject();
            nextData.Add("next", "https://graph.facebook.com/v2.8/next-post-page");

            dataData.Add("paging", nextData);

            mainData.Add("posts", dataData);

            // page likes data
            var valueData = new JsonObject();
            valueData.Add("value", 12);
            valueData.Add("end_time", DateTime.Now.AddDays(-3).Date);

            var valueData1 = new JsonObject();
            valueData1.Add("value", 28);
            valueData1.Add("end_time", DateTime.Now.AddDays(-4).Date);

            var metricData = new JsonObject();
            metricData.Add("name", "page_fan_adds");
            metricData.Add("period", "lifetime");
            metricData.Add("values", new JsonArray { valueData, valueData1 });

            // page stories data
            var valueData3 = new JsonObject();
            valueData3.Add("value", 19);
            valueData3.Add("end_time", DateTime.Now.AddDays(-3).Date);

            var valueData4 = new JsonObject();
            valueData4.Add("value", 7);
            valueData4.Add("end_time", DateTime.Now.AddDays(-4).Date);

            var metricData2 = new JsonObject();
            metricData2.Add("name", "page_stories");
            metricData2.Add("period", "lifetime");
            metricData2.Add("values", new JsonArray { valueData3, valueData4 });

            mainData.Add("page_fan_adds", metricData);
            mainData.Add("page_stories", metricData2);

            return mainData;
        }
    }
}
