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
using System.Threading.Tasks;

namespace FDM90UnitTests
{
    [TestClass]
    public class FacebookHandlerUnitTests
    {
        private Mock<IRepository<FacebookCredentials>> _mockFacebookCredsRepo;
        private Mock<IUserHandler> _mockUserHandler;
        private Mock<IFacebookClientWrapper> _mockFacebookClientWrapper;
        private FacebookHandler _facebookHandler;
        private string permanentAccessToken = "VUdWeWJXRnVaVzUwUVdOalpYTnpWRzlyWlc0PQ==";
        private FacebookCredentials callBackCreds = null;
        private FacebookCredentials returningCreds = null;
        private User callBackUser = null;
        private string callBackMedia = null;
        private string callBackShortTermToken = null;
        private string callBackPageName = null;
        string[] targetProperties = { "Exposure", "Influence", "Engagement" };
        private Dictionary<string, string> pastDictionary;
        private string pastAccessToken;
        List<FacebookCredentials> returningCredentials;
        private Mock<IFileHelper> _mockFileHelper;

        [TestInitialize]
        public void StartUp()
        {
            returningCredentials = new List<FacebookCredentials>()
            {
                new FacebookCredentials()
                {
                    UserId = Guid.NewGuid(),
                    PermanentAccessToken = "VkdocGMwbHpRVlJsYzNSVWIydGxiakU9"
                },
                new FacebookCredentials()
                {
                    UserId = Guid.NewGuid(),
                    PermanentAccessToken = "VkdocGMwbHpRVlJsYzNSVWIydGxiakk9"
                },
                new FacebookCredentials()
                {
                    UserId = Guid.NewGuid(),
                    PermanentAccessToken = "VkdocGMwbHpRVlJsYzNSVWIydGxiak09"
                }
            };

            returningCreds = new FacebookCredentials();
            _mockFacebookCredsRepo = new Mock<IRepository<FacebookCredentials>>();
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>().Setup(x => x.ReadSpecific(It.IsAny<FacebookCredentials>()))
                .Returns(() => returningCreds)
                .Verifiable();
            _mockFacebookCredsRepo.As<IReadAll<FacebookCredentials>>().Setup(x => x.ReadAll()).Returns(returningCredentials).Verifiable();
            _mockFacebookCredsRepo.Setup(repository => repository.Create(It.IsAny<FacebookCredentials>()))
                .Callback<FacebookCredentials>((cred) => callBackCreds = cred)
                .Verifiable();
            _mockFacebookCredsRepo.Setup(repository => repository.Update(It.IsAny<FacebookCredentials>()))
                .Callback<FacebookCredentials>((cred) => callBackCreds = cred)
                .Verifiable();
            _mockFacebookCredsRepo.Setup(repository => repository.Delete(It.IsAny<FacebookCredentials>()))
                .Callback<FacebookCredentials>((cred) => callBackCreds = cred)
                .Verifiable();
            _mockUserHandler = new Mock<IUserHandler>();
            _mockUserHandler.Setup(handler => handler.GetUser(It.IsAny<string>())).Returns((string id) => new User(Guid.Parse(id)));
            _mockUserHandler.Setup(handler => handler.UpdateUserMediaActivation(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Callback<User, string, bool>((user, media, active) =>
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
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.PostData(It.IsAny<Dictionary<string, string>>(), It.IsAny<string>()))
                .Callback((Dictionary<string, string> postParams, string accessToken) =>
                {
                    pastDictionary = postParams;
                    pastAccessToken = accessToken;
                }).Returns(() => null)
                .Verifiable();

            _mockFileHelper = new Mock<IFileHelper>();
            _mockFileHelper.Setup(x => x.DeleteFile(It.IsAny<string>())).Verifiable();

            _facebookHandler = new FacebookHandler(_mockFacebookCredsRepo.Object, _mockUserHandler.Object,
                _mockFacebookClientWrapper.Object, _mockFileHelper.Object);
        }

        [TestCleanup]
        public void CleanUp()
        {
            _mockFileHelper = null;
            pastDictionary = null;
            pastAccessToken = null;
            returningCredentials = null;
            returningCreds = null;
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
        public void GetLogInDetails_ReturningCredsHasAccessToken_ReturnsTrueIfAccessTokenNotUrl()
        {
            //arrange
            FacebookCredentials creds = new FacebookCredentials(Guid.NewGuid(), "TestPage");
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
                .Setup(specific => specific.ReadSpecific(It.IsAny<FacebookCredentials>()))
                .Returns(() =>
                {
                    creds.PermanentAccessToken = "VUdWeWJXRnVaVzUwUVdOalpYTnpWRzlyWlc0PQ==";
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
                    creds.PermanentAccessToken = "VUdWeWJXRnVaVzUwUVdOalpYTnpWRzlyWlc0PQ==";
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
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.PostData(It.IsAny<Dictionary<string, string>>(), It.IsAny<string>()),
                Times.Never);

            _mockFacebookCredsRepo.As<IReadAll<FacebookCredentials>>().Verify(x => x.ReadAll(), Times.Never);
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<FacebookCredentials>()), Times.Once);
            _mockFacebookCredsRepo.Verify(specific => specific.Create(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Update(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Delete(It.IsAny<FacebookCredentials>()), Times.Never);

            _mockUserHandler.Verify(handler => handler.UpdateUserMediaActivation(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);

            _mockFileHelper.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Never());
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
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.PostData(It.IsAny<Dictionary<string, string>>(), It.IsAny<string>()),
                Times.Never);

            _mockFacebookCredsRepo.As<IReadAll<FacebookCredentials>>().Verify(x => x.ReadAll(), Times.Never);
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Create(It.IsAny<FacebookCredentials>()), Times.Once);
            _mockFacebookCredsRepo.Verify(specific => specific.Update(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Delete(It.IsAny<FacebookCredentials>()), Times.Never);

            _mockUserHandler.Verify(handler => handler.UpdateUserMediaActivation(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Once);

            _mockFileHelper.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Never());
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
            result.Wait();

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
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.PostData(It.IsAny<Dictionary<string, string>>(), It.IsAny<string>()),
                Times.Never);

            _mockFacebookCredsRepo.As<IReadAll<FacebookCredentials>>().Verify(x => x.ReadAll(), Times.Never);
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<FacebookCredentials>()), Times.Once);
            _mockFacebookCredsRepo.Verify(specific => specific.Create(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Update(It.IsAny<FacebookCredentials>()), Times.Once);
            _mockFacebookCredsRepo.Verify(specific => specific.Delete(It.IsAny<FacebookCredentials>()), Times.Never);

            _mockUserHandler.Verify(handler => handler.UpdateUserMediaActivation(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);

            _mockFileHelper.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Never());
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
            result.Wait();

            //assert
            Assert.AreEqual(newCredGuid, callBackCreds.UserId);
            Assert.AreNotEqual(permanentAccessToken, callBackCreds.PermanentAccessToken);

            Assert.AreEqual(newCredPageName, callBackPageName);
            Assert.AreEqual(shortTermToken, callBackShortTermToken);
        }

        [TestMethod]
        public void SetAccessToken_GivenAccessTokenContainsEmptySpaces_ReturnsTrueIfMethodsCalled()
        {
            //arrange
            Guid newCredGuid = Guid.NewGuid();
            string shortTermToken = "TestShortTerm";
            string newCredPageName = "TestPage";
            _mockFacebookClientWrapper.Setup(
                    wrapper => wrapper.GetPermanentAccessToken(It.IsAny<string>(), It.IsAny<string>()))
                    .Callback<string, string>((pastShortTermToken, pastPageName) =>
                    {
                        callBackShortTermToken = pastShortTermToken;
                        callBackPageName = pastPageName;
                    })
                .Returns(() => "This Is A Value")
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
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.PostData(It.IsAny<Dictionary<string, string>>(), It.IsAny<string>()),
                Times.Never);

            _mockFacebookCredsRepo.As<IReadAll<FacebookCredentials>>().Verify(x => x.ReadAll(), Times.Never);
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Create(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Update(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Delete(It.IsAny<FacebookCredentials>()), Times.Once);

            _mockUserHandler.Verify(handler => handler.UpdateUserMediaActivation(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);

            _mockFileHelper.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Never());

        }

        [TestMethod]
        public void SetAccessToken_GivenAccessTokenContainsEmptySpaces_ReturnsTrueIfMethodsCalledWithCorrectValues()
        {
            //arrange
            Guid newCredGuid = Guid.NewGuid();
            string shortTermToken = "TestShortTerm";
            string newCredPageName = "TestPage";
            _mockFacebookClientWrapper.Setup(
                    wrapper => wrapper.GetPermanentAccessToken(It.IsAny<string>(), It.IsAny<string>()))
                    .Callback<string, string>((pastShortTermToken, pastPageName) =>
                    {
                        callBackShortTermToken = pastShortTermToken;
                        callBackPageName = pastPageName;
                    })
                .Returns(() => "This Is A Value")
                .Verifiable();

            //act
            var result = _facebookHandler.SetAccessToken(shortTermToken, newCredGuid, newCredPageName);
            result.Wait();

            //assert
            Assert.AreEqual(newCredGuid, callBackCreds.UserId);
            Assert.AreEqual(newCredPageName, callBackCreds.PageName);
        }

        [TestMethod]
        public void GetMediaData_GivenAccessTokenIsEmpty_ReturnsTrueIfMethodsAreNotCalled()
        {
            //arrange
            returningCreds.UserId = Guid.NewGuid();

            //act
            _facebookHandler.GetMediaData(Guid.NewGuid(), GetDates(DateTime.Now.AddMonths(-1), DateTime.Now.AddDays(-7)));

            //assert
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetLoginUrl(), Times.Never);
            _mockFacebookClientWrapper.Verify(
                wrapper => wrapper.GetPermanentAccessToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetData(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.PostData(It.IsAny<Dictionary<string, string>>(), It.IsAny<string>()),
                Times.Never);

            _mockFacebookCredsRepo.As<IReadAll<FacebookCredentials>>().Verify(x => x.ReadAll(), Times.Never);
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<FacebookCredentials>()), Times.Once);
            _mockFacebookCredsRepo.Verify(specific => specific.Create(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Update(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Delete(It.IsAny<FacebookCredentials>()), Times.Never);

            _mockUserHandler.Verify(handler => handler.UpdateUserMediaActivation(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);

            _mockFileHelper.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Never());

        }

        [TestMethod]
        public void GetMediaData_GivenParameters_ReturnsTrueIfMethodsAreCalled()
        {
            //arrange
            returningCreds.UserId = Guid.NewGuid();
            returningCreds.PermanentAccessToken = "VkdocGMwbHpRVlJsYzNSVWIydGxiZz09";

            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me?fields=id,name,fan_count,talking_about_count", It.IsAny<string>()))
                        .Returns(GetBasicFacebookData());
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me?fields=posts{id,message,story,created_time,picture,likes,comments,shares}", It.IsAny<string>()))
                        .Returns(GetBasicFacebookDataWithPosts(true, false));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/123456789_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("123456789_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/987654321_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("987654321_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me/insights/page_fan_adds", It.IsAny<string>()))
                .Returns(GetInsightsData("page_fan_adds", false, true));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me/insights/page_stories/day", It.IsAny<string>()))
                .Returns(GetInsightsData("page_stories", false, true));

            //act
            _facebookHandler.GetMediaData(Guid.NewGuid(), GetDates(DateTime.Now.AddMonths(-1), DateTime.Now.AddDays(-7)));

            //assert
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetLoginUrl(), Times.Never);
            _mockFacebookClientWrapper.Verify(
                wrapper => wrapper.GetPermanentAccessToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetData(It.IsAny<string>(), It.IsAny<string>()),
                Times.Exactly(6));
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.PostData(It.IsAny<Dictionary<string, string>>(), It.IsAny<string>()),
                Times.Never);

            _mockFacebookCredsRepo.As<IReadAll<FacebookCredentials>>().Verify(x => x.ReadAll(), Times.Never);
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<FacebookCredentials>()), Times.Once);
            _mockFacebookCredsRepo.Verify(specific => specific.Create(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Update(It.IsAny<FacebookCredentials>()), Times.Once);
            _mockFacebookCredsRepo.Verify(specific => specific.Delete(It.IsAny<FacebookCredentials>()), Times.Never);

            _mockUserHandler.Verify(handler => handler.UpdateUserMediaActivation(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);

            _mockFileHelper.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Never());

        }

        [TestMethod]
        public void GetMediaData_GivenParameters_ReturnsTrueIfMethodsAreGivenCorrectValues()
        {
            //arrange
            returningCreds.UserId = Guid.NewGuid();
            returningCreds.PermanentAccessToken = "VkdocGMwbHpRVlJsYzNSVWIydGxiZz09";

            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me?fields=id,name,fan_count,talking_about_count", It.IsAny<string>()))
                        .Returns(GetBasicFacebookData());
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me?fields=posts{id,message,story,created_time,picture,likes,comments,shares}", It.IsAny<string>()))
                        .Returns(GetBasicFacebookDataWithPosts(true, false));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/123456789_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("123456789_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/987654321_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("987654321_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me/insights/page_fan_adds", It.IsAny<string>()))
                .Returns(GetInsightsData("page_fan_adds", false, true));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me/insights/page_stories/day", It.IsAny<string>()))
                .Returns(GetInsightsData("page_stories", false, true));

            //act
            _facebookHandler.GetMediaData(Guid.NewGuid(), GetDates(DateTime.Now.AddMonths(-1), DateTime.Now.AddDays(-7)));

            //assert
            Assert.IsTrue(!string.IsNullOrWhiteSpace(callBackCreds.FacebookData));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("id"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("name"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("fan_count"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("talking_about_count"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("posts"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("message"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("created_time"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("post_impressions_organic_unique"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("lifetime"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("period"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("posts"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("values"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("post_engaged_users"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("page_fan_adds"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("page_stories"));
        }

        [TestMethod]
        public void GetMediaData_GivenValuesHavePaging_ReturnsTrueIfMethodsAreCalled()
        {
            //arrange
            returningCreds.UserId = Guid.NewGuid();
            returningCreds.PermanentAccessToken = "VkdocGMwbHpRVlJsYzNSVWIydGxiZz09";

            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me?fields=id,name,fan_count,talking_about_count", It.IsAny<string>()))
                        .Returns(GetBasicFacebookData());
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me?fields=posts{id,message,story,created_time,picture,likes,comments,shares}", It.IsAny<string>()))
                        .Returns(GetBasicFacebookDataWithPosts(false, false));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/next-post-page", It.IsAny<string>()))
                .Returns(GetPastPosts());
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/123456789_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("123456789_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/987654321_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("987654321_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/123456789_132456798/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("123456789_132456798"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/987654321_123456789/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("987654321_123456789"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/987654321_132789456/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("987654321_132789456"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me/insights/page_fan_adds", It.IsAny<string>()))
                .Returns(GetInsightsData("page_fan_adds", false, false));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me/insights/page_stories/day", It.IsAny<string>()))
                .Returns(GetInsightsData("page_stories", false, false));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/previous-insight" + "page_fan_adds", It.IsAny<string>()))
                .Returns(GetInsightsData("page_fan_adds", true, false));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/previous-insight" + "page_stories", It.IsAny<string>()))
                .Returns(GetInsightsData("page_stories", true, false));

            //act
            _facebookHandler.GetMediaData(Guid.NewGuid(), GetDates(DateTime.Now.AddMonths(-1), DateTime.Now.AddDays(-7)));

            //assert
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetLoginUrl(), Times.Never);
            _mockFacebookClientWrapper.Verify(
                wrapper => wrapper.GetPermanentAccessToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetData(It.IsAny<string>(), It.IsAny<string>()),
                Times.Exactly(10));
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.PostData(It.IsAny<Dictionary<string, string>>(), It.IsAny<string>()),
                Times.Never);

            _mockFacebookCredsRepo.As<IReadAll<FacebookCredentials>>().Verify(x => x.ReadAll(), Times.Never);
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<FacebookCredentials>()), Times.Once);
            _mockFacebookCredsRepo.Verify(specific => specific.Create(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Update(It.IsAny<FacebookCredentials>()), Times.Once);
            _mockFacebookCredsRepo.Verify(specific => specific.Delete(It.IsAny<FacebookCredentials>()), Times.Never);

            _mockUserHandler.Verify(handler => handler.UpdateUserMediaActivation(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);

            _mockFileHelper.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Never());

        }

        [TestMethod]
        public void GetMediaData_GivenValuesHavePaging_ReturnsTrueIfMethodsAreGivenCorrectValues()
        {
            //arrange
            returningCreds.UserId = Guid.NewGuid();
            returningCreds.PermanentAccessToken = "VkdocGMwbHpRVlJsYzNSVWIydGxiZz09";

            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me?fields=id,name,fan_count,talking_about_count", It.IsAny<string>()))
                        .Returns(GetBasicFacebookData());
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me?fields=posts{id,message,story,created_time,picture,likes,comments,shares}", It.IsAny<string>()))
                        .Returns(GetBasicFacebookDataWithPosts(false, false));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/next-post-page", It.IsAny<string>()))
                .Returns(GetPastPosts());
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/123456789_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("123456789_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/987654321_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("987654321_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/123456789_132456798/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("123456789_132456798"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/987654321_123456789/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("987654321_123456789"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/987654321_132789456/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("987654321_132789456"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me/insights/page_fan_adds", It.IsAny<string>()))
                .Returns(GetInsightsData("page_fan_adds", false, false));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me/insights/page_stories/day", It.IsAny<string>()))
                .Returns(GetInsightsData("page_stories", false, false));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/previous-insight" + "page_fan_adds", It.IsAny<string>()))
                .Returns(GetInsightsData("page_fan_adds", true, false));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/previous-insight" + "page_stories", It.IsAny<string>()))
                .Returns(GetInsightsData("page_stories", true, false));

            //act
            _facebookHandler.GetMediaData(Guid.NewGuid(), GetDates(DateTime.Now.AddMonths(-1), DateTime.Now.AddDays(-7)));

            //assert
            Assert.IsTrue(!string.IsNullOrWhiteSpace(callBackCreds.FacebookData));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("id"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("name"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("fan_count"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("talking_about_count"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("posts"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("message"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("created_time"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("post_impressions_organic_unique"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("lifetime"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("period"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("posts"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("values"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("post_engaged_users"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("page_fan_adds"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("page_stories"));
        }

        [TestMethod]
        public void GetMediaData_GivenParametersAndUserHasData_ReturnsTrueIfMethodsAreCalled()
        {
            //arrange
            returningCreds.UserId = Guid.NewGuid();
            returningCreds.PermanentAccessToken = "VkdocGMwbHpRVlJsYzNSVWIydGxiZz09";
            returningCreds.FacebookData = "{\"access_token\":null,\"id\":\"1233456789\",\"name\":\"Test Name\",\"fan_count\":951,\"new_like_count\":0,\"talking_about_count\":159,\"posts\":[{\"id\":\"123456789_987564321\",\"message\":\"This Is A Test Message\",\"story\":null,\"created_time\":\"2017-08-30T00:00:00+01:00\",\"post_impressions_organic_unique\":{\"name\":\"post_impressions_organic_unique\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"0001-01-01T00:00:00\",\"value\":72433}]},\"post_engaged_users\":{\"name\":\"post_engaged_users\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"0001-01-01T00:00:00\",\"value\":115433}]},\"post_negative_feedback\":null,\"picture\":null,\"likes\":null,\"comments\":null,\"shares\":null},{\"id\":\"987654321_987564321\",\"message\":\"This Is A Test Message\",\"story\":null,\"created_time\":\"2017-08-29T00:00:00+01:00\",\"post_impressions_organic_unique\":{\"name\":\"post_impressions_organic_unique\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"0001-01-01T00:00:00\",\"value\":72433}]},\"post_engaged_users\":{\"name\":\"post_engaged_users\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"0001-01-01T00:00:00\",\"value\":115433}]},\"post_negative_feedback\":null,\"picture\":null,\"likes\":null,\"comments\":null,\"shares\":null},{\"id\":\"123456789_132456798\",\"message\":\"This Is A Test Message\",\"story\":null,\"created_time\":\"2017-08-08T00:00:00+01:00\",\"post_impressions_organic_unique\":{\"name\":\"post_impressions_organic_unique\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"0001-01-01T00:00:00\",\"value\":908813}]},\"post_engaged_users\":{\"name\":\"post_engaged_users\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"0001-01-01T00:00:00\",\"value\":516712}]},\"post_negative_feedback\":null,\"picture\":null,\"likes\":null,\"comments\":null,\"shares\":null}],\"page_fan_adds\":{\"name\":\"page_fan_adds\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"2017-09-01T17:52:08+01:00\",\"value\":108},{\"end_time\":\"2017-08-31T17:52:08+01:00\",\"value\":131}]},\"page_stories\":{\"name\":\"page_stories\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"2017-09-01T17:52:08+01:00\",\"value\":108},{\"end_time\":\"2017-08-31T17:52:08+01:00\",\"value\":131},{\"end_time\":\"2017-08-30T17:52:08+01:00\",\"value\":17},{\"end_time\":\"2017-08-29T17:52:08+01:00\",\"value\":117},{\"end_time\":\"2017-08-28T17:52:08+01:00\",\"value\":126}]}}";

            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me?fields=id,name,fan_count,talking_about_count", It.IsAny<string>()))
                        .Returns(GetBasicFacebookData());
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me?fields=posts{id,message,story,created_time,picture,likes,comments,shares}", It.IsAny<string>()))
                        .Returns(GetBasicFacebookDataWithPosts(true, false));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/123456789_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("123456789_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/987654321_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("987654321_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me/insights/page_fan_adds", It.IsAny<string>()))
                .Returns(GetInsightsData("page_fan_adds", false, true));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me/insights/page_stories/day", It.IsAny<string>()))
                .Returns(GetInsightsData("page_stories", false, true));

            //act
            _facebookHandler.GetMediaData(Guid.NewGuid(), GetDates(DateTime.Now.AddMonths(-1), DateTime.Now.AddDays(-7)));

            //assert
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetLoginUrl(), Times.Never);
            _mockFacebookClientWrapper.Verify(
                wrapper => wrapper.GetPermanentAccessToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetData(It.IsAny<string>(), It.IsAny<string>()),
                Times.Exactly(6));
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.PostData(It.IsAny<Dictionary<string, string>>(), It.IsAny<string>()),
                Times.Never);

            _mockFacebookCredsRepo.As<IReadAll<FacebookCredentials>>().Verify(x => x.ReadAll(), Times.Never);
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<FacebookCredentials>()), Times.Once);
            _mockFacebookCredsRepo.Verify(specific => specific.Create(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Update(It.IsAny<FacebookCredentials>()), Times.Once);
            _mockFacebookCredsRepo.Verify(specific => specific.Delete(It.IsAny<FacebookCredentials>()), Times.Never);

            _mockUserHandler.Verify(handler => handler.UpdateUserMediaActivation(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);
            _mockFileHelper.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void GetMediaData_GivenParametersAndUserHasData_ReturnsTrueIfMethodsAreGivenCorrectValues()
        {
            //arrange
            returningCreds.UserId = Guid.NewGuid();
            returningCreds.PermanentAccessToken = "VkdocGMwbHpRVlJsYzNSVWIydGxiZz09";
            returningCreds.FacebookData = "{\"access_token\":null,\"id\":\"1233456789\",\"name\":\"Test Name\",\"fan_count\":951,\"new_like_count\":0,\"talking_about_count\":159,\"posts\":[{\"id\":\"123456789_987564321\",\"message\":\"This Is A Test Message\",\"story\":null,\"created_time\":\"2017-08-30T00:00:00+01:00\",\"post_impressions_organic_unique\":{\"name\":\"post_impressions_organic_unique\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"0001-01-01T00:00:00\",\"value\":72433}]},\"post_engaged_users\":{\"name\":\"post_engaged_users\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"0001-01-01T00:00:00\",\"value\":115433}]},\"post_negative_feedback\":null,\"picture\":null,\"likes\":null,\"comments\":null,\"shares\":null},{\"id\":\"987654321_987564321\",\"message\":\"This Is A Test Message\",\"story\":null,\"created_time\":\"2017-08-29T00:00:00+01:00\",\"post_impressions_organic_unique\":{\"name\":\"post_impressions_organic_unique\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"0001-01-01T00:00:00\",\"value\":72433}]},\"post_engaged_users\":{\"name\":\"post_engaged_users\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"0001-01-01T00:00:00\",\"value\":115433}]},\"post_negative_feedback\":null,\"picture\":null,\"likes\":null,\"comments\":null,\"shares\":null},{\"id\":\"123456789_132456798\",\"message\":\"This Is A Test Message\",\"story\":null,\"created_time\":\"2017-08-08T00:00:00+01:00\",\"post_impressions_organic_unique\":{\"name\":\"post_impressions_organic_unique\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"0001-01-01T00:00:00\",\"value\":908813}]},\"post_engaged_users\":{\"name\":\"post_engaged_users\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"0001-01-01T00:00:00\",\"value\":516712}]},\"post_negative_feedback\":null,\"picture\":null,\"likes\":null,\"comments\":null,\"shares\":null}],\"page_fan_adds\":{\"name\":\"page_fan_adds\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"2017-09-01T17:52:08+01:00\",\"value\":108},{\"end_time\":\"2017-08-31T17:52:08+01:00\",\"value\":131}]},\"page_stories\":{\"name\":\"page_stories\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"2017-09-01T17:52:08+01:00\",\"value\":108},{\"end_time\":\"2017-08-31T17:52:08+01:00\",\"value\":131},{\"end_time\":\"2017-08-30T17:52:08+01:00\",\"value\":17},{\"end_time\":\"2017-08-29T17:52:08+01:00\",\"value\":117},{\"end_time\":\"2017-08-28T17:52:08+01:00\",\"value\":126}]}}";

            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me?fields=id,name,fan_count,talking_about_count", It.IsAny<string>()))
                        .Returns(GetBasicFacebookData());
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me?fields=posts{id,message,story,created_time,picture,likes,comments,shares}", It.IsAny<string>()))
                        .Returns(GetBasicFacebookDataWithPosts(true, false));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/123456789_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("123456789_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/987654321_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("987654321_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me/insights/page_fan_adds", It.IsAny<string>()))
                .Returns(GetInsightsData("page_fan_adds", false, true));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me/insights/page_stories/day", It.IsAny<string>()))
                .Returns(GetInsightsData("page_stories", false, true));

            //act
            _facebookHandler.GetMediaData(Guid.NewGuid(), GetDates(DateTime.Now.AddMonths(-1), DateTime.Now.AddDays(-7)));

            //assert
            Assert.IsTrue(returningCreds.FacebookData.Length < callBackCreds.FacebookData.Length);
            Assert.IsTrue(callBackCreds.FacebookData.Contains("id"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("name"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("fan_count"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("talking_about_count"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("posts"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("message"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("created_time"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("post_impressions_organic_unique"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("lifetime"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("period"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("posts"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("values"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("post_engaged_users"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("page_fan_adds"));
            Assert.IsTrue(callBackCreds.FacebookData.Contains("page_stories"));
        }

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
                        Message = "This Is A Test Message1",
                        CreatedTime = new DateTime(2016, 05, 02),
                        TotalReach = new FacebookInsightsData() {
                            Name = "post_impressions_organic_unique",
                            Period = "Forever",
                            Values = new List<FacebookInsightValueData>()
                            {
                                new FacebookInsightValueData()
                                {
                                    Value = 6,
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
                                    Value = 6,
                                    EndTime = new DateTime(2016, 05, 02)
                                }
                            }
                        }
                    },
                    new FacebookPostData()
                    {
                        Id = "654789123_987564321",
                        Message = "This Is A Test Message2",
                        CreatedTime = new DateTime(2016, 05, 02),
                        TotalReach = new FacebookInsightsData() {
                            Name = "post_impressions_organic_unique",
                            Period = "Forever",
                            Values = new List<FacebookInsightValueData>()
                            {
                                new FacebookInsightValueData()
                                {
                                    Value = 4,
                                    EndTime = new DateTime(2016, 05, 02)
                                }
                            }
                        },
                        Likes = new List<FacebookFanData>()
                        {
                            new FacebookFanData()
                            {
                                Id = "13698745",
                                Name = "Test Name"
                            }
                        },
                        Comments = new List<FacebookCommentsData>()
                        {
                            new FacebookCommentsData()
                            {
                                Id = "164981688_51343150",
                                Message = "This Is A Test Message"
                            }
                        },
                        Shares = new FacebookSharesData()
                        {
                            Count = 4
                        },
                        EngagedUsers = new FacebookInsightsData() {
                            Name = "post_engaged_users",
                            Period = "Forever",
                            Values = new List<FacebookInsightValueData>()
                            {
                                new FacebookInsightValueData()
                                {
                                    Value = 1,
                                    EndTime = new DateTime(2016, 05, 02)
                                }
                            }
                        }
                    },
                    new FacebookPostData()
                    {
                        Id = "987654321_987564321",
                        Message = "This Is A Test Message3",
                        CreatedTime = new DateTime(2016, 05, 03),
                        TotalReach = new FacebookInsightsData() {
                            Name = "post_impressions_organic_unique",
                            Period = "Forever",
                            Values = new List<FacebookInsightValueData>()
                            {
                                new FacebookInsightValueData()
                                {
                                    Value = 5,
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
                                    Value = 2,
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
                                },
                                new FacebookInsightValueData()
                                {
                                    Value = 1,
                                    EndTime = new DateTime(2016, 05, 02)
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
                    creds.PermanentAccessToken = "VUdWeWJXRnVaVzUwUVdOalpYTnpWRzlyWlc0PQ==";
                    creds.FacebookData = JsonConvert.SerializeObject(data);
                    return creds;
                });

            //act
            var result = _facebookHandler.GetCampaignInfo(Guid.NewGuid(), GetDates(new DateTime(2016, 04, 30), new DateTime(2016, 08, 05)));

            //assert
            Assert.IsNotNull(result);
            foreach (JObject resultObject in result)
            {
                Assert.AreEqual(data.Posts.Sum(x => x.TotalReach.Values[0].Value), resultObject.GetValue("Exposure"));
                Assert.AreEqual(data.Posts.Sum(x => x.Likes?.Count + x.Comments?.Count + x.Shares?.Count)
                                + data.PageLikes.Values.Sum(x => x.Value) + data.PageStories.Values.Sum(x => x.Value), resultObject.GetValue("Influence"));
                Assert.AreEqual(data.Posts.Sum(x => x.EngagedUsers.Values[0].Value), resultObject.GetValue("Engagement"));

                if (resultObject["Acquisition"] != null)
                    Assert.AreEqual(data.PageLikes.Values.Sum(x => x.Value), resultObject.GetValue("Acquisition"));
            }
        }

        [TestMethod]
        public void GetFacebookData_GivenParameters_ReturnsTrueIfCorrectMethodsCalled()
        {
            //arrange
            returningCreds.UserId = Guid.NewGuid();
            returningCreds.PermanentAccessToken = "VkdocGMwbHpRVlJsYzNSVWIydGxiZz09";
            returningCreds.FacebookData = "{\"access_token\":null,\"id\":\"1233456789\",\"name\":\"Test Name\",\"fan_count\":951,\"new_like_count\":0,\"talking_about_count\":159,\"posts\":[{\"id\":\"123456789_987564321\",\"message\":\"This Is A Test Message\",\"story\":null,\"created_time\":\"2017-08-30T00:00:00+01:00\",\"post_impressions_organic_unique\":{\"name\":\"post_impressions_organic_unique\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"0001-01-01T00:00:00\",\"value\":72433}]},\"post_engaged_users\":{\"name\":\"post_engaged_users\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"0001-01-01T00:00:00\",\"value\":115433}]},\"post_negative_feedback\":null,\"picture\":null,\"likes\":null,\"comments\":null,\"shares\":null},{\"id\":\"987654321_987564321\",\"message\":\"This Is A Test Message\",\"story\":null,\"created_time\":\"2017-08-29T00:00:00+01:00\",\"post_impressions_organic_unique\":{\"name\":\"post_impressions_organic_unique\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"0001-01-01T00:00:00\",\"value\":72433}]},\"post_engaged_users\":{\"name\":\"post_engaged_users\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"0001-01-01T00:00:00\",\"value\":115433}]},\"post_negative_feedback\":null,\"picture\":null,\"likes\":null,\"comments\":null,\"shares\":null},{\"id\":\"123456789_132456798\",\"message\":\"This Is A Test Message\",\"story\":null,\"created_time\":\"2017-08-08T00:00:00+01:00\",\"post_impressions_organic_unique\":{\"name\":\"post_impressions_organic_unique\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"0001-01-01T00:00:00\",\"value\":908813}]},\"post_engaged_users\":{\"name\":\"post_engaged_users\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"0001-01-01T00:00:00\",\"value\":516712}]},\"post_negative_feedback\":null,\"picture\":null,\"likes\":null,\"comments\":null,\"shares\":null}],\"page_fan_adds\":{\"name\":\"page_fan_adds\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"2017-09-01T17:52:08+01:00\",\"value\":108},{\"end_time\":\"2017-08-31T17:52:08+01:00\",\"value\":131}]},\"page_stories\":{\"name\":\"page_stories\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"2017-09-01T17:52:08+01:00\",\"value\":108},{\"end_time\":\"2017-08-31T17:52:08+01:00\",\"value\":131},{\"end_time\":\"2017-08-30T17:52:08+01:00\",\"value\":17},{\"end_time\":\"2017-08-29T17:52:08+01:00\",\"value\":117},{\"end_time\":\"2017-08-28T17:52:08+01:00\",\"value\":126}]}}";

            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me?fields=id,name,fan_count,talking_about_count", It.IsAny<string>()))
                        .Returns(GetBasicFacebookData());
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me?fields=posts{id,message,story,created_time,picture,likes,comments,shares}", It.IsAny<string>()))
                        .Returns(GetBasicFacebookDataWithPosts(true, false));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/123456789_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("123456789_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/987654321_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("987654321_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me/insights/page_fan_adds", It.IsAny<string>()))
                .Returns(GetInsightsData("page_fan_adds", false, true));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me/insights/page_stories/day", It.IsAny<string>()))
                .Returns(GetInsightsData("page_stories", false, true));

            //act
            var result = _facebookHandler.GetFacebookData(returningCreds.UserId);

            //assert
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetLoginUrl(), Times.Never);
            _mockFacebookClientWrapper.Verify(
                wrapper => wrapper.GetPermanentAccessToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetData(It.IsAny<string>(), It.IsAny<string>()),
                Times.Exactly(3));
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.PostData(It.IsAny<Dictionary<string, string>>(), It.IsAny<string>()),
                Times.Never);

            _mockFacebookCredsRepo.As<IReadAll<FacebookCredentials>>().Verify(x => x.ReadAll(), Times.Never);
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<FacebookCredentials>()), Times.Once);
            _mockFacebookCredsRepo.Verify(specific => specific.Create(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Update(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Delete(It.IsAny<FacebookCredentials>()), Times.Never);

            _mockUserHandler.Verify(handler => handler.UpdateUserMediaActivation(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);

            _mockFileHelper.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Never());

        }

        [TestMethod]
        public void GetFacebookData_GivenParametersAndUserHasData_ReturnsTrueIfValuesAreUpdated()
        {
            //arrange
            returningCreds.UserId = Guid.NewGuid();
            returningCreds.PermanentAccessToken = "VkdocGMwbHpRVlJsYzNSVWIydGxiZz09";
            returningCreds.FacebookData = "{\"access_token\":null,\"id\":\"1233456789\",\"name\":\"Test Name\",\"fan_count\":951,\"new_like_count\":0,\"talking_about_count\":159,\"posts\":[{\"id\":\"123456789_987564321\",\"message\":\"This Is A Test Message\",\"story\":null,\"created_time\":\"2017-08-17T00:00:00+01:00\",\"post_impressions_organic_unique\":{\"name\":\"post_impressions_organic_unique\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"0001-01-01T00:00:00\",\"value\":72433}]},\"post_engaged_users\":{\"name\":\"post_engaged_users\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"0001-01-01T00:00:00\",\"value\":115433}]},\"post_negative_feedback\":null,\"picture\":null,\"likes\":null,\"comments\":null,\"shares\":null},{\"id\":\"987654321_987564321\",\"message\":\"This Is A Test Message\",\"story\":null,\"created_time\":\"2017-08-16T00:00:00+01:00\",\"post_impressions_organic_unique\":{\"name\":\"post_impressions_organic_unique\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"0001-01-01T00:00:00\",\"value\":72433}]},\"post_engaged_users\":{\"name\":\"post_engaged_users\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"0001-01-01T00:00:00\",\"value\":115433}]},\"post_negative_feedback\":null,\"picture\":null,\"likes\":null,\"comments\":null,\"shares\":null},{\"id\":\"123456789_132456798\",\"message\":\"This Is A Test Message\",\"story\":null,\"created_time\":\"2017-08-14T00:00:00+01:00\",\"post_impressions_organic_unique\":{\"name\":\"post_impressions_organic_unique\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"0001-01-01T00:00:00\",\"value\":908813}]},\"post_engaged_users\":{\"name\":\"post_engaged_users\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"0001-01-01T00:00:00\",\"value\":516712}]},\"post_negative_feedback\":null,\"picture\":null,\"likes\":null,\"comments\":null,\"shares\":null}],\"page_fan_adds\":{\"name\":\"page_fan_adds\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"2017-09-01T17:52:08+01:00\",\"value\":108},{\"end_time\":\"2017-08-31T17:52:08+01:00\",\"value\":131}]},\"page_stories\":{\"name\":\"page_stories\",\"period\":\"lifetime\",\"values\":[{\"end_time\":\"2017-09-01T17:52:08+01:00\",\"value\":108},{\"end_time\":\"2017-08-31T17:52:08+01:00\",\"value\":131},{\"end_time\":\"2017-08-30T17:52:08+01:00\",\"value\":17},{\"end_time\":\"2017-08-29T17:52:08+01:00\",\"value\":117},{\"end_time\":\"2017-08-28T17:52:08+01:00\",\"value\":126}]}}";

            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me?fields=id,name,fan_count,talking_about_count", It.IsAny<string>()))
                        .Returns(GetBasicFacebookData());
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me?fields=posts{id,message,story,created_time,picture,likes,comments,shares}", It.IsAny<string>()))
                        .Returns(GetBasicFacebookDataWithPosts(true, true));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/123456789_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("123456789_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/987654321_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("987654321_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/9876544321_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("9876544321_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/9876554321_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("9876554321_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me/insights/page_fan_adds", It.IsAny<string>()))
                .Returns(GetInsightsData("page_fan_adds", false, true));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me/insights/page_stories/day", It.IsAny<string>()))
                .Returns(GetInsightsData("page_stories", false, true));

            //act
            var result = _facebookHandler.GetFacebookData(returningCreds.UserId);

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Posts.Count);
            Assert.IsFalse(result.Posts.All(x => x.CreatedTime.Date == DateTime.Now.Date));
        }

        [TestMethod]
        public void GetFacebookData_GivenCredsNull_ReturnsTrueValueIsNull()
        {
            //arrange
            returningCreds = null;

            //act
            var result = _facebookHandler.GetFacebookData(Guid.NewGuid());

            //assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetFacebookData_GivenCredsNull_ReturnsTrueIfCorrectMethodsAreCalled()
        {
            //arrange
            returningCreds = null;

            //act
            var result = _facebookHandler.GetFacebookData(Guid.NewGuid());

            //assert
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetLoginUrl(), Times.Never);
            _mockFacebookClientWrapper.Verify(
                wrapper => wrapper.GetPermanentAccessToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetData(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.PostData(It.IsAny<Dictionary<string, string>>(), It.IsAny<string>()),
                Times.Never);

            _mockFacebookCredsRepo.As<IReadAll<FacebookCredentials>>().Verify(x => x.ReadAll(), Times.Never);
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<FacebookCredentials>()), Times.Once);
            _mockFacebookCredsRepo.Verify(specific => specific.Create(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Update(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Delete(It.IsAny<FacebookCredentials>()), Times.Never);

            _mockUserHandler.Verify(handler => handler.UpdateUserMediaActivation(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);

            _mockFileHelper.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Never());

        }

        [TestMethod]
        public void GetFacebookData_GivenCredsAccessTokenNull_ReturnsTrueValueIsNull()
        {
            //arrange
            returningCreds.UserId = Guid.NewGuid();

            //act
            var result = _facebookHandler.GetFacebookData(returningCreds.UserId);

            //assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetFacebookData_GivenCredsAccessTokenNull_ReturnsTrueIfCorrectMethodsAreCalled()
        {
            //arrange
            returningCreds.UserId = Guid.NewGuid();

            //act
            var result = _facebookHandler.GetFacebookData(returningCreds.UserId);

            //assert
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetLoginUrl(), Times.Never);
            _mockFacebookClientWrapper.Verify(
                wrapper => wrapper.GetPermanentAccessToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetData(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.PostData(It.IsAny<Dictionary<string, string>>(), It.IsAny<string>()),
                Times.Never);

            _mockFacebookCredsRepo.As<IReadAll<FacebookCredentials>>().Verify(x => x.ReadAll(), Times.Never);
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<FacebookCredentials>()), Times.Once);
            _mockFacebookCredsRepo.Verify(specific => specific.Create(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Update(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Delete(It.IsAny<FacebookCredentials>()), Times.Never);

            _mockUserHandler.Verify(handler => handler.UpdateUserMediaActivation(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);

            _mockFileHelper.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Never());

        }

        [TestMethod]
        public void GetFacebookData_GivenCredsFacebookDataNull_ReturnsOnlyTodaysValue()
        {
            //arrange
            returningCreds.UserId = Guid.NewGuid();
            returningCreds.PermanentAccessToken = "VkdocGMwbHpRVlJsYzNSVWIydGxiZz09";

            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me?fields=id,name,fan_count,talking_about_count", It.IsAny<string>()))
                        .Returns(GetBasicFacebookData());
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me?fields=posts{id,message,story,created_time,picture,likes,comments,shares}", It.IsAny<string>()))
                        .Returns(GetBasicFacebookDataWithPosts(true, true));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/123456789_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("123456789_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/987654321_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("987654321_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/9876544321_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("9876544321_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/9876554321_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("9876554321_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me/insights/page_fan_adds", It.IsAny<string>()))
                .Returns(GetInsightsData("page_fan_adds", false, true));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me/insights/page_stories/day", It.IsAny<string>()))
                .Returns(GetInsightsData("page_stories", false, true));

            //act
            var result = _facebookHandler.GetFacebookData(returningCreds.UserId);

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Posts.Count);
            Assert.IsTrue(result.Posts.All(x => x.CreatedTime.Date == DateTime.Now.Date));
        }

        [TestMethod]
        public void GetFacebookData_GivenCredsFacebookDataNull_ReturnsTrueIfCorrectMethodsAreCalled()
        {
            //arrange
            returningCreds.UserId = Guid.NewGuid();
            returningCreds.PermanentAccessToken = "VkdocGMwbHpRVlJsYzNSVWIydGxiZz09";

            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me?fields=id,name,fan_count,talking_about_count", It.IsAny<string>()))
                        .Returns(GetBasicFacebookData());
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me?fields=posts{id,message,story,created_time,picture,likes,comments,shares}", It.IsAny<string>()))
                        .Returns(GetBasicFacebookDataWithPosts(true, true));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/123456789_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("123456789_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/987654321_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("987654321_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/9876544321_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("9876544321_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/9876554321_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("9876554321_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me/insights/page_fan_adds", It.IsAny<string>()))
                .Returns(GetInsightsData("page_fan_adds", false, true));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me/insights/page_stories/day", It.IsAny<string>()))
                .Returns(GetInsightsData("page_stories", false, true));

            //act
            var result = _facebookHandler.GetFacebookData(returningCreds.UserId);

            //assert
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetLoginUrl(), Times.Never);
            _mockFacebookClientWrapper.Verify(
                wrapper => wrapper.GetPermanentAccessToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetData(It.IsAny<string>(), It.IsAny<string>()),
                Times.Exactly(5));
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.PostData(It.IsAny<Dictionary<string, string>>(), It.IsAny<string>()),
                Times.Never);

            _mockFacebookCredsRepo.As<IReadAll<FacebookCredentials>>().Verify(x => x.ReadAll(), Times.Never);
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<FacebookCredentials>()), Times.Once);
            _mockFacebookCredsRepo.Verify(specific => specific.Create(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Update(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Delete(It.IsAny<FacebookCredentials>()), Times.Never);

            _mockUserHandler.Verify(handler => handler.UpdateUserMediaActivation(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);

            _mockFileHelper.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Never());

        }

        [TestMethod]
        public void PostData_GivenParameters_ReturnsTrueIfMethodPassedCorrectValue()
        {
            // arrange
            Guid specificGuid = Guid.NewGuid();
            returningCreds.UserId = specificGuid;
            returningCreds.PermanentAccessToken = permanentAccessToken;

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("message", "This is the message to send");
            parameters.Add("picture", "this is a path");

            //act
            _facebookHandler.PostData(parameters, specificGuid);

            // assert
            Assert.AreEqual(parameters, pastDictionary);
            Assert.AreNotEqual(returningCreds.PermanentAccessToken, pastAccessToken);
        }

        [TestMethod]
        public void PostData_GivenParameters_ReturnsTrueIfCorrectMethodsCalled()
        {
            // arrange
            Guid specificGuid = Guid.NewGuid();
            returningCreds.UserId = specificGuid;
            returningCreds.PermanentAccessToken = permanentAccessToken;

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("message", "This is the message to send");
            parameters.Add("picture", "this is a path");

            //act
            _facebookHandler.PostData(parameters, specificGuid);

            // assert
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetLoginUrl(), Times.Never);
            _mockFacebookClientWrapper.Verify(
                wrapper => wrapper.GetPermanentAccessToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetData(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.PostData(It.IsAny<Dictionary<string, string>>(), It.IsAny<string>()),
                Times.Once);

            _mockFacebookCredsRepo.As<IReadAll<FacebookCredentials>>().Verify(x => x.ReadAll(), Times.Never);
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<FacebookCredentials>()), Times.Once);
            _mockFacebookCredsRepo.Verify(specific => specific.Create(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Update(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Delete(It.IsAny<FacebookCredentials>()), Times.Never);

            _mockUserHandler.Verify(handler => handler.UpdateUserMediaActivation(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);

            _mockFileHelper.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public void DailyUpdate_GivenValues_ReturnsTrueIfCorrectMethodsCalled()
        {
            // arrange
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>().Setup(x => x.ReadSpecific(It.IsAny<FacebookCredentials>()))
                .Returns((FacebookCredentials creds) => returningCredentials.First(x => x.UserId == creds.UserId));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me?fields=id,name,fan_count,talking_about_count", It.IsAny<string>()))
            .Returns(GetBasicFacebookData());
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me?fields=posts{id,message,story,created_time,picture,likes,comments,shares}", It.IsAny<string>()))
                        .Returns(GetBasicFacebookDataWithPosts(true, true));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/123456789_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("123456789_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/987654321_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("987654321_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/9876544321_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("9876544321_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/9876554321_987564321/insights/post_impressions_organic_unique,post_engaged_users", It.IsAny<string>()))
                .Returns(GetMetricDataForPost("9876554321_987564321"));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me/insights/page_fan_adds", It.IsAny<string>()))
                .Returns(GetInsightsData("page_fan_adds", false, true));
            _mockFacebookClientWrapper.Setup(wrapper => wrapper.GetData("https://graph.facebook.com/v2.8/me/insights/page_stories/day", It.IsAny<string>()))
                .Returns(GetInsightsData("page_stories", false, true));

            //act
            Task.WaitAll(_facebookHandler.DailyUpdate().ToArray());

            // assert
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetLoginUrl(), Times.Never);
            _mockFacebookClientWrapper.Verify(
                wrapper => wrapper.GetPermanentAccessToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetData(It.IsAny<string>(), It.IsAny<string>()),
                Times.Exactly(12));
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.PostData(It.IsAny<Dictionary<string, string>>(), It.IsAny<string>()),
                Times.Never);

            _mockFacebookCredsRepo.As<IReadAll<FacebookCredentials>>().Verify(x => x.ReadAll(), Times.Once);
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<FacebookCredentials>()), Times.Exactly(3));
            _mockFacebookCredsRepo.Verify(specific => specific.Create(It.IsAny<FacebookCredentials>()), Times.Never);
            _mockFacebookCredsRepo.Verify(specific => specific.Update(It.IsAny<FacebookCredentials>()), Times.Exactly(3));
            _mockFacebookCredsRepo.Verify(specific => specific.Delete(It.IsAny<FacebookCredentials>()), Times.Never);

            _mockUserHandler.Verify(handler => handler.UpdateUserMediaActivation(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);

            _mockFileHelper.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Never());

        }

        public JsonObject GetBasicFacebookData()
        {
            var mainData = new JsonObject();
            mainData.Add("id", "1233456789");
            mainData.Add("name", "Test Name");
            mainData.Add("fan_count", 951);
            mainData.Add("talking_about_count", 159);

            return mainData;
        }

        public JsonObject GetBasicFacebookDataWithPosts(bool includePreviousPost, bool includeTodayPosts)
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

            var postData4 = new JsonObject();
            postData4.Add("id", "9876544321_987564321");
            postData4.Add("message", "This Is A Test Message4");
            postData4.Add("created_time", DateTime.Now.Date);

            var postData5 = new JsonObject();
            postData5.Add("id", "9876554321_987564321");
            postData5.Add("message", "This Is A Test Message5");
            postData5.Add("created_time", DateTime.Now.Date);

            var dataData = new JsonObject();
            JsonArray posts = new JsonArray() { postData, postData2 };

            if (includePreviousPost)
                posts.Add(postData3);

            if (includeTodayPosts)
                posts.AddRange(new[] { postData4, postData5 });

            dataData.Add("data", posts);

            var nextData = new JsonObject();
            nextData.Add("next", "https://graph.facebook.com/v2.8/next-post-page");

            dataData.Add("paging", nextData);

            mainData.Add("posts", dataData);

            return mainData;
        }

        public JsonObject GetPastPosts()
        {
            // post data
            var postData = new JsonObject();
            postData.Add("id", "123456789_132456798");
            postData.Add("message", "This Is A Test Message");
            postData.Add("created_time", DateTime.Now.AddMonths(-1).Date);

            var postData2 = new JsonObject();
            postData2.Add("id", "987654321_123456789");
            postData2.Add("message", "This Is A Test Message");
            postData2.Add("created_time", DateTime.Now.AddMonths(-2).Date);

            var postData3 = new JsonObject();
            postData3.Add("id", "987654321_132789456");
            postData3.Add("message", "This Is A Test Message");
            postData3.Add("created_time", DateTime.Now.AddMonths(-3).Date);

            var dataData = new JsonObject();

            dataData.Add("data", new JsonArray() { postData, postData2, postData3 });

            var nextData = new JsonObject();
            nextData.Add("next", "https://graph.facebook.com/v2.8/next-post-page");

            dataData.Add("paging", nextData);

            return dataData;
        }

        public JsonObject GetMetricDataForPost(string postId)
        {
            Random rdm = new Random();

            var valueData = new JsonObject();
            valueData.Add("value", Math.Ceiling((decimal)rdm.Next(0, 1000000)));

            var metricData = new JsonObject();
            metricData.Add("name", "post_impressions_organic_unique");
            metricData.Add("period", "lifetime");
            metricData.Add("values", new JsonArray { valueData });

            var value2Data = new JsonObject();
            value2Data.Add("value", Math.Ceiling((decimal)rdm.Next(0, 1000000)));

            var metricData2 = new JsonObject();
            metricData2.Add("name", "post_engaged_users");
            metricData2.Add("period", "lifetime");
            metricData2.Add("values", new JsonArray { value2Data });

            var metricsData = new JsonObject();
            metricsData.Add("data", new JsonArray { metricData, metricData2 });

            return metricsData;
        }

        public JsonObject GetInsightsData(string insightName, bool pastAMonth, bool goPastAMonth)
        {
            Random rdm = new Random();

            JsonArray values = new JsonArray();
            for (int i = (pastAMonth ? 20 : 0); i < (pastAMonth ? 35 : goPastAMonth ? 35 : 20); i++)
            {
                var valueData = new JsonObject();
                valueData.Add("value", Math.Ceiling((decimal)rdm.Next(0, 200)));
                valueData.Add("end_time", DateTime.Now.AddDays(-i).ToString("s") + "+0000");
                values.Add(valueData);
            }

            var previousData = new JsonObject();
            previousData.Add("previous", "https://graph.facebook.com/v2.8/previous-insight" + insightName);

            var metricData = new JsonObject();
            metricData.Add("name", insightName);
            metricData.Add("period", "lifetime");
            metricData.Add("values", values);

            var metricsData = new JsonObject();
            metricsData.Add("data", new JsonArray { metricData });
            metricsData.Add("paging", previousData);

            return metricsData;
        }

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
