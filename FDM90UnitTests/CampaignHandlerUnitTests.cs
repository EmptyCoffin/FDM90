using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FDM90.Handlers;
using FDM90.Models;
using Moq;
using FDM90.Repository;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace FDM90UnitTests
{
    [TestClass]
    public class CampaignHandlerUnitTests
    {
        private Mock<IRepository<Campaign>> _mockCampaignRepo;
        private Mock<IFacebookHandler> _mockFacebookHandler;
        private Mock<ITwitterHandler> _mockTwitterHandler;
        private Mock<IUserHandler> _mockUserHandler;
        private string[] _medias = new[] { "Facebook", "Twitter" };

        private CampaignHandler _campaignHandler;
        private static Campaign updatedCampaign;
        private static Campaign createdCampaign;
        private List<Campaign> _returningCampaigns;
        private User _returningUser;
        private JObject _facebookReturner;
        private JObject _twitterReturner;
        private DateTime[] _passedFacebookHandlerDates;
        private DateTime[] _passedTwitterHandlerDates;

        private static DateTimeFormatInfo dateInfo = DateTimeFormatInfo.CurrentInfo;
        private Calendar calendar = dateInfo.Calendar;
        int currentWeekNumber;
        int overallProgressExposure = 0;
        int overallProgressInfluence = 0;
        int overallProgressEngagement = 0;
        int overallTargetExposure = 0;
        int overallTargetInfluence = 0;
        int overallTargetEngagement = 0;

        [TestInitialize]
        public void StartUp()
        {
            _returningCampaigns = new List<Campaign>();
            _returningUser = new User();
            _facebookReturner = new JObject();
            _twitterReturner = new JObject();
            _mockCampaignRepo = new Mock<IRepository<Campaign>>();
            _mockCampaignRepo.Setup(x => x.Create(It.IsAny<Campaign>())).Callback<Campaign>(campaign => createdCampaign = campaign).Verifiable();
            _mockCampaignRepo.Setup(x => x.Update(It.IsAny<Campaign>())).Callback<Campaign>(campaign => updatedCampaign = campaign).Verifiable();
            _mockCampaignRepo.As<IReadMultipleSpecific<Campaign>>().Setup(s => s.ReadMultipleSpecific(It.IsAny<string>())).Returns(_returningCampaigns).Verifiable();
            _mockCampaignRepo.As<IReadAll<Campaign>>().Setup(s => s.ReadAll()).Returns(_returningCampaigns).Verifiable();
            _mockCampaignRepo.As<IReadSpecific<Campaign>>();

            _mockFacebookHandler = new Mock<IFacebookHandler>();
            _mockFacebookHandler.As<IReadAll<FacebookCredentials>>();
            _mockFacebookHandler.Setup(x => x.GetCampaignInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()))
                .Callback<Guid, DateTime[]>((passedGuid, passedDates) => _passedFacebookHandlerDates = passedDates)
                .Returns(_facebookReturner.Values()).Verifiable();
            _mockFacebookHandler.Setup(x => x.MediaName).Returns("Facebook");
            _mockTwitterHandler = new Mock<ITwitterHandler>();
            _mockTwitterHandler.Setup(x => x.GetCampaignInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()))
                .Callback<Guid, DateTime[]>((passedGuid, passedDates) => _passedTwitterHandlerDates = passedDates)
                .Returns(_twitterReturner.Values()).Verifiable();
            _mockTwitterHandler.Setup(x => x.MediaName).Returns("Twitter");
            _mockUserHandler = new Mock<IUserHandler>();
            _mockUserHandler.Setup(x => x.GetUser(It.IsAny<string>())).Returns(_returningUser);
            currentWeekNumber = calendar.GetWeekOfYear(DateTime.Now, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);

            _campaignHandler = new CampaignHandler(_mockCampaignRepo.Object, _mockFacebookHandler.Object, _mockTwitterHandler.Object, _mockUserHandler.Object);
        }

        [TestCleanup]
        public void CleanUp()
        {
            updatedCampaign = null;
            createdCampaign = null;
            _returningCampaigns = null;
            _returningUser = null;
            _facebookReturner = null;
            _twitterReturner = null;
            _passedFacebookHandlerDates = null;
            _passedTwitterHandlerDates = null;

            _mockCampaignRepo = null;
            _mockFacebookHandler = null;
            _mockTwitterHandler = null;
            _mockUserHandler = null;

            _campaignHandler = null;

            dateInfo = DateTimeFormatInfo.CurrentInfo;
            calendar = dateInfo.Calendar;
            currentWeekNumber = -1;
            overallProgressExposure = 0;
            overallProgressInfluence = 0;
            overallProgressEngagement = 0;
            overallTargetExposure = 0;
            overallTargetInfluence = 0;
            overallTargetEngagement = 0;
        }

        [TestMethod]
        public void CampaignHandler_ConstructorTest()
        {
            //act
            _campaignHandler = new CampaignHandler();

            //assert
            Assert.IsNotNull(_campaignHandler);
        }

        [TestMethod]
        public void CreateCampaign_GivenValues_ShouldCallCreateAndUpdate()
        {
            // arrange
            var metricData = new JObject();
            metricData.Add("Exposure", 25);
            metricData.Add("Influence", 24);
            metricData.Add("Engagement", 23);
            metricData.Add("Acquisition", 15);

            _returningUser.Facebook = true;
            _returningUser.Twitter = true;

            _facebookReturner.Add("Week" + (currentWeekNumber - 2), metricData);
            _facebookReturner.Add("Week" + +(currentWeekNumber - 1), metricData);
            _twitterReturner.Add("Week" + (currentWeekNumber - 2), metricData);
            _twitterReturner.Add("Week" + (currentWeekNumber - 1), metricData);

            // act
            var resultTask =_campaignHandler.CreateCampaign(new User() { UserId = Guid.NewGuid(), Campaigns = 0 }, "TestName", 
                DateTime.Now.AddDays(-7).Date.ToShortDateString(), DateTime.Now.AddMonths(7).Date.ToShortDateString(), "{ \"Facebook\": { \"Exposure\": \"3500\", \"Influence\": \"2100\", \"Engagement\": \"1700\", \"Acquisition\": \"25\" }, \"Twitter\": { \"Exposure\": \"9000\", \"Influence\": \"4500\", \"Engagement\": \"2400\", \"Acquisition\": \"35\" }   }");

            resultTask.Wait();

            // assert
            Assert.IsNotNull(createdCampaign.UserId);
            Assert.AreEqual("TestName", createdCampaign.CampaignName);
            Assert.IsNotNull(createdCampaign.StartDate);
            Assert.IsNotNull(createdCampaign.EndDate);
            Assert.IsNotNull(createdCampaign.Targets);
            _mockCampaignRepo.Verify(x => x.Create(It.IsAny<Campaign>()), Times.Once());
            _mockCampaignRepo.Verify(x => x.Update(It.IsAny<Campaign>()), Times.Once());
            Assert.AreEqual(createdCampaign.UserId, updatedCampaign.UserId);
            Assert.AreEqual(createdCampaign.CampaignName, updatedCampaign.CampaignName);
            Assert.AreEqual(createdCampaign.StartDate, updatedCampaign.StartDate);
            Assert.AreEqual(createdCampaign.EndDate, updatedCampaign.EndDate);
            Assert.AreEqual(createdCampaign.Targets, updatedCampaign.Targets);
            Assert.IsNotNull(updatedCampaign.Progress);
        }

        // starting date = now
        [TestMethod]
        public void UpdateCampaigns_GivenNotInRangeOrEmptyExistingValuesFromToday_ShouldHaveEmptyProgress()
        {
            // arrange
            var progressData = new JObject();
            var facebookData = new JObject();
            var twitterData = new JObject();
            var metricData = new JObject();
            metricData.Add("Exposure", 25);
            metricData.Add("Influence", 24);
            metricData.Add("Engagement", 23);
            metricData.Add("Acquisition", 15);

            facebookData.Add("Week" + (currentWeekNumber - 1), metricData);
            twitterData.Add("Week" + (currentWeekNumber - 1), metricData);
            facebookData.Add("Week" + (currentWeekNumber - 2), metricData);
            twitterData.Add("Week" + (currentWeekNumber - 2), metricData);

            progressData.Add("Facebook", facebookData);
            progressData.Add("Twitter", twitterData);

            _returningCampaigns.Add(new Campaign() { StartDate = DateTime.Now.AddDays(-7).Date, EndDate = DateTime.Now.AddMonths(3).Date, Progress = progressData.ToString() });
            _returningCampaigns.Add(new Campaign() { StartDate = DateTime.Now.AddDays(4).Date, EndDate = DateTime.Now.AddMonths(3).Date, Progress = string.Empty });

            _returningUser.Facebook = true;
            _returningUser.Twitter = true;

            // act
            _campaignHandler.UpdateCampaigns(Guid.NewGuid(), new Campaign() { StartDate = DateTime.Now.Date, EndDate = DateTime.Now.AddMonths(3).Date }).Wait();

            // assert
            Assert.IsNotNull(updatedCampaign);
            Assert.IsTrue(!updatedCampaign.Progress.Contains("Facebook"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("Twitter"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("Exposure"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("Influence"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("Engagement"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("Acquisition"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("23"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("24"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("25"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("15"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 1}"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 2}"));
            _mockTwitterHandler.Verify(t => t.GetCampaignInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Never);
            _mockFacebookHandler.Verify(f => f.GetCampaignInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Never);
        }

        [TestMethod]
        public void UpdateCampaigns_GivenNoExistingValuesInRangeFromToday_ShouldHaveEmptyProgress()
        {
            // arrange

            _returningUser.Facebook = true;
            _returningUser.Twitter = true;

            // act
            _campaignHandler.UpdateCampaigns(Guid.NewGuid(), new Campaign() { StartDate = DateTime.Now.Date, EndDate = DateTime.Now.AddMonths(3).Date }).Wait();

            // assert
            Assert.IsNotNull(updatedCampaign);
            Assert.IsTrue(!updatedCampaign.Progress.Contains("Facebook"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("Twitter"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("Exposure"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("Influence"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("Engagement"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("Acquisition"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("23"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("24"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("25"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("15"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 1}"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 2}"));
            _mockTwitterHandler.Verify(x => x.GetCampaignInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Never);
            _mockFacebookHandler.Verify(x => x.GetCampaignInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Never);
        }

        // starting date = previous date
        [TestMethod]
        public void UpdateCampaigns_GivenExistingValuesInRangeFromPreviousDate_ShouldAddValuesToProgress()
        {
            // arrange
            var progressData = new JObject();
            var facebookData = new JObject();
            var twitterData = new JObject();
            var metricData = new JObject();
            metricData.Add("Exposure", 25);
            metricData.Add("Influence", 24);
            metricData.Add("Engagement", 23);
            metricData.Add("Acquisition", 15);

            facebookData.Add("Week" + (currentWeekNumber - 2), metricData);
            twitterData.Add("Week" + (currentWeekNumber - 2), metricData);
            facebookData.Add("Week" + (currentWeekNumber - 1), metricData);
            twitterData.Add("Week" + (currentWeekNumber - 1), metricData);

            progressData.Add("Facebook", facebookData);
            progressData.Add("Twitter", twitterData);

            _returningCampaigns.Add(new Campaign() { StartDate = DateTime.Now.AddDays(-14).Date, EndDate = DateTime.Now.AddMonths(3).Date, Progress = progressData.ToString() });

            _returningUser.Facebook = true;
            _returningUser.Twitter = true;

            // act
            _campaignHandler.UpdateCampaigns(Guid.NewGuid(), new Campaign() { StartDate = DateTime.Now.AddDays(-14).Date, EndDate = DateTime.Now.AddMonths(3).Date }).Wait();

            // assert
            Assert.IsNotNull(updatedCampaign);
            Assert.IsTrue(updatedCampaign.Progress.Contains("Facebook"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Twitter"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Exposure"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Influence"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Engagement"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Acquisition"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("23"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("24"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("25"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("15"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 1}"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 2}"));
            _mockTwitterHandler.Verify(x => x.GetCampaignInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Never);
            _mockFacebookHandler.Verify(x => x.GetCampaignInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Never);
        }

        [TestMethod]
        public void UpdateCampaigns_GivenNoExistingValuesInRangeFromPreviousDate_ShouldHavePopulatedProgress()
        {
            // arrange
            var metricData = new JObject();
            metricData.Add("Exposure", 25);
            metricData.Add("Influence", 24);
            metricData.Add("Engagement", 23);
            metricData.Add("Acquisition", 15);

            _facebookReturner.Add("Week" + (currentWeekNumber - 2), metricData);
            _facebookReturner.Add("Week" + (currentWeekNumber - 1), metricData);
            _twitterReturner.Add("Week" + (currentWeekNumber - 2), metricData);
            _twitterReturner.Add("Week" + (currentWeekNumber - 1), metricData);

            _returningUser.Facebook = true;
            _returningUser.Twitter = true;

            // act
            _campaignHandler.UpdateCampaigns(Guid.NewGuid(), new Campaign() { StartDate = DateTime.Now.AddDays(-14).Date, EndDate = DateTime.Now.AddMonths(3).Date }).Wait();

            // assert
            Assert.IsNotNull(updatedCampaign);
            Assert.IsTrue(updatedCampaign.Progress.Contains("Facebook"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Twitter"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Exposure"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Influence"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Engagement"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Acquisition"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("23"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("24"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("25"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("15"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 1}"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 2}"));
            _mockTwitterHandler.Verify(x => x.GetCampaignInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Once);
            _mockFacebookHandler.Verify(x => x.GetCampaignInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Once);

            Assert.AreEqual(13, _passedFacebookHandlerDates.Count());
            Assert.AreEqual(13, _passedTwitterHandlerDates.Count());
        }

        // starting date = previous date, filtered existing campaigns
        [TestMethod]
        public void UpdateCampaigns_GivenExistingValuesPartInRangeFromPreviousDate_ShouldAddValuesToProgress()
        {
            // arrange
            var progressData = new JObject();
            var facebookData = new JObject();
            var twitterData = new JObject();
            var metricData = new JObject();
            metricData.Add("Exposure", 25);
            metricData.Add("Influence", 24);
            metricData.Add("Engagement", 23);
            metricData.Add("Acquisition", 15);

            facebookData.Add("Week" + (currentWeekNumber - 2), metricData);
            twitterData.Add("Week" + (currentWeekNumber - 2), metricData);
            facebookData.Add("Week" + (currentWeekNumber - 3), metricData);
            twitterData.Add("Week" + (currentWeekNumber - 3), metricData);

            progressData.Add("Facebook", facebookData);
            progressData.Add("Twitter", twitterData);

            _returningCampaigns.Add(new Campaign() { StartDate = DateTime.Now.AddDays(-21).Date, EndDate = DateTime.Now.AddDays(-14).Date, Progress = progressData.ToString() });

            _facebookReturner.Add("Week" + +(currentWeekNumber - 1), metricData);
            _twitterReturner.Add("Week" + (currentWeekNumber - 1), metricData);

            _returningUser.Facebook = true;
            _returningUser.Twitter = true;

            // act
            _campaignHandler.UpdateCampaigns(Guid.NewGuid(), new Campaign() { StartDate = DateTime.Now.AddDays(-15).Date, EndDate = DateTime.Now.AddMonths(3).Date }).Wait();

            // assert
            Assert.IsNotNull(updatedCampaign);
            Assert.IsTrue(updatedCampaign.Progress.Contains("Facebook"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Twitter"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Exposure"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Influence"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Engagement"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Acquisition"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("23"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("24"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("25"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("15"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 1}"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 2}"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 3}"));
            _mockTwitterHandler.Verify(t => t.GetCampaignInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Once);
            _mockFacebookHandler.Verify(f => f.GetCampaignInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Once);
            Assert.AreEqual(5, _passedFacebookHandlerDates.Count());
            Assert.AreEqual(5, _passedTwitterHandlerDates.Count());
        }

        // starting date = future date
        [TestMethod]
        public void UpdateCampaigns_GivenStartingDateFutureDate_ShouldHaveEmptyProgress()
        {
            // arrange
            _returningUser.Facebook = true;
            _returningUser.Twitter = true;

            // act
            _campaignHandler.UpdateCampaigns(Guid.NewGuid(), new Campaign() { StartDate = DateTime.Now.AddDays(7).Date, EndDate = DateTime.Now.AddMonths(3).Date }).Wait();
            
            // assert
            Assert.IsNotNull(updatedCampaign);
            Assert.IsTrue(!updatedCampaign.Progress.Contains("Facebook"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("Twitter"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("Exposure"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("Influence"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("Engagement"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("Acquisition"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("23"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("24"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("25"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("15"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 1}"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 2}"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 3}"));
            _mockTwitterHandler.Verify(x => x.GetCampaignInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Never);
            _mockFacebookHandler.Verify(x => x.GetCampaignInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Never);
        }

        // extra tests
        [TestMethod]
        public void UpdateCampaigns_GivenUserHasNoTwitter_ShouldNotCallTwitter()
        {
            // arrange
            var progressData = new JObject();
            var facebookData = new JObject();
            var metricData = new JObject();
            metricData.Add("Exposure", 25);
            metricData.Add("Influence", 24);
            metricData.Add("Engagement", 23);
            metricData.Add("Acquisition", 15);

            facebookData.Add("Week" + (currentWeekNumber - 4), metricData);
            facebookData.Add("Week" + (currentWeekNumber - 3), metricData);

            progressData.Add("Facebook", facebookData);

            _returningCampaigns.Add(new Campaign() { StartDate = DateTime.Now.AddDays(-28).Date, EndDate = DateTime.Now.AddMonths(3).Date, Progress = progressData.ToString() });

            _facebookReturner.Add("Week" + (currentWeekNumber - 2), metricData);
            _facebookReturner.Add("Week" + (currentWeekNumber - 1), metricData);

            _returningUser.Facebook = true;
            _returningUser.Twitter = false;

            // act
            _campaignHandler.UpdateCampaigns(Guid.NewGuid(), new Campaign() { StartDate = DateTime.Now.AddDays(-28).Date, EndDate = DateTime.Now.AddMonths(3).Date }).Wait();

            // assert
            Assert.IsNotNull(updatedCampaign);
            Assert.IsTrue(updatedCampaign.Progress.Contains("Facebook"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("Twitter"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Exposure"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Influence"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Engagement"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Acquisition"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("23"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("24"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("25"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("15"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 1}"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 2}"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 4}"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 3}"));
            _mockTwitterHandler.Verify(x => x.GetCampaignInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Never);
            _mockFacebookHandler.Verify(x => x.GetCampaignInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Once);
        }
        
        [TestMethod]
        public void DailyUpdate_GivenExistingProgressDoesntHaveWeekData_ShouldAddWeekData()
        {
            // arrange
            var progressData = new JObject();
            var facebookData = new JObject();
            var twitterData = new JObject();
            var metricData = new JObject();
            metricData.Add("Exposure", 25);
            metricData.Add("Influence", 24);
            metricData.Add("Engagement", 23);
            metricData.Add("Acquisition", 15);

            facebookData.Add("Week" + (currentWeekNumber - 2), metricData);
            twitterData.Add("Week" + (currentWeekNumber - 2), metricData);
            facebookData.Add("Week" + (currentWeekNumber - 3), metricData);
            twitterData.Add("Week" + (currentWeekNumber - 3), metricData);

            progressData.Add("Facebook", facebookData);
            progressData.Add("Twitter", twitterData);

            _facebookReturner.Add("Week" + (currentWeekNumber-1), metricData);
            _twitterReturner.Add("Week" + (currentWeekNumber-1), metricData);

            _returningUser.Facebook = true;
            _returningUser.Twitter = true;
            _returningUser.UserId = Guid.NewGuid();

            _returningCampaigns.Add(new Campaign() { UserId = _returningUser.UserId, StartDate = DateTime.Now.AddMonths(-1).Date, EndDate = DateTime.Now.Date.Date, Progress = progressData.ToString() });
            _returningCampaigns.Add(new Campaign() { UserId = _returningUser.UserId, StartDate = DateTime.Now.AddDays(-5).Date, EndDate = DateTime.Now.Date.AddDays(-8).Date, Progress = progressData.ToString() });

            // act
            _campaignHandler.DailyUpdate().Wait();

            // assert
            _mockCampaignRepo.Verify(x => x.Update(It.IsAny<Campaign>()), Times.Exactly(1));
            Assert.IsNotNull(updatedCampaign);
            Assert.IsTrue(updatedCampaign.Progress.Contains("Facebook"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Twitter"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Exposure"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Influence"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Engagement"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Acquisition"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("23"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("24"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("25"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("15"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("46"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("48"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("50"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 1}"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 2}"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 3}"));
        }

        [TestMethod]
        public void DailyUpdate_GivenExistingProgressDoesntHaveFacebookParent_ShouldAddWeekData()
        {
            // arrange
            var progressData = new JObject();
            var twitterData = new JObject();
            var metricData = new JObject();
            metricData.Add("Exposure", 25);
            metricData.Add("Influence", 24);
            metricData.Add("Engagement", 23);
            metricData.Add("Acquisition", 15);

            twitterData.Add("Week" + (currentWeekNumber - 2), metricData);
            twitterData.Add("Week" + (currentWeekNumber - 3), metricData);

            progressData.Add("Twitter", twitterData);

            _facebookReturner.Add("Week" + (currentWeekNumber-1), metricData);
            _twitterReturner.Add("Week" + (currentWeekNumber-1), metricData);

            _returningUser.Facebook = true;
            _returningUser.Twitter = true;
            _returningUser.UserId = Guid.NewGuid();

            _returningCampaigns.Add(new Campaign() { UserId = _returningUser.UserId, StartDate = DateTime.Now.AddMonths(-1).Date, EndDate = DateTime.Now.Date.Date, Progress = progressData.ToString() });

            // act
            _campaignHandler.DailyUpdate().Wait();
            _mockCampaignRepo.Verify(x => x.Update(It.IsAny<Campaign>()), Times.Exactly(1));
            Assert.IsNotNull(updatedCampaign);
            Assert.IsTrue(updatedCampaign.Progress.Contains("Facebook"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Twitter"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Exposure"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Influence"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Engagement"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Acquisition"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("23"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("24"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("25"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("15"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("46"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("48"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("50"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 1}"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 2}"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 3}"));
        }

        [TestMethod]
        public void DailyUpdate_GivenExistingProgressDoesHaveWeekData_ShouldAccumulateWeekData()
        {
            // arrange
            var progressData = new JObject();
            var facebookData = new JObject();
            var twitterData = new JObject();
            var metricData = new JObject();
            metricData.Add("Exposure", 25);
            metricData.Add("Influence", 24);
            metricData.Add("Engagement", 23);
            metricData.Add("Acquisition", 15);

            facebookData.Add("Week" + (currentWeekNumber - 2), metricData);
            twitterData.Add("Week" + (currentWeekNumber - 2), metricData);
            facebookData.Add("Week" + (currentWeekNumber-1), metricData);
            twitterData.Add("Week" + (currentWeekNumber-1), metricData);

            progressData.Add("Facebook", facebookData);
            progressData.Add("Twitter", twitterData);

            _facebookReturner.Add("Week" + (currentWeekNumber-1), metricData);
            _twitterReturner.Add("Week" + (currentWeekNumber-1), metricData);

            _returningUser.Facebook = true;
            _returningUser.Twitter = true;
            _returningUser.UserId = Guid.NewGuid();

            _returningCampaigns.Add(new Campaign() { UserId = _returningUser.UserId, StartDate = DateTime.Now.AddMonths(-1).Date, EndDate = DateTime.Now.Date, Progress = progressData.ToString() });

            // act
            _campaignHandler.DailyUpdate().Wait();
            _mockCampaignRepo.Verify(x => x.Update(It.IsAny<Campaign>()), Times.Exactly(1));
            Assert.IsNotNull(updatedCampaign);
            Assert.IsTrue(updatedCampaign.Progress.Contains("Facebook"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Twitter"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Exposure"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Influence"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Engagement"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Acquisition"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("23"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("24"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("25"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("15"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("46"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("48"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("50"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 1}"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 2}"));
        }

        [TestMethod]
        public void DailyUpdate_GivenExistingProgressDoesHaveWeekPartData_ShouldAccumulateWeekData()
        {
            // arrange
            var progressData = new JObject();
            var facebookData = new JObject();
            var twitterData = new JObject();
            var metricData = new JObject();
            var partMetricData = new JObject();

            metricData.Add("Exposure", 25);
            metricData.Add("Influence", 24);
            metricData.Add("Engagement", 23);
            metricData.Add("Acquisition", 15);

            partMetricData.Add("Influence", 24);
            partMetricData.Add("Engagement", 23);

            facebookData.Add("Week" + (currentWeekNumber - 2), metricData);
            twitterData.Add("Week" + (currentWeekNumber - 2), metricData);
            facebookData.Add("Week" + (currentWeekNumber -1), partMetricData);
            twitterData.Add("Week" + (currentWeekNumber -1), metricData);

            progressData.Add("Facebook", facebookData);
            progressData.Add("Twitter", twitterData);

            _facebookReturner.Add("Week" + (currentWeekNumber-1), metricData);
            _twitterReturner.Add("Week" + (currentWeekNumber-1), metricData);

            _returningUser.Facebook = true;
            _returningUser.Twitter = true;
            _returningUser.UserId = Guid.NewGuid();

            _returningCampaigns.Add(new Campaign() { UserId = _returningUser.UserId, StartDate = DateTime.Now.AddMonths(-1).Date, EndDate = DateTime.Now.Date, Progress = progressData.ToString() });

            // act
            _campaignHandler.DailyUpdate().Wait();

            // assert
            _mockCampaignRepo.Verify(x => x.Update(It.IsAny<Campaign>()), Times.Exactly(1));
            Assert.IsNotNull(updatedCampaign);
            Assert.IsTrue(updatedCampaign.Progress.Contains("Facebook"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Twitter"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Exposure"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Influence"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Engagement"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Acquisition"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("23"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("24"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("25"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("15"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("46"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("48"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("50"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 1}"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 2}"));
        }

        [TestMethod]
        public void GenerateCampaignDataTable_GivenCampaign_ReturnsTrueIfDataTableIsReturned()
        {
            // arrange

            // act
            var result = _campaignHandler.GenerateCampaignDataTable(new Campaign() { Progress = GetProgressData().ToString(), Targets = GetTargetData().ToString() });

            // assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AsEnumerable().Where(w => w[0].ToString() == "Overall" && w[2].ToString() == "Exposure").Sum(s => int.Parse(s[4].ToString())));
            Assert.IsNotNull(result.AsEnumerable().Where(w => w[0].ToString() == "Overall" && w[2].ToString() == "Influence").Sum(s => int.Parse(s[4].ToString())));
            Assert.IsNotNull(result.AsEnumerable().Where(w => w[0].ToString() == "Overall" && w[2].ToString() == "Engagement").Sum(s => int.Parse(s[4].ToString())));

            // check all inidividual values sum to overall value
            Assert.AreEqual(overallProgressExposure, result.AsEnumerable().Where(w => w[0].ToString() == "Overall" && w[2].ToString() == "Exposure").Sum(s => int.Parse(s[4].ToString())));
            Assert.AreEqual(overallProgressInfluence, result.AsEnumerable().Where(w => w[0].ToString() == "Overall" && w[2].ToString() == "Influence").Sum(s => int.Parse(s[4].ToString())));
            Assert.AreEqual(overallProgressEngagement, result.AsEnumerable().Where(w => w[0].ToString() == "Overall" && w[2].ToString() == "Engagement").Sum(s => int.Parse(s[4].ToString())));

            // check all overall fields have overall target
            Assert.IsTrue(result.AsEnumerable().Where(w => w[0].ToString() == "Overall" && w[2].ToString() == "Exposure").All(a => a[3].ToString() == overallTargetExposure.ToString()));
            Assert.IsTrue(result.AsEnumerable().Where(w => w[0].ToString() == "Overall" && w[2].ToString() == "Influence").All(a => a[3].ToString() == overallTargetInfluence.ToString()));
            Assert.IsTrue(result.AsEnumerable().Where(w => w[0].ToString() == "Overall" && w[2].ToString() == "Engagement").All(a => a[3].ToString() == overallTargetEngagement.ToString()));

            // last accumulated media values added together is same as overall
            Assert.AreEqual(int.Parse(result.AsEnumerable().Last(w => w[0].ToString() == "Facebook" && w[2].ToString() == "Exposure")[5].ToString()) + int.Parse(result.AsEnumerable().Last(w => w[0].ToString() == "Twitter" && w[2].ToString() == "Exposure")[5].ToString()), int.Parse(result.AsEnumerable().Last(w => w[0].ToString() == "Overall" && w[2].ToString() == "Exposure")[5].ToString()));
            Assert.AreEqual(int.Parse(result.AsEnumerable().Last(w => w[0].ToString() == "Facebook" && w[2].ToString() == "Influence")[5].ToString()) + int.Parse(result.AsEnumerable().Last(w => w[0].ToString() == "Twitter" && w[2].ToString() == "Influence")[5].ToString()), int.Parse(result.AsEnumerable().Last(w => w[0].ToString() == "Overall" && w[2].ToString() == "Influence")[5].ToString()));
            Assert.AreEqual(int.Parse(result.AsEnumerable().Last(w => w[0].ToString() == "Facebook" && w[2].ToString() == "Engagement")[5].ToString()) + int.Parse(result.AsEnumerable().Last(w => w[0].ToString() == "Twitter" && w[2].ToString() == "Engagement")[5].ToString()), int.Parse(result.AsEnumerable().Last(w => w[0].ToString() == "Overall" && w[2].ToString() == "Engagement")[5].ToString()));

            // media values added together is same as overall
            Assert.AreEqual(result.AsEnumerable().Where(w => w[0].ToString() == "Facebook" && w[2].ToString() == "Exposure").Sum(s => int.Parse(s[4].ToString())) + result.AsEnumerable().Where(w => w[0].ToString() == "Twitter" && w[2].ToString() == "Exposure").Sum(s => int.Parse(s[4].ToString())), int.Parse(result.AsEnumerable().Last(w => w[0].ToString() == "Overall" && w[2].ToString() == "Exposure")[5].ToString()));
            Assert.AreEqual(result.AsEnumerable().Where(w => w[0].ToString() == "Facebook" && w[2].ToString() == "Influence").Sum(s => int.Parse(s[4].ToString())) + result.AsEnumerable().Where(w => w[0].ToString() == "Twitter" && w[2].ToString() == "Influence").Sum(s => int.Parse(s[4].ToString())), int.Parse(result.AsEnumerable().Last(w => w[0].ToString() == "Overall" && w[2].ToString() == "Influence")[5].ToString()));
            Assert.AreEqual(result.AsEnumerable().Where(w => w[0].ToString() == "Facebook" && w[2].ToString() == "Engagement").Sum(s => int.Parse(s[4].ToString())) + result.AsEnumerable().Where(w => w[0].ToString() == "Twitter" && w[2].ToString() == "Engagement").Sum(s => int.Parse(s[4].ToString())), int.Parse(result.AsEnumerable().Last(w => w[0].ToString() == "Overall" && w[2].ToString() == "Engagement")[5].ToString()));
        }

        private JObject GetProgressData()
        {
            var progressData = new JObject();
            Random rdm = new Random();

            foreach (var item in _medias)
            {
                var weekData = new JObject();

                for (int i = 0; i < 10; i++)
                {
                    var metricData = new JObject();
                    int exposureNewValue = rdm.Next(1, 100);
                    overallProgressExposure += exposureNewValue;
                    metricData.Add("Exposure", exposureNewValue);

                    int influenceNewValue = rdm.Next(1, 100);
                    overallProgressInfluence += influenceNewValue;
                    metricData.Add("Influence", influenceNewValue);

                    int engagmentNewValue = rdm.Next(1, 100);
                    overallProgressEngagement += engagmentNewValue;
                    metricData.Add("Engagement", engagmentNewValue);

                    weekData.Add("Week" + i, metricData);
                }

                progressData.Add(item, weekData);
            }

            return progressData;
        }

        private JObject GetSkippedProgressData()
        {
            var progressData = new JObject();
            Random rdm = new Random();
            bool skip = true;

            foreach (var item in _medias)
            {
                var weekData = new JObject();

                for (int i = 0; i < 10; i++)
                {
                    if (skip && i == 9)
                    {
                        var metricData = new JObject();
                        int exposureNewValue = rdm.Next(1, 100);
                        overallProgressExposure += exposureNewValue;
                        metricData.Add("Exposure", exposureNewValue);

                        weekData.Add("Week" + i, metricData);

                        skip = false;
                    }
                    else
                    {
                        var metricData = new JObject();
                        int exposureNewValue = rdm.Next(1, 100);
                        overallProgressExposure += exposureNewValue;
                        metricData.Add("Exposure", exposureNewValue);

                        int influenceNewValue = rdm.Next(1, 100);
                        overallProgressInfluence += influenceNewValue;
                        metricData.Add("Influence", influenceNewValue);

                        int engagmentNewValue = rdm.Next(1, 100);
                        overallProgressEngagement += engagmentNewValue;
                        metricData.Add("Engagement", engagmentNewValue);

                        weekData.Add("Week" + i, metricData);
                    }
                }
                progressData.Add(item, weekData);
            }

            return progressData;
        }

        private JObject GetTargetData()
        {
            var targetData = new JObject();
            Random rdm = new Random();

            foreach (var item in _medias)
            {
                var metricData = new JObject();

                var exposureTarget = rdm.Next(400, 1000);
                overallTargetExposure += exposureTarget;
                metricData.Add("Exposure", exposureTarget);

                var influenceTarget = rdm.Next(400, 1000);
                overallTargetInfluence += influenceTarget;
                metricData.Add("Influence", influenceTarget);

                var engagementTarget = rdm.Next(400, 1000);
                overallTargetEngagement += engagementTarget;
                metricData.Add("Engagement", engagementTarget);

                targetData.Add(item, metricData);
            }

            return targetData;
        }
    }
}
