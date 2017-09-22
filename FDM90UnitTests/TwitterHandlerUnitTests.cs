using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FDM90.Handlers;
using Moq;
using FDM90.Repository;
using FDM90.Models;
using FDM90.Models.Helpers;
using System.Collections.Generic;
using System.Linq;
using LinqToTwitter;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Globalization;

namespace FDM90UnitTests
{
    [TestClass]
    public class TwitterHandlerUnitTests
    {
        private Mock<IRepository<TwitterCredentials>> _mockTwitterCredsRepo;
        private Mock<IUserHandler> _mockUserHandler;
        private Mock<ITwitterClientWrapper> _mockTwitterClientWrapper;
        private TwitterCredentials _pastCreateCreds;
        private TwitterCredentials _pastUpdateCreds;
        private TwitterCredentials _pastPostCreds;
        private FDM90.Models.User _pastUser;
        private List<TwitterCredentials> _twitterCredentialsList;
        private Guid _specificGuid;
        private TwitterHandler _twitterHandler;
        static DateTimeFormatInfo dateInfo = DateTimeFormatInfo.CurrentInfo;
        static Calendar calendar = dateInfo.Calendar;
        private Dictionary<string, string> _pastDictionary;

        [TestInitialize]
        public void StartUp()
        {
            _specificGuid = Guid.NewGuid();
            _twitterCredentialsList = new List<TwitterCredentials>()
            {
                new TwitterCredentials()
                {
                    UserId = Guid.NewGuid(),
                    ScreenName = "ScreenName1",
                    AccessToken = "UVdOalpYTnpWRzlyWlc0eA==",
                    AccessTokenSecret = "UVdOalpYTnpWRzlyWlc1VFpXTnlaWFF4"
                },
                new TwitterCredentials()
                {
                    UserId = _specificGuid,
                    ScreenName = "ScreenName2",
                    AccessToken = "UVdOalpYTnpWRzlyWlc0eQ==",
                    AccessTokenSecret = "UVdOalpYTnpWRzlyWlc1VFpXTnlaWFF5"
                },
                new TwitterCredentials()
                {
                    UserId = Guid.NewGuid(),
                    ScreenName = "ScreenName3",
                    AccessToken = "UVdOalpYTnpWRzlyWlc0eg==",
                    AccessTokenSecret = "UVdOalpYTnpWRzlyWlc1VFpXTnlaWFF6"
                },
                new TwitterCredentials()
                {
                    UserId = Guid.NewGuid(),
                    ScreenName = "ScreenName4",
                    AccessToken = "UVdOalpYTnpWRzlyWlc0MA==",
                    AccessTokenSecret = "UVdOalpYTnpWRzlyWlc1VFpXTnlaWFEw"
                }
            };

            _mockTwitterCredsRepo = new Mock<IRepository<TwitterCredentials>>();
            _mockTwitterCredsRepo.As<IReadAll<TwitterCredentials>>().Setup(x => x.ReadAll())
                .Returns(_twitterCredentialsList).Verifiable();
            _mockTwitterCredsRepo.Setup(x => x.Create(It.IsAny<TwitterCredentials>()))
                .Callback((TwitterCredentials creds) => _pastCreateCreds = creds).Verifiable();
            _mockTwitterCredsRepo.Setup(x => x.Update(It.IsAny<TwitterCredentials>()))
                .Callback((TwitterCredentials creds) => _pastUpdateCreds = creds).Verifiable();
            _mockTwitterCredsRepo.As<IReadSpecific<TwitterCredentials>>().Setup(x => x.ReadSpecific(It.IsAny<TwitterCredentials>()))
                .Returns((TwitterCredentials twitterCreds) => _twitterCredentialsList.First(x => x.UserId == twitterCreds.UserId)).Verifiable();

            _mockUserHandler = new Mock<IUserHandler>();
            _mockUserHandler.Setup(x => x.UpdateUserMediaActivation(It.IsAny<FDM90.Models.User>(), "Twitter", It.IsAny<bool>()))
                .Callback((FDM90.Models.User updatingUser, string media, bool active) => _pastUser = updatingUser).Returns(() => null).Verifiable();

            _mockTwitterClientWrapper = new Mock<ITwitterClientWrapper>();

            _twitterHandler = new TwitterHandler(_mockTwitterCredsRepo.Object, _mockUserHandler.Object,
                _mockTwitterClientWrapper.Object);
        }

        [TestCleanup]
        public void CleanUp()
        {
            _pastUser = null;
            _specificGuid = Guid.Empty;
            _twitterCredentialsList = null;
            _pastCreateCreds = null;
            _mockTwitterCredsRepo = null;
            _mockUserHandler = null;
            _mockTwitterClientWrapper = null;
            _twitterHandler = null;
        }

        [TestMethod]
        public void ConstructorTest_CallToParameterlessConstructor_ReturnsTrueIfObjectIsNotNull()
        {
            //arrange

            //act
            _twitterHandler = new TwitterHandler();

            //assert
            Assert.IsNotNull(_twitterHandler);
        }

        [TestMethod]
        public void SaveUserDetails_GivenParameters_ReturnsTrueIfPastValuesAreCorrect()
        {
            // arrange
            string newAccessToken = "TestAccessToken";
            string newAccessTokenSecret = "TestAccessTokenSecret";
            string newScreenName = "TestScreenName";
            string userId = Guid.NewGuid().ToString();
            _mockTwitterCredsRepo.As<IReadSpecific<TwitterCredentials>>().Setup(x => x.ReadSpecific(It.IsAny<TwitterCredentials>()))
                .Returns(() => null).Verifiable();

            // act
            var result = _twitterHandler.SaveUserDetails(newAccessToken, newAccessTokenSecret, newScreenName, userId);
            result.Wait();

            // assert
            Assert.AreEqual(userId, _pastCreateCreds.UserId.ToString());
            Assert.AreEqual(newScreenName, _pastCreateCreds.ScreenName);
            Assert.AreEqual(newAccessToken, _pastCreateCreds.AccessToken);
            Assert.AreEqual(newAccessTokenSecret, _pastCreateCreds.AccessTokenSecret);
            Assert.IsNull(_pastCreateCreds.TwitterData);
            Assert.AreEqual(userId, _pastUser.UserId.ToString());
        }

        [TestMethod]
        public void SaveUserDetails_GivenParameters_ReturnsTrueIfCorrectMethodsCalled()
        {
            // arrange
            string newAccessToken = "TestAccessToken";
            string newAccessTokenSecret = "TestAccessTokenSecret";
            string newScreenName = "TestScreenName";
            string userId = Guid.NewGuid().ToString();
            _mockTwitterCredsRepo.As<IReadSpecific<TwitterCredentials>>().Setup(x => x.ReadSpecific(It.IsAny<TwitterCredentials>()))
                .Returns(() => null).Verifiable();

            // act
            var result = _twitterHandler.SaveUserDetails(newAccessToken, newAccessTokenSecret, newScreenName, userId);
            result.Wait();

            // assert
            _mockTwitterCredsRepo.Verify(x => x.Create(It.IsAny<TwitterCredentials>()), Times.Once);
            _mockTwitterCredsRepo.As<IReadSpecific<TwitterCredentials>>().Verify(x => x.ReadSpecific(It.IsAny<TwitterCredentials>()), Times.Once);

            _mockUserHandler.Verify(x => x.UpdateUserMediaActivation(It.IsAny<FDM90.Models.User>(), "Twitter", It.IsAny<bool>()), Times.Once);
        }

        [TestMethod]
        public void GetMediaData_GivenParameters_ReturnsTrueIfMethodsAreCalled()
        {
            //arrange
            DateTime[] dates = GetDates(DateTime.Now.AddMonths(-1), DateTime.Now.AddDays(-7));
            List<Status> returnedStatus = null;
            _mockTwitterClientWrapper.Setup(x => x.GetTweets(It.IsAny<TwitterCredentials>()))
                .Returns(() =>
                {
                    returnedStatus = GetPastTweets(false, dates).Result;
                    return GetPastTweets(false, dates);
                }).Verifiable();
            _mockTwitterClientWrapper.Setup(x => x.GetRetweeterFollowers(It.IsAny<TwitterCredentials>(), It.IsAny<ulong>()))
                .Returns((TwitterCredentials twitterCreds, ulong statusId) =>
                        GetRetweetFollowers(statusId, returnedStatus.First(x => x.StatusID == statusId).RetweetCount)).Verifiable();

            //act
            _twitterHandler.GetMediaData(_specificGuid, dates);

            //assert
            _mockTwitterClientWrapper.Verify(wrapper => wrapper.GetTweets(It.IsAny<TwitterCredentials>()), Times.Once);
            _mockTwitterClientWrapper.Verify(
                wrapper => wrapper.GetRetweeterFollowers(It.IsAny<TwitterCredentials>(), It.IsAny<ulong>()), Times.Exactly(returnedStatus.Count(x => x.RetweetCount > 0)));
            _mockTwitterClientWrapper.Verify(wrapper => wrapper.PostTweet(It.IsAny<TwitterCredentials>(), It.IsAny<Dictionary<string, string>>()),
                Times.Never);

            _mockTwitterCredsRepo.As<IReadAll<TwitterCredentials>>().Verify(x => x.ReadAll(), Times.Never);
            _mockTwitterCredsRepo.As<IReadSpecific<TwitterCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<TwitterCredentials>()), Times.Once);
            _mockTwitterCredsRepo.Verify(specific => specific.Create(It.IsAny<TwitterCredentials>()), Times.Never);
            _mockTwitterCredsRepo.Verify(specific => specific.Update(It.IsAny<TwitterCredentials>()), Times.Once);
            _mockTwitterCredsRepo.Verify(specific => specific.Delete(It.IsAny<TwitterCredentials>()), Times.Never);

            _mockUserHandler.Verify(handler => handler.UpdateUserMediaActivation(It.IsAny<FDM90.Models.User>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);
        }

        [TestMethod]
        public void GetMediaData_GivenParameters_ReturnsTrueIfMethodsAreGivenCorrectValues()
        {
            //arrange
            List<Status> returnedStatus = null;
            DateTime[] dates = GetDates(DateTime.Now.AddMonths(-1), DateTime.Now.AddDays(-7));

            _mockTwitterClientWrapper.Setup(x => x.GetTweets(It.IsAny<TwitterCredentials>()))
                .Returns(() =>
                {
                    returnedStatus = GetPastTweets(false, dates).Result;
                    return GetPastTweets(false, dates);
                }).Verifiable();
            _mockTwitterClientWrapper.Setup(x => x.GetRetweeterFollowers(It.IsAny<TwitterCredentials>(), It.IsAny<ulong>()))
                .Returns((TwitterCredentials twitterCreds, ulong statusId) =>
                        GetRetweetFollowers(statusId, returnedStatus.First(x => x.StatusID == statusId).RetweetCount)).Verifiable();

            //act
            _twitterHandler.GetMediaData(_specificGuid, dates);

            //assert
            Assert.IsTrue(!string.IsNullOrWhiteSpace(_pastUpdateCreds.TwitterData));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"NumberOfFollowers\"").Count.Equals(returnedStatus.Sum(x => x.RetweetCount) + 1));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"RetweetedUsers\"").Count.Equals(returnedStatus.Count));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"CreatedAt\"").Count.Equals(returnedStatus.Count));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"ScreenName\"").Count.Equals(returnedStatus.Count));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"Tweets\"").Count.Equals(1));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"FavoriteCount\"").Count.Equals(returnedStatus.Count));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"RetweetCount\"").Count.Equals(returnedStatus.Count));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"StatusID\"").Count.Equals(returnedStatus.Count));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"Text\"").Count.Equals(returnedStatus.Count));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"Retweeted\"").Count.Equals(returnedStatus.Count));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"Favorited\"").Count.Equals(returnedStatus.Count));
        }

        [TestMethod]
        public void GetMediaData_GivenValuesNotInDateRange_ReturnsTrueIfMethodsAreCalled()
        {
            //arrange
            List<Status> returnedStatus = null;
            DateTime[] dates = GetDates(DateTime.Now.AddMonths(-1), DateTime.Now.AddDays(-7));

            _mockTwitterClientWrapper.Setup(x => x.GetTweets(It.IsAny<TwitterCredentials>()))
                .Returns(() =>
                {
                    returnedStatus = GetPastTweets(false, dates).Result;
                    return GetPastTweets(false, dates);
                }).Verifiable();
            _mockTwitterClientWrapper.Setup(x => x.GetRetweeterFollowers(It.IsAny<TwitterCredentials>(), It.IsAny<ulong>()))
                .Returns((TwitterCredentials twitterCreds, ulong statusId) =>
                        GetRetweetFollowers(statusId, returnedStatus.First(x => x.StatusID == statusId).RetweetCount)).Verifiable();

            //act
            _twitterHandler.GetMediaData(_specificGuid, new[] { DateTime.Now.Date });

            //assert
            _mockTwitterClientWrapper.Verify(wrapper => wrapper.GetTweets(It.IsAny<TwitterCredentials>()), Times.Once);
            _mockTwitterClientWrapper.Verify(
                wrapper => wrapper.GetRetweeterFollowers(It.IsAny<TwitterCredentials>(), It.IsAny<ulong>()), Times.Never);
            _mockTwitterClientWrapper.Verify(wrapper => wrapper.PostTweet(It.IsAny<TwitterCredentials>(), It.IsAny<Dictionary<string, string>>()),
                Times.Never);

            _mockTwitterCredsRepo.As<IReadAll<TwitterCredentials>>().Verify(x => x.ReadAll(), Times.Never);
            _mockTwitterCredsRepo.As<IReadSpecific<TwitterCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<TwitterCredentials>()), Times.Once);
            _mockTwitterCredsRepo.Verify(specific => specific.Create(It.IsAny<TwitterCredentials>()), Times.Never);
            _mockTwitterCredsRepo.Verify(specific => specific.Update(It.IsAny<TwitterCredentials>()), Times.Once);
            _mockTwitterCredsRepo.Verify(specific => specific.Delete(It.IsAny<TwitterCredentials>()), Times.Never);

            _mockUserHandler.Verify(handler => handler.UpdateUserMediaActivation(It.IsAny<FDM90.Models.User>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);
        }

        [TestMethod]
        public void GetMediaData_GivenValuesNotInDateRange_ReturnsTrueIfMethodsAreGivenCorrectValues()
        {
            //arrange
            List<Status> returnedStatus = null;
            DateTime[] dates = GetDates(DateTime.Now.AddMonths(-1), DateTime.Now.AddDays(-7));

            _mockTwitterClientWrapper.Setup(x => x.GetTweets(It.IsAny<TwitterCredentials>()))
                .Returns(() =>
                {
                    returnedStatus = GetPastTweets(false, dates).Result;
                    return GetPastTweets(false, dates);
                }).Verifiable();
            _mockTwitterClientWrapper.Setup(x => x.GetRetweeterFollowers(It.IsAny<TwitterCredentials>(), It.IsAny<ulong>()))
                .Returns((TwitterCredentials twitterCreds, ulong statusId) =>
                        GetRetweetFollowers(statusId, returnedStatus.First(x => x.StatusID == statusId).RetweetCount)).Verifiable();

            //act
            _twitterHandler.GetMediaData(_specificGuid, new[] { DateTime.Now.Date });

            //assert
            Assert.IsTrue(!string.IsNullOrWhiteSpace(_pastUpdateCreds.TwitterData));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"NumberOfFollowers\"").Count.Equals(1));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"RetweetedUsers\"").Count.Equals(0));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"CreatedAt\"").Count.Equals(0));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"ScreenName\"").Count.Equals(0));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"Tweets\"").Count.Equals(1));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"FavoriteCount\"").Count.Equals(0));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"RetweetCount\"").Count.Equals(0));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"StatusID\"").Count.Equals(0));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"Text\"").Count.Equals(0));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"Retweeted\"").Count.Equals(0));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"Favorited\"").Count.Equals(0));
        }

        [TestMethod]
        public void GetMediaData_GivenValuesOutOfDateRange_ReturnsTrueIfMethodsAreCalled()
        {
            //arrange
            List<Status> returnedStatus = null;
            DateTime[] dates = GetDates(DateTime.Now.AddMonths(-1), DateTime.Now.AddDays(-7));

            _mockTwitterClientWrapper.Setup(x => x.GetTweets(It.IsAny<TwitterCredentials>()))
                .Returns(() =>
                {
                    returnedStatus = GetPastTweets(true, dates).Result;
                    return GetPastTweets(true, dates);
                }).Verifiable();
            _mockTwitterClientWrapper.Setup(x => x.GetRetweeterFollowers(It.IsAny<TwitterCredentials>(), It.IsAny<ulong>()))
                .Returns((TwitterCredentials twitterCreds, ulong statusId) =>
                        GetRetweetFollowers(statusId, returnedStatus.First(x => x.StatusID == statusId).RetweetCount)).Verifiable();

            //act
            _twitterHandler.GetMediaData(_specificGuid, dates);

            //assert
            _mockTwitterClientWrapper.Verify(wrapper => wrapper.GetTweets(It.IsAny<TwitterCredentials>()), Times.Once);
            _mockTwitterClientWrapper.Verify(
                wrapper => wrapper.GetRetweeterFollowers(It.IsAny<TwitterCredentials>(), It.IsAny<ulong>()),
                                Times.Exactly(returnedStatus.Where(x => dates.Select(y => y.Date).Contains(x.CreatedAt.Date)).Count(x => x.RetweetCount > 0)));
            _mockTwitterClientWrapper.Verify(wrapper => wrapper.PostTweet(It.IsAny<TwitterCredentials>(), It.IsAny<Dictionary<string, string>>()),
                Times.Never);

            _mockTwitterCredsRepo.As<IReadAll<TwitterCredentials>>().Verify(x => x.ReadAll(), Times.Never);
            _mockTwitterCredsRepo.As<IReadSpecific<TwitterCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<TwitterCredentials>()), Times.Once);
            _mockTwitterCredsRepo.Verify(specific => specific.Create(It.IsAny<TwitterCredentials>()), Times.Never);
            _mockTwitterCredsRepo.Verify(specific => specific.Update(It.IsAny<TwitterCredentials>()), Times.Once);
            _mockTwitterCredsRepo.Verify(specific => specific.Delete(It.IsAny<TwitterCredentials>()), Times.Never);

            _mockUserHandler.Verify(handler => handler.UpdateUserMediaActivation(It.IsAny<FDM90.Models.User>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);
        }

        [TestMethod]
        public void GetMediaData_GivenValuesOutOfDateRange_ReturnsTrueIfMethodsAreGivenCorrectValues()
        {
            //arrange
            List<Status> returnedStatus = null;
            DateTime[] dates = GetDates(DateTime.Now.AddMonths(-1), DateTime.Now.AddDays(-7));

            _mockTwitterClientWrapper.Setup(x => x.GetTweets(It.IsAny<TwitterCredentials>()))
                .Returns(() =>
                {
                    returnedStatus = GetPastTweets(true, dates).Result;
                    return GetPastTweets(true, dates);
                }).Verifiable();
            _mockTwitterClientWrapper.Setup(x => x.GetRetweeterFollowers(It.IsAny<TwitterCredentials>(), It.IsAny<ulong>()))
                .Returns((TwitterCredentials twitterCreds, ulong statusId) =>
                        GetRetweetFollowers(statusId, returnedStatus.First(x => x.StatusID == statusId).RetweetCount)).Verifiable();

            //act
            _twitterHandler.GetMediaData(_specificGuid, dates);

            //assert
            Assert.IsTrue(!string.IsNullOrWhiteSpace(_pastUpdateCreds.TwitterData));
            Assert.AreNotEqual(Regex.Matches(_pastUpdateCreds.TwitterData, "\"Text\"").Count, returnedStatus.Count);
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"NumberOfFollowers\"").Count
                                            .Equals(returnedStatus.Where(x => dates.Select(y => y.Date).Contains(x.CreatedAt.Date)).Sum(x => x.RetweetCount) + 1));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"RetweetedUsers\"").Count.Equals(returnedStatus
                                            .Where(x => dates.Select(y => y.Date).Contains(x.CreatedAt.Date)).Count()));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"CreatedAt\"").Count.Equals(returnedStatus
                                            .Where(x => dates.Select(y => y.Date).Contains(x.CreatedAt.Date)).Count()));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"ScreenName\"").Count.Equals(returnedStatus
                                                            .Where(x => dates.Select(y => y.Date).Contains(x.CreatedAt.Date)).Count()));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"Tweets\"").Count.Equals(1));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"FavoriteCount\"").Count.Equals(returnedStatus
                                                            .Where(x => dates.Select(y => y.Date).Contains(x.CreatedAt.Date)).Count()));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"RetweetCount\"").Count.Equals(returnedStatus
                                                            .Where(x => dates.Select(y => y.Date).Contains(x.CreatedAt.Date)).Count()));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"StatusID\"").Count.Equals(returnedStatus
                                                            .Where(x => dates.Select(y => y.Date).Contains(x.CreatedAt.Date)).Count()));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"Text\"").Count.Equals(returnedStatus
                                                            .Where(x => dates.Select(y => y.Date).Contains(x.CreatedAt.Date)).Count()));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"Retweeted\"").Count.Equals(returnedStatus
                                                            .Where(x => dates.Select(y => y.Date).Contains(x.CreatedAt.Date)).Count()));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"Favorited\"").Count.Equals(returnedStatus
                                                            .Where(x => dates.Select(y => y.Date).Contains(x.CreatedAt.Date)).Count()));
        }

        [TestMethod]
        public void GetMediaData_GivenParametersAndUserHasData_ReturnsTrueIfMethodsAreCalled()
        {
            //arrange
            List<Status> returnedStatus = null;
            DateTime[] dates = GetDates(DateTime.Now.AddMonths(-1), DateTime.Now.AddDays(-7));
            _twitterCredentialsList.First(x => x.UserId == _specificGuid).TwitterData = GetTwitterDataString();

            _mockTwitterClientWrapper.Setup(x => x.GetTweets(It.IsAny<TwitterCredentials>()))
                .Returns(() =>
                {
                    returnedStatus = GetPastTweets(true, dates).Result;
                    return GetPastTweets(true, dates);
                }).Verifiable();
            _mockTwitterClientWrapper.Setup(x => x.GetRetweeterFollowers(It.IsAny<TwitterCredentials>(), It.IsAny<ulong>()))
                .Returns((TwitterCredentials twitterCreds, ulong statusId) =>
                        GetRetweetFollowers(statusId, returnedStatus.First(x => x.StatusID == statusId).RetweetCount)).Verifiable();

            //act
            _twitterHandler.GetMediaData(_specificGuid, dates);

            //assert
            _mockTwitterClientWrapper.Verify(wrapper => wrapper.GetTweets(It.IsAny<TwitterCredentials>()), Times.Once);
            _mockTwitterClientWrapper.Verify(
                wrapper => wrapper.GetRetweeterFollowers(It.IsAny<TwitterCredentials>(), It.IsAny<ulong>()),
                                Times.Exactly(returnedStatus.Where(x => dates.Select(y => y.Date).Contains(x.CreatedAt.Date)).Count(x => x.RetweetCount > 0)));
            _mockTwitterClientWrapper.Verify(wrapper => wrapper.PostTweet(It.IsAny<TwitterCredentials>(), It.IsAny<Dictionary<string, string>>()),
                Times.Never);

            _mockTwitterCredsRepo.As<IReadAll<TwitterCredentials>>().Verify(x => x.ReadAll(), Times.Never);
            _mockTwitterCredsRepo.As<IReadSpecific<TwitterCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<TwitterCredentials>()), Times.Once);
            _mockTwitterCredsRepo.Verify(specific => specific.Create(It.IsAny<TwitterCredentials>()), Times.Never);
            _mockTwitterCredsRepo.Verify(specific => specific.Update(It.IsAny<TwitterCredentials>()), Times.Once);
            _mockTwitterCredsRepo.Verify(specific => specific.Delete(It.IsAny<TwitterCredentials>()), Times.Never);

            _mockUserHandler.Verify(handler => handler.UpdateUserMediaActivation(It.IsAny<FDM90.Models.User>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);
        }

        [TestMethod]
        public void GetMediaData_GivenParametersAndUserHasData_ReturnsTrueIfMethodsAreGivenCorrectValues()
        {
            //arrange
            List<Status> returnedStatus = null;
            DateTime[] dates = GetDates(DateTime.Now.AddMonths(-1), DateTime.Now.AddDays(-7));
            _twitterCredentialsList.First(x => x.UserId == _specificGuid).TwitterData = GetTwitterDataString();

            _mockTwitterClientWrapper.Setup(x => x.GetTweets(It.IsAny<TwitterCredentials>()))
                .Returns(() =>
                {
                    returnedStatus = GetPastTweets(true, dates).Result;
                    return GetPastTweets(true, dates);
                }).Verifiable();
            _mockTwitterClientWrapper.Setup(x => x.GetRetweeterFollowers(It.IsAny<TwitterCredentials>(), It.IsAny<ulong>()))
                .Returns((TwitterCredentials twitterCreds, ulong statusId) =>
                        GetRetweetFollowers(statusId, returnedStatus.First(x => x.StatusID == statusId).RetweetCount)).Verifiable();

            //act
            _twitterHandler.GetMediaData(_specificGuid, dates);

            //assert
            TwitterData parsedData = TwitterData.Parse(GetTwitterDataString(), new TwitterData());

            Assert.IsTrue(!string.IsNullOrWhiteSpace(_pastUpdateCreds.TwitterData));
            Assert.AreNotEqual(GetTwitterDataString().Length, _pastUpdateCreds.TwitterData.Length);
            Assert.AreNotEqual(Regex.Matches(_pastUpdateCreds.TwitterData, "\"Text\"").Count, returnedStatus.Count + parsedData.Tweets.Count());
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"NumberOfFollowers\"").Count
                                            .Equals(returnedStatus.Where(x => dates.Select(y => y.Date).Contains(x.CreatedAt.Date)).Sum(x => x.RetweetCount) + 1 
                                            + parsedData.Tweets.Sum(x => x.RetweetCount)));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"RetweetedUsers\"").Count.Equals(returnedStatus
                                            .Where(x => dates.Select(y => y.Date).Contains(x.CreatedAt.Date)).Count() + parsedData.Tweets.Count()));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"CreatedAt\"").Count.Equals(returnedStatus
                                            .Where(x => dates.Select(y => y.Date).Contains(x.CreatedAt.Date)).Count() + parsedData.Tweets.Count()));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"ScreenName\"").Count.Equals(returnedStatus
                                                            .Where(x => dates.Select(y => y.Date).Contains(x.CreatedAt.Date)).Count() + parsedData.Tweets.Count()));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"Tweets\"").Count.Equals(1));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"FavoriteCount\"").Count.Equals(returnedStatus
                                                            .Where(x => dates.Select(y => y.Date).Contains(x.CreatedAt.Date)).Count() + parsedData.Tweets.Count()));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"RetweetCount\"").Count.Equals(returnedStatus
                                                            .Where(x => dates.Select(y => y.Date).Contains(x.CreatedAt.Date)).Count() + parsedData.Tweets.Count()));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"StatusID\"").Count.Equals(returnedStatus
                                                            .Where(x => dates.Select(y => y.Date).Contains(x.CreatedAt.Date)).Count() + parsedData.Tweets.Count()));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"Text\"").Count.Equals(returnedStatus
                                                            .Where(x => dates.Select(y => y.Date).Contains(x.CreatedAt.Date)).Count() + parsedData.Tweets.Count()));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"Retweeted\"").Count.Equals(returnedStatus
                                                            .Where(x => dates.Select(y => y.Date).Contains(x.CreatedAt.Date)).Count() + parsedData.Tweets.Count()));
            Assert.IsTrue(Regex.Matches(_pastUpdateCreds.TwitterData, "\"Favorited\"").Count.Equals(returnedStatus
                                                            .Where(x => dates.Select(y => y.Date).Contains(x.CreatedAt.Date)).Count() + parsedData.Tweets.Count()));
        }

        [TestMethod]
        public void GetCampaignInfo_GivenValuesReturned_ReturnsTrueIfValuesAreCorrect()
        {
            //arrange
            TwitterData twitterData = GetTwitterData(false);

            _mockTwitterCredsRepo.As<IReadSpecific<TwitterCredentials>>().Setup(x => x.ReadSpecific(It.IsAny<TwitterCredentials>()))
                .Returns(() => new TwitterCredentials()
                {
                    UserId = _specificGuid,
                    TwitterData = JsonConvert.SerializeObject(twitterData)
                });

            //act
            var result = _twitterHandler.GetCampaignInfo(_specificGuid, GetDates(new DateTime(2016, 04, 30), new DateTime(2016, 08, 05)));

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            AssertTwitterCampaignValues(result, twitterData);
        }

        [TestMethod]
        public void GetCampaignInfo_GivenValuesFromMultipleWeeksReturned_ReturnsTrueIfValuesAreCorrect()
        {
            //arrange
            TwitterData twitterData = GetTwitterData(true);

            _mockTwitterCredsRepo.As<IReadSpecific<TwitterCredentials>>().Setup(x => x.ReadSpecific(It.IsAny<TwitterCredentials>()))
                .Returns(() => new TwitterCredentials()
                {
                    UserId = _specificGuid,
                    TwitterData = JsonConvert.SerializeObject(twitterData)
                });

            //act
            var result = _twitterHandler.GetCampaignInfo(_specificGuid, GetDates(new DateTime(2016, 04, 30), new DateTime(2016, 08, 05)));

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            AssertTwitterCampaignValues(result, twitterData);
        }

        [TestMethod]
        public void GetTweets_GivenParametersAndUserHasData_ReturnsTrueIfMethodsAreCalled()
        {
            //arrange
            List<Status> returnedStatus = null;
            DateTime[] dates = new[] { DateTime.Now.Date };
            _twitterCredentialsList.First(x => x.UserId == _specificGuid).TwitterData = GetTwitterDataString();

            _mockTwitterClientWrapper.Setup(x => x.GetTweets(It.IsAny<TwitterCredentials>()))
                .Returns(() =>
                {
                    returnedStatus = GetPastTweets(false, dates).Result;
                    return GetPastTweets(false, dates);
                }).Verifiable();
            _mockTwitterClientWrapper.Setup(x => x.GetRetweeterFollowers(It.IsAny<TwitterCredentials>(), It.IsAny<ulong>()))
                .Returns((TwitterCredentials twitterCreds, ulong statusId) =>
                        GetRetweetFollowers(statusId, returnedStatus.First(x => x.StatusID == statusId).RetweetCount)).Verifiable();

            //act
            var result = _twitterHandler.GetTweets(_specificGuid.ToString());

            //assert
            _mockTwitterClientWrapper.Verify(wrapper => wrapper.GetTweets(It.IsAny<TwitterCredentials>()), Times.Once);
            _mockTwitterClientWrapper.Verify(
                wrapper => wrapper.GetRetweeterFollowers(It.IsAny<TwitterCredentials>(), It.IsAny<ulong>()),
                                Times.Exactly(returnedStatus.Where(x => dates.Select(y => y.Date).Contains(x.CreatedAt.Date)).Count(x => x.RetweetCount > 0)));
            _mockTwitterClientWrapper.Verify(wrapper => wrapper.PostTweet(It.IsAny<TwitterCredentials>(), It.IsAny<Dictionary<string, string>>()),
                Times.Never);

            _mockTwitterCredsRepo.As<IReadAll<TwitterCredentials>>().Verify(x => x.ReadAll(), Times.Never);
            _mockTwitterCredsRepo.As<IReadSpecific<TwitterCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<TwitterCredentials>()), Times.Once);
            _mockTwitterCredsRepo.Verify(specific => specific.Create(It.IsAny<TwitterCredentials>()), Times.Never);
            _mockTwitterCredsRepo.Verify(specific => specific.Update(It.IsAny<TwitterCredentials>()), Times.Never);
            _mockTwitterCredsRepo.Verify(specific => specific.Delete(It.IsAny<TwitterCredentials>()), Times.Never);

            _mockUserHandler.Verify(handler => handler.UpdateUserMediaActivation(It.IsAny<FDM90.Models.User>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);
        }

        [TestMethod]
        public void GetTweets_GivenParametersAndUserHasData_ReturnsTrueIfMethodsAreGivenCorrectValues()
        {
            //arrange
            List<Status> returnedStatus = null;
            DateTime[] dates = new[] { DateTime.Now.Date };
            _twitterCredentialsList.First(x => x.UserId == _specificGuid).TwitterData = GetTwitterDataString();

            _mockTwitterClientWrapper.Setup(x => x.GetTweets(It.IsAny<TwitterCredentials>()))
                .Returns(() =>
                {
                    returnedStatus = GetPastTweets(false, dates).Result;
                    return GetPastTweets(false, dates);
                }).Verifiable();
            _mockTwitterClientWrapper.Setup(x => x.GetRetweeterFollowers(It.IsAny<TwitterCredentials>(), It.IsAny<ulong>()))
                .Returns((TwitterCredentials twitterCreds, ulong statusId) =>
                        GetRetweetFollowers(statusId, returnedStatus.First(x => x.StatusID == statusId).RetweetCount)).Verifiable();

            //act
            var result = _twitterHandler.GetTweets(_specificGuid.ToString());
            
            //assert
            TwitterData parsedData = TwitterData.Parse(GetTwitterDataString(), new TwitterData());
            Assert.AreNotEqual(returnedStatus.Count, result.Tweets.Count);
            Assert.IsFalse(result.Tweets.All(x => x.CreatedAt.Date == DateTime.Now.Date));
            Assert.IsTrue(result.Tweets.Any(x => x.CreatedAt.Date == DateTime.Now.Date));
        }

        [TestMethod]
        public void GetTweets_GivenParametersAndUserHasNullData_ReturnsTrueIfMethodsAreCalled()
        {
            //arrange
            List<Status> returnedStatus = null;
            DateTime[] dates = new[] { DateTime.Now.Date };

            _mockTwitterClientWrapper.Setup(x => x.GetTweets(It.IsAny<TwitterCredentials>()))
                .Returns(() =>
                {
                    returnedStatus = GetPastTweets(false, dates).Result;
                    return GetPastTweets(false, dates);
                }).Verifiable();
            _mockTwitterClientWrapper.Setup(x => x.GetRetweeterFollowers(It.IsAny<TwitterCredentials>(), It.IsAny<ulong>()))
                .Returns((TwitterCredentials twitterCreds, ulong statusId) =>
                        GetRetweetFollowers(statusId, returnedStatus.First(x => x.StatusID == statusId).RetweetCount)).Verifiable();

            //act
            var result = _twitterHandler.GetTweets(_specificGuid.ToString());

            //assert
            _mockTwitterClientWrapper.Verify(wrapper => wrapper.GetTweets(It.IsAny<TwitterCredentials>()), Times.Once);
            _mockTwitterClientWrapper.Verify(
                wrapper => wrapper.GetRetweeterFollowers(It.IsAny<TwitterCredentials>(), It.IsAny<ulong>()),
                                Times.Exactly(returnedStatus.Where(x => dates.Select(y => y.Date).Contains(x.CreatedAt.Date)).Count(x => x.RetweetCount > 0)));
            _mockTwitterClientWrapper.Verify(wrapper => wrapper.PostTweet(It.IsAny<TwitterCredentials>(), It.IsAny<Dictionary<string, string>>()),
                Times.Never);

            _mockTwitterCredsRepo.As<IReadAll<TwitterCredentials>>().Verify(x => x.ReadAll(), Times.Never);
            _mockTwitterCredsRepo.As<IReadSpecific<TwitterCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<TwitterCredentials>()), Times.Once);
            _mockTwitterCredsRepo.Verify(specific => specific.Create(It.IsAny<TwitterCredentials>()), Times.Never);
            _mockTwitterCredsRepo.Verify(specific => specific.Update(It.IsAny<TwitterCredentials>()), Times.Never);
            _mockTwitterCredsRepo.Verify(specific => specific.Delete(It.IsAny<TwitterCredentials>()), Times.Never);

            _mockUserHandler.Verify(handler => handler.UpdateUserMediaActivation(It.IsAny<FDM90.Models.User>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);
        }

        [TestMethod]
        public void GetTweets_GivenParametersAndUserHasNullData_ReturnsTrueIfMethodsAreGivenCorrectValues()
        {
            //arrange
            List<Status> returnedStatus = null;
            DateTime[] dates = new[] { DateTime.Now.Date };

            _mockTwitterClientWrapper.Setup(x => x.GetTweets(It.IsAny<TwitterCredentials>()))
                .Returns(() =>
                {
                    returnedStatus = GetPastTweets(false, dates).Result;
                    return GetPastTweets(false, dates);
                }).Verifiable();
            _mockTwitterClientWrapper.Setup(x => x.GetRetweeterFollowers(It.IsAny<TwitterCredentials>(), It.IsAny<ulong>()))
                .Returns((TwitterCredentials twitterCreds, ulong statusId) =>
                        GetRetweetFollowers(statusId, returnedStatus.First(x => x.StatusID == statusId).RetweetCount)).Verifiable();

            //act
            var result = _twitterHandler.GetTweets(_specificGuid.ToString());

            //assert
            TwitterData parsedData = TwitterData.Parse(GetTwitterDataString(), new TwitterData());
            Assert.AreEqual(returnedStatus.Count, result.Tweets.Count);
            Assert.IsTrue(result.Tweets.All(x => x.CreatedAt.Date == DateTime.Now.Date));
        }

        [TestMethod]
        public void PostData_GivenParameters_ReturnsTrueIfMethodsAreCalled()
        {
            //arrange
            _mockTwitterClientWrapper.Setup(x => x.PostTweet(It.IsAny<TwitterCredentials>(), It.IsAny<Dictionary<string, string>>()))
                .Callback((TwitterCredentials creds, Dictionary<string, string> pastParams) => {
                    _pastPostCreds = creds;
                    _pastDictionary = pastParams;
                }).Returns(() => null).Verifiable();

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("message", "This is the message to send");
            parameters.Add("picture", "this is a path");

            //act
            _twitterHandler.PostData(parameters, _specificGuid);

            //assert
            _mockTwitterClientWrapper.Verify(wrapper => wrapper.GetTweets(It.IsAny<TwitterCredentials>()), Times.Never);
            _mockTwitterClientWrapper.Verify(
                wrapper => wrapper.GetRetweeterFollowers(It.IsAny<TwitterCredentials>(), It.IsAny<ulong>()), Times.Never);
            _mockTwitterClientWrapper.Verify(wrapper => wrapper.PostTweet(It.IsAny<TwitterCredentials>(), It.IsAny<Dictionary<string, string>>()),
                Times.Once);

            _mockTwitterCredsRepo.As<IReadAll<TwitterCredentials>>().Verify(x => x.ReadAll(), Times.Never);
            _mockTwitterCredsRepo.As<IReadSpecific<TwitterCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<TwitterCredentials>()), Times.Once);
            _mockTwitterCredsRepo.Verify(specific => specific.Create(It.IsAny<TwitterCredentials>()), Times.Never);
            _mockTwitterCredsRepo.Verify(specific => specific.Update(It.IsAny<TwitterCredentials>()), Times.Never);
            _mockTwitterCredsRepo.Verify(specific => specific.Delete(It.IsAny<TwitterCredentials>()), Times.Never);

            _mockUserHandler.Verify(handler => handler.UpdateUserMediaActivation(It.IsAny<FDM90.Models.User>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);
        }

        [TestMethod]
        public void PostData_GivenParameters_ReturnsTrueIfMethodsAreGivenCorrectValues()
        {
            //arrange
            _mockTwitterClientWrapper.Setup(x => x.PostTweet(It.IsAny<TwitterCredentials>(), It.IsAny<Dictionary<string, string>>()))
                .Callback((TwitterCredentials creds, Dictionary<string, string> pastParams) => {
                    _pastPostCreds = creds;
                    _pastDictionary = pastParams;
                }).Returns(() => null).Verifiable();

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("message", "This is the message to send");
            parameters.Add("picture", "this is a path");

            //act
            _twitterHandler.PostData(parameters, _specificGuid);

            //assert
            Assert.AreEqual(parameters, _pastDictionary);
            Assert.AreNotEqual(_pastPostCreds.AccessToken, _twitterCredentialsList.First(x => x.UserId == _specificGuid).AccessToken);
            Assert.AreNotEqual(_pastPostCreds.AccessTokenSecret, _twitterCredentialsList.First(x => x.UserId == _specificGuid).AccessTokenSecret);
        }

        [TestMethod]
        public void DailyUpdate_GivenValues_ReturnsTrueIfCorrectMethodsCalled()
        {
            // arrange
            List<Status> returnedStatus = null;
            DateTime[] dates = GetDates(DateTime.Now.AddDays(-8), DateTime.Now.AddDays(-8));

            _mockTwitterClientWrapper.Setup(x => x.GetTweets(It.IsAny<TwitterCredentials>()))
                .Returns(() =>
                {
                    returnedStatus = GetPastTweets(false, dates).Result;
                    return GetPastTweets(false, dates);
                }).Verifiable();
            _mockTwitterClientWrapper.Setup(x => x.GetRetweeterFollowers(It.IsAny<TwitterCredentials>(), It.IsAny<ulong>()))
                .Returns((TwitterCredentials twitterCreds, ulong statusId) =>
                        GetRetweetFollowers(statusId, returnedStatus.First(x => x.StatusID == statusId).RetweetCount)).Verifiable();

            //act
            Task.WaitAll(_twitterHandler.DailyUpdate().ToArray());

            // assert
            _mockTwitterClientWrapper.Verify(wrapper => wrapper.GetTweets(It.IsAny<TwitterCredentials>()), Times.Exactly(_twitterCredentialsList.Count()));
            _mockTwitterClientWrapper.Verify(
                wrapper => wrapper.GetRetweeterFollowers(It.IsAny<TwitterCredentials>(), It.IsAny<ulong>()), 
                    Times.Exactly((returnedStatus.Count(x => x.RetweetCount > 0)) * _twitterCredentialsList.Count()));
            _mockTwitterClientWrapper.Verify(wrapper => wrapper.PostTweet(It.IsAny<TwitterCredentials>(), It.IsAny<Dictionary<string, string>>()),
                Times.Never);

            _mockTwitterCredsRepo.As<IReadAll<TwitterCredentials>>().Verify(x => x.ReadAll(), Times.Once);
            _mockTwitterCredsRepo.As<IReadSpecific<TwitterCredentials>>()
                .Verify(specific => specific.ReadSpecific(It.IsAny<TwitterCredentials>()), Times.Exactly(_twitterCredentialsList.Count()));
            _mockTwitterCredsRepo.Verify(specific => specific.Create(It.IsAny<TwitterCredentials>()), Times.Never);
            _mockTwitterCredsRepo.Verify(specific => specific.Update(It.IsAny<TwitterCredentials>()), Times.Exactly(_twitterCredentialsList.Count()));
            _mockTwitterCredsRepo.Verify(specific => specific.Delete(It.IsAny<TwitterCredentials>()), Times.Never);

            _mockUserHandler.Verify(handler => handler.UpdateUserMediaActivation(It.IsAny<FDM90.Models.User>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);
        }

        private static TwitterData GetTwitterData(bool getPastWeeks)
        {
            var data = new TwitterData()
            {
                NumberOfFollowers = 75,
                Tweets = new List<Tweet>()
                {
                    new Tweet()
                    {
                        StatusID = 13545351867,
                        CreatedAt = new DateTime(2016, 05, 03),
                         FavoriteCount = 7,
                         RetweetCount = 0,
                         RetweetedUsers = new List<TwitterUser>()
                         {
                             new TwitterUser()
                             {
                                 NumberOfFollowers = 14,
                             },
                             new TwitterUser()
                             {
                                 NumberOfFollowers = 18,
                             }
                         }
                    },
                    new Tweet()
                    {
                        StatusID = 4894154894,
                        CreatedAt = new DateTime(2016, 05, 02),
                         FavoriteCount = 7,
                         RetweetCount = 0,
                         RetweetedUsers = new List<TwitterUser>()
                    },
                    new Tweet()
                    {
                        StatusID = 4894154894,
                        CreatedAt = new DateTime(2016, 05, 02),
                         FavoriteCount = 0,
                         RetweetCount = 0,
                         RetweetedUsers = new List<TwitterUser>()
                    },
                    new Tweet()
                    {
                        StatusID = 284615861,
                        CreatedAt = new DateTime(2016, 05, 03),
                         FavoriteCount = 7,
                         RetweetCount = 5,
                         RetweetedUsers = new List<TwitterUser>()
                         {
                             new TwitterUser()
                             {
                                 NumberOfFollowers = 14,
                             },
                             new TwitterUser()
                             {
                                 NumberOfFollowers = 18,
                             },
                             new TwitterUser()
                             {
                                 NumberOfFollowers = 14,
                             },
                             new TwitterUser()
                             {
                                 NumberOfFollowers = 7,
                             },
                             new TwitterUser()
                             {
                                 NumberOfFollowers = 1,
                             }
                         }
                    },
                    new Tweet()
                    {
                        StatusID = 19794183484,
                        CreatedAt = new DateTime(2016, 05, 02),
                         FavoriteCount = 1,
                         RetweetCount = 1,
                         RetweetedUsers = new List<TwitterUser>()
                         {
                             new TwitterUser()
                             {
                                 NumberOfFollowers = 14,
                             }
                         }
                    },
                }
            };

            if (getPastWeeks)
            {
                data.Tweets.AddRange(new Tweet[] {
                    new Tweet()
                    {
                        StatusID = 6416541305,
                        CreatedAt = new DateTime(2016, 04, 26),
                         FavoriteCount = 10,
                         RetweetCount = 3,
                         RetweetedUsers = new List<TwitterUser>()
                         {
                             new TwitterUser()
                             {
                                 NumberOfFollowers = 11,
                             },
                            new TwitterUser()
                             {
                                 NumberOfFollowers = 7,
                             },
                            new TwitterUser()
                             {
                                 NumberOfFollowers = 34,
                             }
                         }
                    },
                    new Tweet()
                    {
                        StatusID = 1684615846,
                        CreatedAt = new DateTime(2016, 04, 27),
                         FavoriteCount = 3,
                         RetweetCount = 1,
                         RetweetedUsers = new List<TwitterUser>()
                         {
                             new TwitterUser()
                             {
                                 NumberOfFollowers = 80,
                             }
                         }
                    },
                    new Tweet()
                    {
                        StatusID = 1654890348,
                        CreatedAt = new DateTime(2016, 04, 26),
                         FavoriteCount = 1,
                         RetweetCount = 7,
                         RetweetedUsers = new List<TwitterUser>()
                         {
                             new TwitterUser()
                             {
                                 NumberOfFollowers = 35,
                             },
                            new TwitterUser()
                             {
                                 NumberOfFollowers = 24,
                             },
                            new TwitterUser()
                             {
                                 NumberOfFollowers = 9,
                             },
                             new TwitterUser()
                             {
                                 NumberOfFollowers = 17,
                             },
                            new TwitterUser()
                             {
                                 NumberOfFollowers = 2,
                             },
                            new TwitterUser()
                             {
                                 NumberOfFollowers = 75,
                             },
                            new TwitterUser()
                             {
                                 NumberOfFollowers = 58,
                             }
                         }
                    }
            });
            }

            Dictionary<DateTime, int> numberOfFollowersByDate = new Dictionary<DateTime, int>();
            numberOfFollowersByDate.Add(new DateTime(2016, 05, 01), 205);
            numberOfFollowersByDate.Add(new DateTime(2016, 05, 02), 200);
            numberOfFollowersByDate.Add(new DateTime(2016, 05, 03), 210);

            data.NumberOfFollowersByDate = numberOfFollowersByDate;

            return data;
        }

        private string GetTwitterDataString()
        {
            return "{\"NumberOfFollowers\":50,\"Tweets\":[{\"CreatedAt\":\"2017-07-28T14:42:51.9829694+01:00\",\"ScreenName\":null,\"FavoriteCount\":3,\"RetweetCount\":6,\"StatusID\":1254987456,\"Text\":\"This Is Test Tweet 1\",\"Retweeted\":true,\"Favorited\":true,\"RetweetedUsers\":[{\"NumberOfFollowers\":181},{\"NumberOfFollowers\":143},{\"NumberOfFollowers\":405},{\"NumberOfFollowers\":166},{\"NumberOfFollowers\":448},{\"NumberOfFollowers\":493}]},{\"CreatedAt\":\"2017-07-29T14:42:51.9829694+01:00\",\"ScreenName\":null,\"FavoriteCount\":8,\"RetweetCount\":16,\"StatusID\":546158431,\"Text\":\"This Is Test Tweet 2\",\"Retweeted\":true,\"Favorited\":true,\"RetweetedUsers\":[{\"NumberOfFollowers\":181},{\"NumberOfFollowers\":143},{\"NumberOfFollowers\":405},{\"NumberOfFollowers\":166},{\"NumberOfFollowers\":448},{\"NumberOfFollowers\":493},{\"NumberOfFollowers\":25},{\"NumberOfFollowers\":235},{\"NumberOfFollowers\":257},{\"NumberOfFollowers\":241},{\"NumberOfFollowers\":342},{\"NumberOfFollowers\":32},{\"NumberOfFollowers\":2},{\"NumberOfFollowers\":422},{\"NumberOfFollowers\":110},{\"NumberOfFollowers\":215}]},{\"CreatedAt\":\"2017-07-30T14:42:51.9829694+01:00\",\"ScreenName\":null,\"FavoriteCount\":1,\"RetweetCount\":2,\"StatusID\":549845579112,\"Text\":\"This Is Test Tweet 3\",\"Retweeted\":true,\"Favorited\":true,\"RetweetedUsers\":[{\"NumberOfFollowers\":181},{\"NumberOfFollowers\":143}]},{\"CreatedAt\":\"2017-08-01T14:42:51.9829694+01:00\",\"ScreenName\":null,\"FavoriteCount\":8,\"RetweetCount\":8,\"StatusID\":32154578612,\"Text\":\"This Is Test Tweet 4\",\"Retweeted\":true,\"Favorited\":true,\"RetweetedUsers\":[{\"NumberOfFollowers\":181},{\"NumberOfFollowers\":143},{\"NumberOfFollowers\":405},{\"NumberOfFollowers\":166},{\"NumberOfFollowers\":448},{\"NumberOfFollowers\":493},{\"NumberOfFollowers\":25},{\"NumberOfFollowers\":235}]}],\"NumberOfFollowersByDate\":{\"2017-07-28T14:42:51.9829694+01:00\":5,\"2017-07-29T14:42:51.9829694+01:00\":3,\"2017-07-30T14:42:51.9829694+01:00\":0,\"2017-08-01T14:42:51.9829694+01:00\":1}}";
        }

        private Task<List<Status>> GetPastTweets(bool includeOutOfRangeDates, DateTime[] dates)
        {
            Random rdm = new Random();
            int differenceToFirstDate = (DateTime.Now.Date - dates.OrderByDescending(x => x.Date).First()).Days + 1;

            List<Status> list = new List<Status>() {
                new Status()
                {
                    CreatedAt = DateTime.Now.AddDays(dates.Count() == 1 && dates.First().Date.Equals(DateTime.Now.Date) ? 0
                                                        :  -rdm.Next(differenceToFirstDate, differenceToFirstDate < dates.Count() ? 
                                                                        dates.Count() - differenceToFirstDate : dates.Count() + differenceToFirstDate)),
                    FavoriteCount = 3,
                    RetweetCount = 6,
                    StatusID = 1234987456,
                    Text = "This Is Test Tweet 1",
                    User = new LinqToTwitter.User()
                    {
                        FollowersCount = 50
                    }
                },
                new Status()
                {
                    CreatedAt = DateTime.Now.AddDays(dates.Count() == 1 && dates.First().Date.Equals(DateTime.Now.Date) ? 0
                                                        :  -rdm.Next(differenceToFirstDate, differenceToFirstDate < dates.Count() ?
                                                                        dates.Count() - differenceToFirstDate : dates.Count() + differenceToFirstDate)),
                    FavoriteCount = 8,
                    RetweetCount = 16,
                    StatusID = 546198431,
                    Text = "This Is Test Tweet 2",
                    User = new LinqToTwitter.User()
                    {
                        FollowersCount = 50
                    }
                },
                new Status()
                {
                    CreatedAt = DateTime.Now.AddDays(dates.Count() == 1 && dates.First().Date.Equals(DateTime.Now.Date) ? 0
                                                        :  -rdm.Next(differenceToFirstDate, differenceToFirstDate < dates.Count() ?
                                                                        dates.Count() - differenceToFirstDate : dates.Count() + differenceToFirstDate)),
                    FavoriteCount = 1,
                    RetweetCount = 2,
                    StatusID = 549846579112,
                    Text = "This Is Test Tweet 3",
                    User = new LinqToTwitter.User()
                    {
                        FollowersCount = 50
                    }
                },
                new Status()
                {
                    CreatedAt = DateTime.Now.AddDays(dates.Count() == 1 && dates.First().Date.Equals(DateTime.Now.Date) ? 0
                                                        :  -rdm.Next(differenceToFirstDate, differenceToFirstDate < dates.Count() ?
                                                                        dates.Count() - differenceToFirstDate : dates.Count() + differenceToFirstDate)),
                    FavoriteCount = 8,
                    RetweetCount = 8,
                    StatusID = 321654578612,
                    Text = "This Is Test Tweet 4",
                    User = new LinqToTwitter.User()
                    {
                        FollowersCount = 50
                    }
                }
            };

            if (includeOutOfRangeDates)
            {
                list.AddRange(new[] {
                                    new Status()
                {
                    CreatedAt = DateTime.Now.AddDays(dates.Count() == 1 && dates.First().Date.Equals(DateTime.Now.Date) ? 0
                                                        :-rdm.Next(dates.Count() + differenceToFirstDate, (dates.Count() + differenceToFirstDate) * 2)),
                    FavoriteCount = 1,
                    RetweetCount = 2,
                    StatusID = 549846579112,
                    Text = "This Is Test Tweet 3",
                    User = new LinqToTwitter.User()
                    {
                        FollowersCount = 50
                    }
                },
                new Status()
                {
                    CreatedAt = DateTime.Now.AddDays(dates.Count() == 1 && dates.First().Date.Equals(DateTime.Now.Date) ? 0
                                                        :-rdm.Next(dates.Count() + differenceToFirstDate, (dates.Count() + differenceToFirstDate) * 2)),
                    FavoriteCount = 8,
                    RetweetCount = 8,
                    StatusID = 321654578612,
                    Text = "This Is Test Tweet 4",
                    User = new LinqToTwitter.User()
                    {
                        FollowersCount = 50
                    }
                }
                });
            }

            return Task.FromResult(list);
        }

        private Task<List<Status>> GetRetweetFollowers(ulong statusId, int retweetCount)
        {
            List<Status> list = new List<Status>();
            Random rdm = new Random();

            for (int i = 0; i < retweetCount; i++)
            {
                list.Add(new Status()
                {
                    StatusID = statusId,
                    User = new LinqToTwitter.User()
                    {
                        FollowersCount = rdm.Next(0, 500)
                    }
                });
            }

            return Task.FromResult(list);
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

        private void AssertTwitterCampaignValues(IJEnumerable<JToken> result, TwitterData twitterData)
        {
            foreach (JObject resultObject in result)
            {
                int exposureValue = 0;
                int influenceValue = 0;
                int engagementValue = 0;
                int acquisitionValue = 0;
                int weekNumber = int.Parse(resultObject.Path.Substring(4));

                for (int i = 0; i < twitterData.Tweets.Where(x => calendar.GetWeekOfYear(x.CreatedAt.Date, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek) == weekNumber)
                                                            .Select(x => x.CreatedAt.Date).Distinct().Count(); i++)
                {
                    var tweets = twitterData.Tweets.Where(x => calendar.GetWeekOfYear(x.CreatedAt.Date, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek) == weekNumber);
                    var date = tweets.Select(x => x.CreatedAt.Date).ToList()[i];

                    exposureValue += (twitterData.NumberOfFollowers * tweets.Count(x => x.CreatedAt.Date == date) +
                                                tweets.Where(x => x.CreatedAt.Date == date).Sum(x => x.RetweetedUsers.Sum(y => y.NumberOfFollowers))) / 10;

                    influenceValue += tweets.Where(x => x.CreatedAt.Date == date).Sum(x => x.RetweetedUsers.Sum(y => y.NumberOfFollowers)) / 10;

                    engagementValue += tweets.Where(x => x.CreatedAt.Date == date).Sum(x => x.RetweetCount) +
                                            tweets.Where(x => x.CreatedAt.Date == date).Sum(x => x.FavoriteCount);
                }


                Assert.AreEqual(exposureValue, resultObject.GetValue("Exposure"));
                Assert.AreEqual(influenceValue, resultObject.GetValue("Influence"));
                Assert.AreEqual(engagementValue, resultObject.GetValue("Engagement"));

                if (resultObject["Acquisition"] != null)
                {
                    for (int i = 0; i < twitterData.NumberOfFollowersByDate.Count; i++)
                    {
                        var date = twitterData.NumberOfFollowersByDate.Select(x => x.Key).ToList();
                        if (i != 0)
                        {
                            acquisitionValue += twitterData.NumberOfFollowersByDate[date[i]] - twitterData.NumberOfFollowersByDate[date[i - 1]];
                        }
                        else
                        {
                            acquisitionValue = 0;
                        }
                    }

                    Assert.AreEqual(acquisitionValue, resultObject.GetValue("Acquisition"));
                }
            }
        }
    }
}
