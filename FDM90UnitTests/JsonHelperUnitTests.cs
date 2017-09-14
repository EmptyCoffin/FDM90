using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FDM90.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FDM90.Models.Helpers;
using System.Web.Script.Serialization;
using Facebook;
using System.Collections.Generic;
using System.Linq;

namespace FDM90UnitTests
{
    [TestClass]
    public class JsonHelperUnitTests
    {
        [TestMethod]
        public void Parse_GivenDynamicData_PassIfDataIsCorrect()
        {
            //arrange
            var mainData = new JsonObject();
            mainData.Add("id", "1233456789");
            mainData.Add("name", "Test Name");
            mainData.Add("fan_count", 951);
            mainData.Add("talking_about_count", 159);

            var postData = new JsonObject();
            postData.Add("id", "123456789_987564321");
            postData.Add("message", "This Is A Test Message");

            var likeData = new JsonObject();
            likeData.Add("id", "123456789");
            likeData.Add("name", "Test User");

            var likeData2 = new JsonObject();
            likeData2.Add("id", "987654321");
            likeData2.Add("name", "Test User 2");

            var fromData = new JsonObject();
            fromData.Add("name", "Test User");
            fromData.Add("id", "132456789");

            var commentData = new JsonObject();
            commentData.Add("created_time", "2017-02-19T16:38:23+0000");
            commentData.Add("message", "This is a Test Message");
            commentData.Add("id", "123456789_987654321");
            commentData.Add("from", fromData);

            var dataData = new JsonObject();
            dataData.Add("data", new JsonArray { likeData, likeData2 });

            postData.Add("likes", dataData);

            dataData = new JsonObject();
            dataData.Add("data", new JsonArray { commentData, commentData, commentData });
            postData.Add("comments", dataData);

            dataData = new JsonObject();
            dataData.Add("data", new JsonArray { postData });

            mainData.Add("posts", dataData);

            //act
            FacebookData result = JsonHelper.Parse(mainData, new FacebookData());

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, "1233456789");
            Assert.AreEqual(result.Name, "Test Name");
            Assert.AreEqual(result.FanCount, 951);
            Assert.AreEqual(result.TalkingAboutCount, 159);
            Assert.AreEqual(result.Posts.Count, 1);
            Assert.AreEqual(result.Posts[0].Comments.Count, 3);
            Assert.AreEqual(result.Posts[0].Likes.Count, 2);
            Assert.AreEqual(result.Posts[0].Id, "123456789_987564321");
            Assert.AreEqual(result.Posts[0].Message, "This Is A Test Message");
            Assert.AreEqual(result.Posts[0].Likes[0].Id, "123456789");
            Assert.AreEqual(result.Posts[0].Likes[1].Id, "987654321");
            Assert.AreEqual(result.Posts[0].Likes[0].Name, "Test User");
            Assert.AreEqual(result.Posts[0].Likes[1].Name, "Test User 2");
            Assert.AreEqual(result.Posts[0].Comments[0].CreatedTime, DateTime.Parse("2017-02-19T16:38:23+0000"));
            Assert.AreEqual(result.Posts[0].Comments[0].Message, "This is a Test Message");
            Assert.AreEqual(result.Posts[0].Comments[0].Id, "123456789_987654321");
            Assert.AreEqual(result.Posts[0].Comments[0].From.Id, "132456789");
            Assert.AreEqual(result.Posts[0].Comments[0].From.Name, "Test User");
            Assert.AreEqual(result.Posts[0].Comments[1].CreatedTime, DateTime.Parse("2017-02-19T16:38:23+0000"));
            Assert.AreEqual(result.Posts[0].Comments[1].Message, "This is a Test Message");
            Assert.AreEqual(result.Posts[0].Comments[1].Id, "123456789_987654321");
            Assert.AreEqual(result.Posts[0].Comments[1].From.Id, "132456789");
            Assert.AreEqual(result.Posts[0].Comments[1].From.Name, "Test User");
            Assert.AreEqual(result.Posts[0].Comments[2].CreatedTime, DateTime.Parse("2017-02-19T16:38:23+0000"));
            Assert.AreEqual(result.Posts[0].Comments[2].Message, "This is a Test Message");
            Assert.AreEqual(result.Posts[0].Comments[2].Id, "123456789_987654321");
            Assert.AreEqual(result.Posts[0].Comments[2].From.Id, "132456789");
            Assert.AreEqual(result.Posts[0].Comments[2].From.Name, "Test User");
        }

        [TestMethod]
        public void Parse_GivenObjectHasValues_PassIfDataIsIsNotOverridden()
        {
            //arrange
            var facebookData = new JsonObject();
            facebookData.Add("name", "Facebook Page");
            facebookData.Add("fan_count", 123);

            FacebookData originalData = new FacebookData() { Id = "123654789" };

            //act
            FacebookData result = JsonHelper.Parse(facebookData, originalData);

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, originalData.Id);
            Assert.AreEqual(result.Name, "Facebook Page");
            Assert.AreEqual(result.FanCount, 123);
        }

        [TestMethod]
        public void Parse_GivenDynamicPostData_PassIfDataIsCorrect()
        {
            //arrange
            var valueData = new JsonObject();
            valueData.Add("value", 54);

            var metricData = new JsonObject();
            metricData.Add("name", "post_impressions_organic_unique");
            metricData.Add("period", "lifetime");
            metricData.Add("values", new JsonArray { valueData });

            var metricData2 = new JsonObject();
            metricData2.Add("name", "post_negative_feedback");
            metricData2.Add("period", "lifetime");
            metricData2.Add("values", new JsonArray { valueData });

            var metricsData = new JsonObject();
            metricsData.Add("data", new JsonArray { metricData, metricData2 });
            dynamic data = metricsData;

            //act
            FacebookPostData result = JsonHelper.Parse(data.data, new FacebookPostData());

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.TotalReach.Name, "post_impressions_organic_unique");
            Assert.AreEqual(result.TotalReach.Period, "lifetime");
            Assert.AreEqual(result.TotalReach.Values.Count, 1);
            Assert.AreEqual(result.TotalReach.Values[0].Value, 54);
        }

        [TestMethod]
        public void Parse_GivenDynamicPostData_PassIfDataIsUpdatesCorrect()
        {
            //arrange
            var valueData = new JsonObject();
            valueData.Add("value", 54);

            var metricData = new JsonObject();
            metricData.Add("name", "post_impressions_organic_unique");
            metricData.Add("period", "lifetime");
            metricData.Add("values", new JsonArray { valueData });

            var metricsData = new JsonObject();
            metricsData.Add("data", new JsonArray { metricData });
            dynamic data = metricsData;

            FacebookData currentData = new FacebookData();
            currentData.Posts = new List<FacebookPostData>()
            {
                new FacebookPostData()
                {
                    Id = "123456789987564321",
                    TotalReach = new FacebookInsightsData()
                    {
                        Name = "post_impressions_organic_unique",
                        Period = "lifetime",
                        Values = new List<FacebookInsightValueData>()
                        {
                            new FacebookInsightValueData()
                            {
                                Value = 24,
                                EndTime = new DateTime(2016, 5, 5)
                            },
                            new FacebookInsightValueData()
                            {
                                Value = 24,
                                EndTime = new DateTime(2016, 5, 6)
                            }
                        }
                    }
                }
            };

            //act
            for (int i = 0; i < data.data[0].values.Count; i++)
            {
                currentData.Posts[0].TotalReach.Values.Add(JsonHelper.Parse(data.data[i], new FacebookInsightValueData()));
            }

            //assert
            Assert.IsNotNull(currentData.Posts[0]);
            Assert.AreEqual(currentData.Posts[0].TotalReach.Name, "post_impressions_organic_unique");
            Assert.AreEqual(currentData.Posts[0].TotalReach.Period, "lifetime");
            Assert.AreEqual(currentData.Posts[0].TotalReach.Values.Count, 3);
            Assert.AreEqual(currentData.Posts[0].TotalReach.Values[0].Value, 24);
        }

        [TestMethod]
        public void Parse_GivenDynamicPageLike_PassIfDataIsCorrect()
        {
            //arrange
            var valueData = new JsonObject();
            valueData.Add("value", 54);
            valueData.Add("end_date", "2017-05-16T07:00:00+0000");

            var metricData = new JsonObject();
            metricData.Add("name", "page_fan_adds");
            metricData.Add("period", "lifetime");
            metricData.Add("values", new JsonArray { valueData, valueData });

            var metricsData = new JsonObject();
            metricsData.Add("data", new JsonArray { metricData });
            dynamic data = metricsData;

            //act
            FacebookData result = JsonHelper.Parse(data.data, new FacebookData());

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.PageLikes.Name, "page_fan_adds");
            Assert.AreEqual(result.PageLikes.Period, "lifetime");
            Assert.AreEqual(result.PageLikes.Values.Count, 2);
            Assert.AreEqual(result.PageLikes.Values[0].Value, 54);
            Assert.AreEqual(result.PageLikes.Values[1].Value, 54);
            //Assert.AreEqual(result.PageLikes.Values[0].EndTime, DateTime.Parse("2017-05-16T07:00:00+0000"));
            //Assert.AreEqual(result.PageLikes.Values[1].EndTime, DateTime.Parse("2017-05-16T07:00:00+0000"));
        }

        [TestMethod]
        public void Parse_GivenDynamicPageLike_PassIfDataIsUpdated()
        {
            //arrange
            var valueData = new JsonObject();
            valueData.Add("value", 54);
            valueData.Add("end_date", "2017-05-16T07:00:00+0000");

            var metricData = new JsonObject();
            metricData.Add("name", "page_fan_adds");
            metricData.Add("period", "lifetime");
            metricData.Add("values", new JsonArray { valueData, valueData });

            var metricsData = new JsonObject();
            metricsData.Add("data", new JsonArray { metricData });
            dynamic data = metricsData;

            FacebookData currentData = new FacebookData();
            currentData.PageLikes = new FacebookInsightsData()
            {
                Name = "page_fan_adds",
                Period = "lifetime",
                Values = new List<FacebookInsightValueData>()
                {
                    new FacebookInsightValueData()
                    {
                        Value = 24,
                        EndTime = new DateTime(2016, 5, 5)
                    },
                    new FacebookInsightValueData()
                    {
                        Value = 24,
                        EndTime = new DateTime(2016, 5, 6)
                    }
                }
            };

            //act
            for (int i = 0; i < data.data[0].values.Count; i++)
            {
                currentData.PageLikes.Values.Add(JsonHelper.Parse(data.data[0].values[i], new FacebookInsightValueData()));
            }

            //assert
            //Assert.IsNotNull(result);
            //Assert.AreEqual(result.Name, "page_fan_adds");
            //Assert.AreEqual(result.Period, "lifetime");
            Assert.AreEqual(currentData.PageLikes.Values.Count, 4);
            //Assert.AreEqual(result.Values[0].Value, 24);
            //Assert.AreEqual(result.Values[1].Value, 24);
            //Assert.AreEqual(result.Values[2].Value, 54);
            //Assert.AreEqual(result.Values[3].Value, 54);
            //Assert.AreEqual(result.PageLikes.Values[0].EndTime, DateTime.Parse("2017-05-16T07:00:00+0000"));
            //Assert.AreEqual(result.PageLikes.Values[1].EndTime, DateTime.Parse("2017-05-16T07:00:00+0000"));
        }

        [TestMethod]
        public void AddWeekValue_GivenNewJsonObject_ReturnsTrueIfWeekValueAdded()
        {
            // arrange
            JObject newObject = new JObject();
            string propertyNameValue = "NewPropertyName";
            int addValue = 15;

            // act
            var result = JsonHelper.AddWeekValue(newObject, propertyNameValue, DateTime.Now.Date, addValue);

            // assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Properties().ToList()[0].Name.Contains("Week"));
            Assert.IsNotNull(((JObject)result.Values().First()).Property(propertyNameValue));
            Assert.AreEqual(addValue, ((JObject)result.Values().First()).Property(propertyNameValue).Value);
        }

        [TestMethod]
        public void AddWeekValue_GivenNewJsonObjectAndValueIsNull_ReturnsTrueIfWeekValueAddedWith0()
        {
            // arrange
            JObject newObject = new JObject();
            string propertyNameValue = "NewPropertyName";

            // act
            var result = JsonHelper.AddWeekValue(newObject, propertyNameValue, DateTime.Now.Date, null);

            // assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Properties().ToList()[0].Name.Contains("Week"));
            Assert.IsNotNull(((JObject)result.Values().First()).Property(propertyNameValue));
            Assert.AreEqual(0, ((JObject)result.Values().First()).Property(propertyNameValue).Value);
        }

        [TestMethod]
        public void AddWeekValue_GivenJsonObjectHasPreviousWeek_ReturnsTrueIfNewWeekValueAdded()
        {
            // arrange
            string propertyNameValue = "NewPropertyName";
            int addValue = 15;
            JObject newObject = new JObject();
            JObject objectValue = new JObject();
            objectValue.Add(propertyNameValue, addValue);
            newObject.Add("Week0", objectValue);

            // act
            var result = JsonHelper.AddWeekValue(newObject, propertyNameValue, DateTime.Now.Date, addValue);

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Properties().Count());
            foreach(var property in result.Properties())
            {
                Assert.IsTrue(property.Name.Contains("Week"));
                foreach(JProperty value in property.Values())
                {
                    Assert.AreEqual(propertyNameValue, value.Name);
                    Assert.AreEqual(addValue, value.Value);
                }
            }
        }

        [TestMethod]
        public void AddWeekValue_GivenJsonObjectHasCurrentWeek_ReturnsTrueIfNewWeekValueUpdated()
        {
            // arrange
            string propertyNameValue = "NewPropertyName";
            int addValue = 15;
            var setupObject = JsonHelper.AddWeekValue(new JObject(), propertyNameValue, DateTime.Now.Date, addValue);

            // act
            var result = JsonHelper.AddWeekValue(setupObject, propertyNameValue, DateTime.Now.Date, addValue);

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Properties().Count());
            foreach (var property in result.Properties())
            {
                Assert.IsTrue(property.Name.Contains("Week"));
                Assert.AreEqual(1, property.Values().Count());
                foreach (JProperty value in property.Values())
                {
                    Assert.AreEqual(propertyNameValue, value.Name);
                    Assert.AreEqual(addValue * 2, value.Value);
                }
            }
        }
    }
}
