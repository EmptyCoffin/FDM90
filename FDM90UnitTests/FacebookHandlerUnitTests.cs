using System;
using FDM90.Handlers;
using FDM90.Models;
using FDM90.Models.Helpers;
using FDM90.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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

        [TestInitialize]
        public void StartUp()
        {
            _mockFacebookCredsRepo = new Mock<IRepository<FacebookCredentials>>();
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>();
            _mockFacebookCredsRepo.Setup(repository => repository.Create(It.IsAny<FacebookCredentials>()))
                .Callback<FacebookCredentials>((cred) => callBackCreds = cred)
                .Verifiable();
            _mockFacebookCredsRepo.Setup(repository => repository.Update(It.IsAny<FacebookCredentials>()))
                .Callback<FacebookCredentials>((cred) => callBackCreds = cred)
                .Verifiable();

            _mockUserHandler = new Mock<IUserHandler>();
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
                .Setup(specific => specific.ReadSpecific(It.IsAny<string>()))
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
                .Setup(specific => specific.ReadSpecific(It.IsAny<string>()))
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
                .Verify(specific => specific.ReadSpecific(It.IsAny<string>()), Times.Once);
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
                .Setup(specific => specific.ReadSpecific(It.IsAny<string>()))
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
                .Setup(specific => specific.ReadSpecific(It.IsAny<string>()))
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
                .Verify(specific => specific.ReadSpecific(It.IsAny<string>()), Times.Once);
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
                .Verify(specific => specific.ReadSpecific(It.IsAny<string>()), Times.Never);
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
            Assert.IsTrue(!string.IsNullOrEmpty(result));
            Assert.AreEqual(permanentAccessToken, result);
        }

        [TestMethod]
        public void SetAccessToken_GivenParameters_ReturnsTrueIfCorrectMethodsCalled()
        {
            //arrange
            Guid newCredGuid = Guid.NewGuid();
            string shortTermToken = "TestShortTerm";
            string newCredPageName = "TestPage";

            //act
            var result = _facebookHandler.SetAccessToken(shortTermToken, newCredGuid, newCredPageName);

            //assert
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetLoginUrl(), Times.Never);
            _mockFacebookClientWrapper.Verify(
                wrapper => wrapper.GetPermanentAccessToken(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mockFacebookClientWrapper.Verify(wrapper => wrapper.GetData(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
            _mockFacebookCredsRepo.As<IReadSpecific<FacebookCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<string>()), Times.Never);
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
    }
}
