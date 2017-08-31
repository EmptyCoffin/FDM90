using System;
using FDM90.Handlers;
using FDM90.Models;
using FDM90.Models.Helpers;
using FDM90.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Facebook;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;

namespace FDM90UnitTests
{
    [TestClass]
    public class FacebookHandlerUnitTests
    {
        private Mock<IRepository<FacebookCredentials>> _mockFacebookCredsRepo;
        private Mock<IUserHandler> _mockUserHandler;
        private Mock<IFacebookClientWrapper> _mockFacebookClientWrapper;
        private FacebookHandler _facebookHandler;
        private string permanentAccessToken = "PermanentAccessToken";
        private FacebookCredentials callBackCreds = null;
        private User callBackUser = null;
        private string callBackMedia = null;
        private string callBackShortTermToken = null;
        private string callBackPageName = null;
        string[] targetProperties = { "Exposure", "Influence", "Engagement" };

        [TestInitialize]
        public void StartUp()
        {
            _mockFacebookCredsRepo = new Mock<IRepository<FacebookCredentials>>();
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>();
            _mockFacebookCredsRepo.As<IReadAll<FacebookCredentials>>();
            _mockFacebookCredsRepo.Setup(repository => repository.Create(It.IsAny<FacebookCredentials>()))
                .Callback<FacebookCredentials>((cred) => callBackCreds = cred)
                .Verifiable();
            _mockFacebookCredsRepo.Setup(repository => repository.Update(It.IsAny<FacebookCredentials>()))
                .Callback<FacebookCredentials>((cred) => callBackCreds = cred)
                .Verifiable();

            _mockUserHandler = new Mock<IUserHandler>();
            _mockUserHandler.Setup(handler => handler.GetUser(It.IsAny<string>())).Returns((string id) => new User(Guid.Parse(id)));
            _mockUserHandler.Setup(handler => handler.UpdateUserMediaActivation(It.IsAny<User>(), It.IsAny<string>()))
                .Callback<User, string>((user, media) =>
                {
                    callBackUser = user;
                    callBackMedia = media;
                });

            _mockFacebookClientWrapper = new Mock<IFacebookClientWrapper>();
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetLoginUrl())
                .Returns(() => "www.testurl.com")
                .Verifiable();
            _mockFacebookClientWrapper.Setup(
                    wrapper => wrapper.GetPermanentAccessToken(It.IsAny<string>(), It.IsAny<string>()))
                    .Callback<string, string>((shortTermToken, pageName) =>
                {
                    callBackShortTermToken = shortTermToken;
                    callBackPageName = pageName;
                })
                .Returns(() => permanentAccessToken)
                .Verifiable();
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData(It.IsAny<string>(), It.IsAny<string>()))
                .Verifiable();

            _facebookHandler = new FacebookHandler(_mockFacebookCredsRepo.Object, _mockUserHandler.Object,
                _mockFacebookClientWrapper.Object);
        }

        [TestCleanup]
        public void CleanUp()
        {
            callBackCreds = null;
            callBackMedia = null;
            callBackPageName = null;
            callBackShortTermToken = null;
            callBackUser = null;
            _mockFacebookCredsRepo = null;
            _mockUserHandler = null;
            _mockFacebookClientWrapper = null;
            _facebookHandler = null;
        }

        [TestMethod]
        public void ConstructorTest_CallToParameterlessConstructor_ReturnsTrueIfObjectIsNotNull()
        {
            //arrange

            //act
            _facebookHandler = new FacebookHandler();

            //assert
            Assert.IsNotNull(_facebookHandler);
        }

        [TestMethod]
        public void GetLogInDetails_ReturningCredsNullAccessToken_ReturnsTrueIfUrlIsGiven()
        {
            //arrange
            FacebookCredentials creds = new FacebookCredentials(Guid.NewGuid(), "TestPage");
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
                .Setup(specific => specific.ReadSpecific(It.IsAny<FacebookCredentials>()))
                .Returns(() => creds);

            //act
            var result = _facebookHandler.GetLogInDetails(creds.UserId);

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(creds.UserId, result.UserId);
            Assert.AreEqual(creds.PageName, result.PageName);
            Assert.IsNotNull(result.PermanentAccessToken);
            Assert.IsTrue(result.PermanentAccessToken.StartsWith("www."));
        }

        [TestMethod]
        public void GetLogInDetails_ReturningCredsNullAccessToken_ReturnsTrueIfWrapperIsCalled()
        {
            //arrange
            FacebookCredentials creds = new FacebookCredentials(Guid.NewGuid(), "TestPage");
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
                .Setup(specific => specific.ReadSpecific(It.IsAny<FacebookCredentials>()))
                .Returns(() => creds)
                .Verifiable();

            //act
            var result = _facebookHandler.GetLogInDetails(creds.UserId);

            //assert
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetLoginUrl(), Times.Once);
            _mockFacebookClientWrapper.Verify(
                wrapper => wrapper.GetPermanentAccessToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetData(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<FacebookCredentials>()), Times.Once);
            _mockFacebookCredsRepo.Verify(specific => specific.Create(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Update(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockUserHandler.Verify(handler => handler.UpdateUserMediaActivation(It.IsAny<User>(), It.IsAny<string>()),
                Times.Never);
        }

        [TestMethod]
        public void GetLogInDetails_ReturningCredsHasAccessToken_ReturnsTrueIfAccessTokenNotUrl()
        {
            //arrange
            FacebookCredentials creds = new FacebookCredentials(Guid.NewGuid(), "TestPage");
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
                .Setup(specific => specific.ReadSpecific(It.IsAny<FacebookCredentials>()))
                .Returns(() =>
                {
                    creds.PermanentAccessToken = "PermanentAccessToken";
                    return creds;
                });

            //act
            var result = _facebookHandler.GetLogInDetails(creds.UserId);

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(creds.UserId, result.UserId);
            Assert.AreEqual(creds.PageName, result.PageName);
            Assert.IsNotNull(result.PermanentAccessToken);
            Assert.IsTrue(!result.PermanentAccessToken.StartsWith("www."));
        }

        [TestMethod]
        public void GetLogInDetails_ReturningCredsHasAccessToken_ReturnsTrueIfWrapperIsNotCalled()
        {
            //arrange
            FacebookCredentials creds = new FacebookCredentials(Guid.NewGuid(), "TestPage");
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
                .Setup(specific => specific.ReadSpecific(It.IsAny<FacebookCredentials>()))
                .Returns(() =>
                {
                    creds.PermanentAccessToken = "PermanentAccessToken";
                    return creds;
                });

            //act
            var result = _facebookHandler.GetLogInDetails(creds.UserId);

            //assert
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetLoginUrl(), Times.Never);
            _mockFacebookClientWrapper.Verify(
                wrapper => wrapper.GetPermanentAccessToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetData(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<FacebookCredentials>()), Times.Once);
            _mockFacebookCredsRepo.Verify(specific => specific.Create(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Update(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockUserHandler.Verify(handler => handler.UpdateUserMediaActivation(It.IsAny<User>(), It.IsAny<string>()),
                Times.Never);
        }

        [TestMethod]
        public void SaveLogInDetails_GivenParameters_ReturnsTrueCorrectCredentialsReturned()
        {
            //arrange
            Guid newCredGuid = Guid.NewGuid();
            string newCredPageName = "TestPage";

            //act
            var result = _facebookHandler.SaveLogInDetails(newCredGuid, newCredPageName);

            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(FacebookCredentials));
            Assert.AreEqual(newCredGuid, result.UserId);
            Assert.AreEqual(newCredPageName, result.PageName);
            Assert.IsNotNull(result.PermanentAccessToken);
            Assert.IsTrue(result.PermanentAccessToken.StartsWith("www."));
        }

        [TestMethod]
        public void SaveLogInDetails_GivenParameters_ReturnsTrueIfCorrectMethodsCalled()
        {
            //arrange
            Guid newCredGuid = Guid.NewGuid();
            string newCredPageName = "TestPage";

            //act
            var result = _facebookHandler.SaveLogInDetails(newCredGuid, newCredPageName);

            //assert
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetLoginUrl(), Times.Once);
            _mockFacebookClientWrapper.Verify(
                wrapper => wrapper.GetPermanentAccessToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetData(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Create(It.IsAny<FacebookCredentials>()), Times.Once);
            _mockFacebookCredsRepo.Verify(specific => specific.Update(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockUserHandler.Verify(handler => handler.UpdateUserMediaActivation(It.IsAny<User>(), It.IsAny<string>()),
                Times.Once);
        }

        [TestMethod]
        public void SaveLogInDetails_GivenParameters_ReturnsTrueIfMethodsCalledWithCorrectValues()
        {
            //arrange
            Guid newCredGuid = Guid.NewGuid();
            string newCredPageName = "TestPage";

            //act
            var result = _facebookHandler.SaveLogInDetails(newCredGuid, newCredPageName);

            //assert
            Assert.AreEqual(newCredGuid, callBackCreds.UserId);
            Assert.AreEqual(newCredPageName, callBackCreds.PageName);

            Assert.AreEqual(newCredGuid, callBackUser.UserId);
            Assert.AreEqual("Facebook", callBackMedia);
        }

        [TestMethod]
        public void SetAccessToken_GivenParameters_ReturnsTrueCorrectIfAccessTokenReturned()
        {
            //arrange
            Guid newCredGuid = Guid.NewGuid();
            string shortTermToken = "TestShortTerm";
            string newCredPageName = "TestPage";

            //act
            var result = _facebookHandler.SetAccessToken(shortTermToken, newCredGuid, newCredPageName);

            //assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void SetAccessToken_GivenParameters_ReturnsTrueIfCorrectMethodsCalled()
        {
            //arrange
            Guid newCredGuid = Guid.NewGuid();
            string shortTermToken = "TestShortTerm";
            string newCredPageName = "TestPage";
            FacebookCredentials creds = new FacebookCredentials(newCredGuid, newCredPageName);
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
                .Setup(specific => specific.ReadSpecific(It.IsAny<FacebookCredentials>()))
                .Returns(() => creds)
                .Verifiable();

            //act
            var result = _facebookHandler.SetAccessToken(shortTermToken, newCredGuid, newCredPageName);
            result.Wait();

            //assert
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetLoginUrl(), Times.Never);
            _mockFacebookClientWrapper.Verify(
                wrapper => wrapper.GetPermanentAccessToken(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetData(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<FacebookCredentials>()), Times.Once);
            _mockFacebookCredsRepo.Verify(specific => specific.Create(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Update(It.IsAny<FacebookCredentials>()), Times.Once);
            _mockUserHandler.Verify(handler => handler.UpdateUserMediaActivation(It.IsAny<User>(), It.IsAny<string>()),
                Times.Never);
        }

        [TestMethod]
        public void SetAccessToken_GivenParameters_ReturnsTrueIfMethodsCalledWithCorrectValues()
        {
            //arrange
            Guid newCredGuid = Guid.NewGuid();
            string shortTermToken = "TestShortTerm";
            string newCredPageName = "TestPage";

            //act
            var result = _facebookHandler.SetAccessToken(shortTermToken, newCredGuid, newCredPageName);

            //assert
            Assert.AreEqual(newCredGuid, callBackCreds.UserId);
            Assert.AreEqual(permanentAccessToken, callBackCreds.PermanentAccessToken);

            Assert.AreEqual(newCredPageName, callBackPageName);
            Assert.AreEqual(shortTermToken, callBackShortTermToken);
        }

        //[TestMethod]
        //public void GetInitialFacebookData_GivenParameters_ReturnsTrueIfValuesAreCorrect()
        //{
        //    //arrange
        //    var mainData = new JsonObject();
        //    mainData.Add("id", "1233456789");
        //    mainData.Add("name", "Test Name");
        //    mainData.Add("fan_count", 951);
        //    mainData.Add("talking_about_count", 159);

        //    var postData = new JsonObject();
        //    postData.Add("id", "123456789_987564321");
        //    postData.Add("message", "This Is A Test Message");

        //    var dataData = new JsonObject();
        //    dataData.Add("data", new JsonArray { postData });

        //    mainData.Add("posts", dataData);

        //    _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData(It.IsAny<string>(), It.IsAny<string>())).Returns(mainData);

        //    string accessToken = "TestShortTerm";

        //    //act
        //    var result = _facebookHandler.GetInitialFacebookData(accessToken);

        //    //assert
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual("1233456789", result.Id);
        //    Assert.AreEqual("Test Name", result.Name);
        //    Assert.AreEqual(951, result.FanCount);
        //    Assert.AreEqual(159, result.TalkingAboutCount);
        //    Assert.AreEqual(1, result.Posts.Count);
        //    Assert.AreEqual("123456789_987564321", result.Posts[0].Id);
        //    Assert.AreEqual("This Is A Test Message", result.Posts[0].Message);
        //    Assert.AreEqual("TestShortTerm", result.AccessToken);
        //}

        //[TestMethod]
        //public void GetPostDetails_GivenParameters_ReturnsTrueIfValuesAreCorrect()
        //{
        //    //arrange
        //    var postInsightData1 = new JsonObject();
        //    postInsightData1.Add("name", "post_impressions_organic_unique");
        //    postInsightData1.Add("period", "Forever");
        //    postInsightData1.Add("title", "Test_This_is");
        //    postInsightData1.Add("description", "Test_Description");
        //    postInsightData1.Add("id", "132456789");
        //    var valueObject = new JsonObject();
        //    valueObject.Add("value", 3);
        //    postInsightData1.Add("values", new JsonArray { valueObject });

        //    var postInsightData2 = new JsonObject();
        //    postInsightData2.Add("name", "post_negative_feedback");
        //    postInsightData2.Add("period", "Forever2");
        //    postInsightData2.Add("title", "Test_This_is2");
        //    postInsightData2.Add("description", "Test_Description2");
        //    postInsightData2.Add("id", "123456789");
        //    var valueObject1 = new JsonObject();
        //    valueObject1.Add("value", 6);
        //    postInsightData2.Add("values", new JsonArray { valueObject1 });

        //    var dataData = new JsonObject();
        //    dataData.Add("data", new JsonArray { postInsightData1, postInsightData2 });
        //    _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData(It.IsAny<string>(), It.IsAny<string>())).Returns(dataData);

        //    //act
        //    var result = _facebookHandler.GetPostDetails(new FacebookData() { Posts = new List<FacebookPostData>() { new FacebookPostData() { Id = "123456789" } } });

        //    //assert
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual(1, result.Posts.Count);
        //    Assert.AreEqual("123456789", result.Posts[0].Id);
        //    Assert.AreEqual("post_impressions_organic_unique", result.Posts[0].TotalReach.Name);
        //    Assert.AreEqual("Forever", result.Posts[0].TotalReach.Period);
        //    Assert.AreEqual(3, result.Posts[0].TotalReach.Values[0].Value);
        //    Assert.AreEqual("post_negative_feedback", result.Posts[0].NegativeFeedback.Name);
        //    Assert.AreEqual("Forever2", result.Posts[0].NegativeFeedback.Period);
        //    Assert.AreEqual(6, result.Posts[0].NegativeFeedback.Values[0].Value);
        //}

        [TestMethod]
        public void GetCampaignInfo_GivenParameters_ReturnsTrueIfValuesAreCorrect()
        {
            //arrange
            FacebookData data = new FacebookData()
            {
                Id = "123456789",
                Name = "Test Name",
                FanCount = 951,
                TalkingAboutCount = 159,
                Posts = new List<FacebookPostData>()
                {
                    new FacebookPostData()
                    {
                        Id = "123456789_987564321",
                        Message = "This Is A Test Message",
                        CreatedTime = new DateTime(2016, 05, 02),
                        TotalReach = new FacebookInsightsData() {
                            Name = "post_impressions_organic_unique",
                            Period = "Forever",
                            Values = new List<FacebookInsightValueData>()
                            {
                                new FacebookInsightValueData()
                                {
                                    Value = 3,
                                    EndTime = new DateTime(2016, 05, 02)
                                }
                            }
                        },
                        EngagedUsers = new FacebookInsightsData() {
                            Name = "post_engaged_users",
                            Period = "Forever",
                            Values = new List<FacebookInsightValueData>()
                            {
                                new FacebookInsightValueData()
                                {
                                    Value = 3,
                                    EndTime = new DateTime(2016, 05, 02)
                                }
                            }
                        }
                    },
                    new FacebookPostData()
                    {
                        Id = "987654321_987564321",
                        Message = "This Is A Test Message",
                        CreatedTime = new DateTime(2016, 05, 03),
                        TotalReach = new FacebookInsightsData() {
                            Name = "post_impressions_organic_unique",
                            Period = "Forever",
                            Values = new List<FacebookInsightValueData>()
                            {
                                new FacebookInsightValueData()
                                {
                                    Value = 3,
                                    EndTime = new DateTime(2016, 05, 03)
                                }
                            }
                        },
                        EngagedUsers = new FacebookInsightsData() {
                            Name = "post_engaged_users",
                            Period = "Forever",
                            Values = new List<FacebookInsightValueData>()
                            {
                                new FacebookInsightValueData()
                                {
                                    Value = 3,
                                    EndTime = new DateTime(2016, 05, 03)
                                }
                            }
                        }
                    }
                },
                PageLikes = new FacebookInsightsData()
                {
                    Name = "page_fans",
                    Period = "Forever",
                    Values = new List<FacebookInsightValueData>()
                            {
                                new FacebookInsightValueData()
                                {
                                    Value = 3,
                                    EndTime = new DateTime(2016, 05, 03)
                                }
                            }
                },
                PageStories = new FacebookInsightsData()
                {
                    Name = "page_stories",
                    Period = "Forever",
                    Values = new List<FacebookInsightValueData>()
                            {
                                new FacebookInsightValueData()
                                {
                                    Value = 3,
                                    EndTime = new DateTime(2016, 05, 03)
                                }
                    }
                }
            };


            FacebookCredentials creds = new FacebookCredentials(Guid.NewGuid(), "TestPage");
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
                .Setup(specific => specific.ReadSpecific(It.IsAny<FacebookCredentials>()))
                .Returns(() =>
                {
                    creds.PermanentAccessToken = "PermanentAccessToken";
                    creds.FacebookData = JsonConvert.SerializeObject(data);
                    return creds;
                });

            //// general page data
            //var mainData = new JsonObject();
            //mainData.Add("id", "1233456789");
            //mainData.Add("name", "Test Name");
            //mainData.Add("fan_count", 951);
            //mainData.Add("talking_about_count", 159);

            //// post data
            //var postData = new JsonObject();
            //postData.Add("id", "123456789_987564321");
            //postData.Add("message", "This Is A Test Message");
            //postData.Add("created_time", "02/05/2016");

            //var postData2 = new JsonObject();
            //postData2.Add("id", "987654321_987564321");
            //postData2.Add("message", "This Is A Test Message");
            //postData2.Add("created_time", "03/05/2016");

            //var dataData = new JsonObject();
            //dataData.Add("data", new JsonArray { postData, postData2 });

            //mainData.Add("posts", dataData);

            //_mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me?fields=posts{id,message,created_time,picture,likes,comments}", It.IsAny<string>())).Returns(mainData);

            //// post reach data
            //var postInsightData1 = new JsonObject();
            //postInsightData1.Add("name", "post_impressions_organic_unique");
            //postInsightData1.Add("period", "Forever");
            //postInsightData1.Add("title", "Test_This_is");
            //postInsightData1.Add("description", "Test_Description");
            //postInsightData1.Add("id", "132456789");
            //var valueObject = new JsonObject();
            //valueObject.Add("value", 3);
            //postInsightData1.Add("values", new JsonArray { valueObject });

            //var postInsightData2 = new JsonObject();
            //postInsightData2.Add("name", "post_impressions_organic_unique");
            //postInsightData2.Add("period", "Forever");
            //postInsightData2.Add("title", "Test_This_is");
            //postInsightData2.Add("description", "Test_Description");
            //postInsightData2.Add("id", "132456789");
            //var valueObject4 = new JsonObject();
            //valueObject4.Add("value", 3);
            //postInsightData2.Add("values", new JsonArray { valueObject4 });

            //var insightData = new JsonObject();
            //insightData.Add("data", new JsonArray { postInsightData1 });
            //_mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/123456789_987564321/insights/post_impressions_organic_unique", It.IsAny<string>())).Returns(insightData);

            //var insightData2 = new JsonObject();
            //insightData2.Add("data", new JsonArray { postInsightData2 });
            //_mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/987654321_987564321/insights/post_impressions_organic_unique", It.IsAny<string>())).Returns(insightData2);

            //// post engaged data
            //var postEngagementData1 = new JsonObject();
            //postEngagementData1.Add("name", "post_engaged_users");
            //postEngagementData1.Add("period", "Forever");
            //postEngagementData1.Add("title", "Test_This_is");
            //postEngagementData1.Add("description", "Test_Description");
            //postEngagementData1.Add("id", "132456789");
            //var valueEngagementObject = new JsonObject();
            //valueEngagementObject.Add("value", 3);
            //postEngagementData1.Add("values", new JsonArray { valueEngagementObject });

            //var postEngagedData = new JsonObject();
            //postEngagedData.Add("data", new JsonArray { postEngagementData1 });
            //_mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/123456789_987564321/insights/post_engaged_users", It.IsAny<string>())).Returns(postEngagedData);

            //var postEngagementData2 = new JsonObject();
            //postEngagementData2.Add("name", "post_engaged_users");
            //postEngagementData2.Add("period", "Forever");
            //postEngagementData2.Add("title", "Test_This_is");
            //postEngagementData2.Add("description", "Test_Description");
            //postEngagementData2.Add("id", "132456789");
            //var valueEngagementObject2 = new JsonObject();
            //valueEngagementObject2.Add("value", 3);
            //postEngagementData2.Add("values", new JsonArray { valueEngagementObject2 });

            //var postEngagedData3 = new JsonObject();
            //postEngagedData3.Add("data", new JsonArray { postEngagementData2 });
            //_mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/987654321_987564321/insights/post_engaged_users", It.IsAny<string>())).Returns(postEngagedData3);

            //// page fans data
            //var likeInsightData1 = new JsonObject();
            //likeInsightData1.Add("name", "page_fans");
            //likeInsightData1.Add("period", "lifetime");
            //var valueObject2 = new JsonObject();
            //valueObject2.Add("value", 3);
            //valueObject2.Add("end_time", "02/05/2016");
            //var valueObject3 = new JsonObject();
            //valueObject3.Add("value", 3);
            //valueObject3.Add("end_time", "03/05/2016");
            //likeInsightData1.Add("values", new JsonArray { valueObject2, valueObject3 });

            //var likeData = new JsonObject();
            //likeData.Add("data", new JsonArray { likeInsightData1 });
            //_mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me/insights/page_fans", It.IsAny<string>())).Returns(likeData);

            //// page stories data
            //var storiesInsightData1 = new JsonObject();
            //storiesInsightData1.Add("name", "page_stories");
            //storiesInsightData1.Add("period", "lifetime");
            //var storiesValue1 = new JsonObject();
            //storiesValue1.Add("value", 3);
            //var storiesValue2 = new JsonObject();
            //storiesValue2.Add("value", 3);
            //storiesInsightData1.Add("values", new JsonArray { storiesValue1, storiesValue2 });

            //var storiesData = new JsonObject();
            //storiesData.Add("data", new JsonArray { storiesInsightData1 });
            //_mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me/insights/page_stories/day", It.IsAny<string>())).Returns(storiesData);

            //act
            var result = _facebookHandler.GetCampaignInfo(Guid.NewGuid(), GetDates(new DateTime(2016, 04, 30), new DateTime(2016, 08, 05)));

            //assert
            Assert.IsNotNull(result);
            foreach (JObject resultObject in result)
            {
                foreach (JProperty properties in resultObject.Properties())
                {
                    Assert.IsTrue(targetProperties.Any(t => t.Contains(properties.Name)));
                    Assert.IsTrue(properties.HasValues);
                    Assert.AreEqual(1, properties.Count);
                    Assert.AreEqual(6, properties.First);
                }
            }
        }


        //[TestMethod]
        //public void GetCampaignInfo_GivenDifferentDatesParameters_ReturnsTrueIfValuesAreCorrect()
        //{
        //    //arrange
        //    FacebookCredentials creds = new FacebookCredentials(Guid.NewGuid(), "TestPage");
        //    _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
        //        .Setup(specific => specific.ReadSpecific(It.IsAny<string>()))
        //        .Returns(() =>
        //        {
        //            creds.PermanentAccessToken = "PermanentAccessToken";
        //            return creds;
        //        });

        //    // general page data
        //    var mainData = new JsonObject();
        //    mainData.Add("id", "1233456789");
        //    mainData.Add("name", "Test Name");
        //    mainData.Add("fan_count", 951);
        //    mainData.Add("talking_about_count", 159);

        //    // post data
        //    var postData = new JsonObject();
        //    postData.Add("id", "123456789_987564321");
        //    postData.Add("message", "This Is A Test Message");
        //    postData.Add("created_time", "02/05/2016");

        //    var postData2 = new JsonObject();
        //    postData2.Add("id", "987654321_987564321");
        //    postData2.Add("message", "This Is A Test Message");
        //    postData2.Add("created_time", "01/05/2016");

        //    var dataData = new JsonObject();
        //    dataData.Add("data", new JsonArray { postData, postData2 });

        //    mainData.Add("posts", dataData);

        //    _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me?fields=posts{id,message,created_time,picture,likes,comments}", It.IsAny<string>())).Returns(mainData);

        //    // post reach data
        //    var postInsightData1 = new JsonObject();
        //    postInsightData1.Add("name", "post_impressions_organic_unique");
        //    postInsightData1.Add("period", "Forever");
        //    postInsightData1.Add("title", "Test_This_is");
        //    postInsightData1.Add("description", "Test_Description");
        //    postInsightData1.Add("id", "132456789");
        //    var valueObject = new JsonObject();
        //    valueObject.Add("value", 3);
        //    postInsightData1.Add("values", new JsonArray { valueObject });

        //    var postInsightData2 = new JsonObject();
        //    postInsightData2.Add("name", "post_impressions_organic_unique");
        //    postInsightData2.Add("period", "Forever");
        //    postInsightData2.Add("title", "Test_This_is");
        //    postInsightData2.Add("description", "Test_Description");
        //    postInsightData2.Add("id", "132456789");
        //    var valueObject4 = new JsonObject();
        //    valueObject4.Add("value", 3);
        //    postInsightData2.Add("values", new JsonArray { valueObject4 });

        //    var insightData = new JsonObject();
        //    insightData.Add("data", new JsonArray { postInsightData1 });
        //    _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/123456789_987564321/insights/post_impressions_organic_unique", It.IsAny<string>())).Returns(insightData);

        //    var insightData2 = new JsonObject();
        //    insightData2.Add("data", new JsonArray { postInsightData2 });
        //    _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/987654321_987564321/insights/post_impressions_organic_unique", It.IsAny<string>())).Returns(insightData2);

        //    // post engaged data
        //    var postEngagementData1 = new JsonObject();
        //    postEngagementData1.Add("name", "post_engaged_users");
        //    postEngagementData1.Add("period", "Forever");
        //    postEngagementData1.Add("title", "Test_This_is");
        //    postEngagementData1.Add("description", "Test_Description");
        //    postEngagementData1.Add("id", "132456789");
        //    var valueEngagementObject = new JsonObject();
        //    valueEngagementObject.Add("value", 3);
        //    postEngagementData1.Add("values", new JsonArray { valueEngagementObject });

        //    var postEngagedData = new JsonObject();
        //    postEngagedData.Add("data", new JsonArray { postEngagementData1 });
        //    _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/123456789_987564321/insights/post_engaged_users", It.IsAny<string>())).Returns(postEngagedData);

        //    var postEngagementData2 = new JsonObject();
        //    postEngagementData2.Add("name", "post_engaged_users");
        //    postEngagementData2.Add("period", "Forever");
        //    postEngagementData2.Add("title", "Test_This_is");
        //    postEngagementData2.Add("description", "Test_Description");
        //    postEngagementData2.Add("id", "132456789");
        //    var valueEngagementObject2 = new JsonObject();
        //    valueEngagementObject2.Add("value", 3);
        //    postEngagementData2.Add("values", new JsonArray { valueEngagementObject2 });

        //    var postEngagedData3 = new JsonObject();
        //    postEngagedData3.Add("data", new JsonArray { postEngagementData2 });
        //    _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/987654321_987564321/insights/post_engaged_users", It.IsAny<string>())).Returns(postEngagedData3);

        //    // page fans data
        //    var likeInsightData1 = new JsonObject();
        //    likeInsightData1.Add("name", "page_fans");
        //    likeInsightData1.Add("period", "lifetime");
        //    var valueObject2 = new JsonObject();
        //    valueObject2.Add("value", 3);
        //    valueObject2.Add("end_time", "02/05/2016");
        //    var valueObject3 = new JsonObject();
        //    valueObject3.Add("value", 3);
        //    valueObject3.Add("end_time", "01/05/2016");
        //    likeInsightData1.Add("values", new JsonArray { valueObject2, valueObject3 });

        //    var likeData = new JsonObject();
        //    likeData.Add("data", new JsonArray { likeInsightData1 });
        //    _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me/insights/page_fans", It.IsAny<string>())).Returns(likeData);

        //    // page stories data
        //    var storiesInsightData1 = new JsonObject();
        //    storiesInsightData1.Add("name", "page_stories");
        //    storiesInsightData1.Add("period", "lifetime");
        //    var storiesValue1 = new JsonObject();
        //    storiesValue1.Add("value", 3);
        //    var storiesValue2 = new JsonObject();
        //    storiesValue2.Add("value", 3);
        //    storiesInsightData1.Add("values", new JsonArray { storiesValue1, storiesValue2 });

        //    var storiesData = new JsonObject();
        //    storiesData.Add("data", new JsonArray { storiesInsightData1 });
        //    _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me/insights/page_stories/day", It.IsAny<string>())).Returns(storiesData);

        //    //act
        //    var result = _facebookHandler.GetCampaignInfo(Guid.NewGuid(), GetDates(new DateTime(2016, 04, 30), new DateTime(2016, 08, 05)));

        //    //assert
        //    Assert.IsNotNull(result);
        //    foreach (JObject resultObject in result)
        //    {
        //        foreach (JProperty properties in resultObject.Properties())
        //        {
        //            Assert.IsTrue(targetProperties.Any(t => t.Contains(properties.Name)));
        //            Assert.IsTrue(properties.HasValues);
        //            Assert.AreEqual(properties.Count, 1);
        //            Assert.AreEqual(properties.First, 3);
        //        }
        //    }
        //}

        private DateTime[] GetDates(DateTime startDate, DateTime endDate)
        {
            List<DateTime> dateList = new List<DateTime>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date <= DateTime.Now.AddDays(-7))
                    dateList.Add(date);
            }
            return dateList.ToArray();
        }
    }
}
