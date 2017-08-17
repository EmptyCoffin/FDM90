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

        //[TestMethod]
        //public void test()
        //{
        //    //arrange
        //    var data = new JsonObject();
        //    data.Add("data", "{[{\"id\":\"1816266855303935_1874251009505519\",\"created_time\":\"2017-07-07T06:06:23+0000\",\"picture\":\"https://scontent.xx.fbcdn.net/v/t1.0-0/p130x130/19702464_1874251009505519_8157319702675744427_n.jpg?oh=a2e77cf78f0534e4c6938acbdbe27c25&oe=5A0B3A69\"},{\"id\":\"1816266855303935_1869330489997571\",\"created_time\":\"2017-06-27T22:36:42+0000\",\"likes\":{\"data\":[{\"id\":\"1816266855303935\",\"name\":\"Garden Tool Base\"}],\"paging\":{\"cursors\":{\"before\":\"MTgxNjI2Njg1NTMwMzkzNQZDZD\",\"after\":\"MTgxNjI2Njg1NTMwMzkzNQZDZD\"}}}},{\"id\":\"1816266855303935_1869255973338356\",\"created_time\":\"2017-06-27T19:16:16+0000\"},{\"id\":\"1816266855303935_1869255840005036\",\"created_time\":\"2017-06-27T19:15:49+0000\",\"picture\":\"https://scontent.xx.fbcdn.net/v/t1.0-0/s130x130/19429732_1869255840005036_585741077312942872_n.jpg?oh=31d34a9e5a08961c30e6a4d089ab799d&oe=59CA3196\"},{\"id\":\"1816266855303935_1869255170005103\",\"created_time\":\"2017-06-27T19:13:15+0000\"},{\"id\":\"1816266855303935_1869254906671796\",\"created_time\":\"2017-06-27T19:12:44+0000\"},{\"id\":\"1816266855303935_1866544166942870\",\"message\":\"Gardening tools\nIf you really wish to keep your #garden in perfect and well shape then garden #tools are a obligation. So if you are to preserve your garden then effective garden tool is necessary.\nhttps://www.gardentoolbase.co.uk/blog/look-gardening-tools/\",\"created_time\":\"2017-06-22T06:42:30+0000\",\"picture\":\"https://scontent.xx.fbcdn.net/v/t1.0-0/p130x130/19396984_1866544166942870_7116507185460928089_n.jpg?oh=4af8c31df149380d660dff8cafb774db&oe=5A05CA45\"},{\"id\":\"1816266855303935_1863840630546557\",\"message\":\"Childrens Garden Tools\n\nhttps://www.gardentoolbase.co.uk/category/Childrens-garden-tools\",\"created_time\":\"2017-06-16T07:38:37+0000\",\"picture\":\"https://scontent.xx.fbcdn.net/v/t1.0-0/p130x130/19274828_1863840630546557_4715859818434433807_n.jpg?oh=7fcc01ca47e7c2ac268338ee9e6bafac&oe=59D2B8CD\",\"likes\":{\"data\":[{\"id\":\"1816266855303935\",\"name\":\"Garden Tool Base\"}],\"paging\":{\"cursors\":{\"before\":\"MTgxNjI2Njg1NTMwMzkzNQZDZD\",\"after\":\"MTgxNjI2Njg1NTMwMzkzNQZDZD\"}}}},{\"id\":\"1816266855303935_1860064774257476\",\"message\":\"All garden surgery tools\nThat great shopping list of #garden tool can aid get you on the go and using any of the #tools above helps to get the clean and tidy garden always.\nhttps://www.gardentoolbase.co.uk/blog/category/garden-tool-base-uk/\",\"created_time\":\"2017-06-08T07:38:44+0000\",\"picture\":\"https://scontent.xx.fbcdn.net/v/t1.0-0/p130x130/18951128_1860064774257476_1988237764700123744_n.jpg?oh=99b48c89573ba5a7965387cd44843e32&oe=59CAAA45\"},{\"id\":\"1816266855303935_1856155027981784\",\"message\":\"Garden Tools Base UK\n#Verdemax employs highly skilled Technicians who select the highest quality material and create many #tool designs that are exclusive to the Brand. All products are rigorously tested to ensure the highest quality.\nhttps://www.gardentoolbase.co.uk/\",\"created_time\":\"2017-05-30T11:47:25+0000\",\"picture\":\"https://scontent.xx.fbcdn.net/v/t1.0-0/p130x130/18740449_1856155027981784_8933939552007014278_n.jpg?oh=473a6e47a83d074efbe25a2370a3421f&oe=5A0A75A8\"},{\"id\":\"1816266855303935_1854096394854314\",\"message\":\"Garden Toolbelts and Bags\nBusiness listings of Leather Tool Pouches manufacturers, suppliers and exporters in #UK along with their contact details & address.\nUK Garden Tool #Bags, UK #Garden Tool Bags Suppliers and Manufacturers ... Products at tools bag, garden #tool set , car tool kit set bag from UK.\nhttps://www.gardentoolbase.co.uk/category/garden-tool-belts\",\"created_time\":\"2017-05-26T06:06:16+0000\",\"picture\":\"https://scontent.xx.fbcdn.net/v/t1.0-0/p130x130/18664226_1854096394854314_7857347206897673508_n.jpg?oh=0567a07e6d2f1a6d6cbb8abc3db6f5f6&oe=5A10BCD5\"},{\"id\":\"1816266855303935_1854066108190676\",\"created_time\":\"2017-05-26T04:14:45+0000\",\"picture\":\"https://scontent.xx.fbcdn.net/v/t15.0-10/p130x130/18577317_840110766145571_8249889700324573184_n.jpg?oh=fc531e2821512e7a715ab5c6bdd5241c&oe=59C64B5C\"},{\"id\":\"1816266855303935_1853294354934518\",\"message\":\"Garden Tie\n\nhttps://www.gardentoolbase.co.uk/category/garden-tie\",\"created_time\":\"2017-05-24T06:52:59+0000\",\"picture\":\"https://scontent.xx.fbcdn.net/v/t1.0-0/s130x130/18700094_1853294354934518_1134467387346276827_n.jpg?oh=01f7a4974495f7c1b16f2277a385d026&oe=5A0E7EFF\"},{\"id\":\"1816266855303935_1853293894934564\",\"message\":\"Brooms\nFind here details of companies selling #Brooms, for your purchase requirements. Get latest info on Brooms, #suppliers, manufacturers, wholesalers, traders with ...\nhttps://www.gardentoolbase.co.uk/category/brooms\",\"created_time\":\"2017-05-24T06:50:50+0000\",\"picture\":\"https://scontent.xx.fbcdn.net/v/t1.0-0/s130x130/18698545_1853293894934564_3281132966230491519_n.jpg?oh=93bfd6d70de91b2539f0e9ae3ff65403&oe=5A03723A\"},{\"id\":\"1816266855303935_1850439065220047\",\"message\":\"Garden Tools Base UK\nWe are a family run business based on the edge of the Peak District and we have a passion for Gardening. Over the years I have purchased many garden tools and products that were used for a season and then needed replacing the following year.\nhttp://www.gardentoolbase.co.uk/\",\"created_time\":\"2017-05-17T07:13:30+0000\",\"picture\":\"https://scontent.xx.fbcdn.net/v/t1.0-0/p130x130/18485730_1850439065220047_3736129976520510785_n.jpg?oh=e55c50862aa41ad5a06c66ce27dfd625&oe=59C9D5A8\"},{\"id\":\"1816266855303935_1837050236558930\",\"message\":\"Childrens garden tools\nChildrens #garden #tools including gift sets, #rakes, #spades, #forks, and #trowels. Kids gardening equipment for both younger and older #children.\nhttps://www.gardentoolbase.co.uk/category/Childrens-garden-tools\",\"created_time\":\"2017-04-20T06:21:25+0000\",\"picture\":\"https://scontent.xx.fbcdn.net/v/t1.0-0/p130x130/18034142_1837050236558930_1700480925549995969_n.png?oh=d17c0d214e2ce93a827eaafdf77f4575&oe=59C944C1\"},{\"id\":\"1816266855303935_1833393490257938\",\"message\":\"Garden Tool belts and Bags\nUK Garden Tool Bags, UK Garden Tool Bags Suppliers and Manufacturers. Products at tools bag ,garden tool set , car tool kit set bag from UK.\nhttps://www.gardentoolbase.co.uk/category/garden-tTool-bags\",\"created_time\":\"2017-04-13T06:06:50+0000\",\"picture\":\"https://scontent.xx.fbcdn.net/v/t1.0-0/p130x130/17759913_1833393490257938_2921008932383413353_n.png?oh=1230de94e57c82167c2750f78549bbd5&oe=5A0FCD23\"},{\"id\":\"1816266855303935_1830777353852885\",\"message\":\"Garden Tools\nOnline Shop for Garden #tools Online in Australia. Buy #Garden Equipment, Garden tool products online at cheap price from ...\nhttps://www.gardentoolbase.co.uk/\",\"created_time\":\"2017-04-08T10:24:03+0000\",\"picture\":\"https://scontent.xx.fbcdn.net/v/t1.0-0/p130x130/17862656_1830777353852885_1188136107737769683_n.png?oh=f110d291dc2e114da7e3cf6549f1a7b8&oe=5A0AEB8C\"},{\"id\":\"1816266855303935_1827739684156652\",\"created_time\":\"2017-04-02T19:26:32+0000\"},{\"id\":\"1816266855303935_1826282930968994\",\"created_time\":\"2017-03-30T22:59:17+0000\"},{\"id\":\"1816266855303935_1826247457639208\",\"created_time\":\"2017-03-30T21:12:43+0000\"},{\"id\":\"1816266855303935_1823833671213920\",\"message\":\"Childrens #garden tools\nChildrens garden tools including gift sets, rakes, spades, #forks, #trowels and hoes. We have a fabulous range of kids #gardening equipment for the younger children and the high quality Gardeners #Tools for older children.....\nhttps://www.gardentoolbase.co.uk/category/Childrens-garden-tools\",\"created_time\":\"2017-03-25T06:30:00+0000\",\"picture\":\"https://scontent.xx.fbcdn.net/v/t1.0-0/s130x130/17522960_1823833671213920_5001589280697399788_n.jpg?oh=455dbaea6538d4157881b079da7d59dd&oe=59FE5550\"},{\"id\":\"1816266855303935_1822986877965266\",\"message\":\"Best Garden Tools in UK\nUK Top Online Garden Tools Sale Website With discounted price on Garden Loppers Garden Secateurs Garden Gloves with Verdemax Garden Tool Brand....\nhttp://www.gardentoolbase.co.uk/\",\"created_time\":\"2017-03-23T09:57:46+0000\",\"picture\":\"https://scontent.xx.fbcdn.net/v/t1.0-0/p130x130/17457507_1822986877965266_2711039866509643487_n.png?oh=dfc65bd60affc9d4ab558229c9631ad0&oe=59CA0A17\"},{\"id\":\"1816266855303935_1822090171388270\",\"message\":\"We are Providing the Best Garden Tools in UK \nUK Top Online Garden Tools Sale Website With discounted price on Garden Loppers Garden Secateurs Garden Gloves with Verdemax Garden Tool Brand...\nhttp://www.gardentoolbase.co.uk/\",\"created_time\":\"2017-03-21T07:46:43+0000\",\"picture\":\"https://scontent.xx.fbcdn.net/v/t1.0-0/p130x130/17424943_1822090171388270_8670678344042820618_n.png?oh=1cb81855b8e015cbf2486e67d818156b&oe=5A0E4C3F\"},{\"id\":\"1816266855303935_1821730444757576\",\"message\":\"Garden Tool Bags\nUK Garden Tool Bags, UK Garden Tool Bags Suppliers and Manufacturers ... Products at tools bag ,garden tool set , car tool kit set bag from UK....\nhttps://www.gardentoolbase.co.uk/category/garden-tTool-bags\",\"created_time\":\"2017-03-20T10:00:03+0000\",\"picture\":\"https://scontent.xx.fbcdn.net/v/t1.0-0/s130x130/17361801_1821730444757576_867370309693649744_n.jpg?oh=2dab54d7b91a067d60a4cabcdf572189&oe=5A011EEB\"}]}");
        //    FacebookData result = new FacebookData();

        //    //act
        //    result.Posts = JsonHelper.Parse(data["data"], new List<FacebookPostData>());

        //    //assert
        //    Assert.IsNotNull(result);
        //    //Assert.AreEqual(result.Id, originalData.Id);
        //    //Assert.AreEqual(result.Name, "Facebook Page");
        //    //Assert.AreEqual(result.FanCount, 123);
        //}


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
    }
}
