using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FDM90.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FDM90.Models.Helpers;
using System.Web.Script.Serialization;
using Facebook;
using System.Collections.Generic;

namespace FDM90UnitTests
{
    [TestClass]
    public class FacebookDataUnitTests
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

            FacebookData originalData = new FacebookData() { Id="123654789" };

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
    }
}
