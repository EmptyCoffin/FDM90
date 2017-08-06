using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FDM90.Handlers;
using FDM90.Models;
using Moq;
using FDM90.Repository;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace FDM90UnitTests
{
    [TestClass]
    public class CampaignHandlerUnitTests
    {
        private Mock<IRepository<Campaign>> _mockCampaignRepo;
        private Mock<IFacebookHandler> _mockFacebookHandler;
        private Mock<ITwitterHandler> _mockTwitterHandler;
        private Mock<IUserHandler> _mockUserHandler;

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
            _mockCampaignRepo.Setup(s => s.ReadAll()).Returns(_returningCampaigns).Verifiable();

            _mockFacebookHandler = new Mock<IFacebookHandler>();
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
            var progressData = new JObject();
            var facebookData = new JObject();
            var twitterData = new JObject();
            var metricData = new JObject();
            metricData.Add("Exposure", 25);
            metricData.Add("Influence", 24);
            metricData.Add("Engagement", 23);

            facebookData.Add("Week" + (currentWeekNumber - 1), metricData);
            twitterData.Add("Week" + (currentWeekNumber - 1), metricData);
            facebookData.Add("Week" + (currentWeekNumber - 2), metricData);
            twitterData.Add("Week" + +(currentWeekNumber - 2), metricData);

            progressData.Add("Facebook", facebookData);
            progressData.Add("Twitter", twitterData);

            _returningUser.Facebook = true;
            _returningUser.Twitter = true;

            _facebookReturner.Add("Week" + (currentWeekNumber - 2), metricData);
            _facebookReturner.Add("Week" + +(currentWeekNumber - 1), metricData);
            _twitterReturner.Add("Week" + (currentWeekNumber - 2), metricData);
            _twitterReturner.Add("Week" + (currentWeekNumber - 1), metricData);

            // act
            _campaignHandler.CreateCampaign(new User() { UserId = Guid.NewGuid(), Campaigns = 0 }, "TestName", 
                DateTime.Now.AddDays(-7).Date.ToShortDateString(), DateTime.Now.AddMonths(7).Date.ToShortDateString(), progressData.ToString());
            _campaignHandler.updateCampaignsTask.Wait();

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

            facebookData.Add("Week" + (currentWeekNumber - 1), metricData);
            twitterData.Add("Week" + (currentWeekNumber - 1), metricData);
            facebookData.Add("Week" + (currentWeekNumber - 2), metricData);
            twitterData.Add("Week" + +(currentWeekNumber - 2), metricData);

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
            Assert.IsTrue(updatedCampaign.Progress.Contains("Facebook"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Twitter"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("Exposure"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("Influence"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("Engagement"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("23"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("24"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("25"));
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
            Assert.IsTrue(!updatedCampaign.Progress.Contains("23"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("24"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("25"));
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

            facebookData.Add("Week" + (currentWeekNumber - 2), metricData);
            twitterData.Add("Week" + (currentWeekNumber - 2), metricData);
            facebookData.Add("Week" + (currentWeekNumber - 1), metricData);
            twitterData.Add("Week" + +(currentWeekNumber - 1), metricData);

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
            Assert.IsTrue(updatedCampaign.Progress.Contains("23"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("24"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("25"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 1}"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 2}"));
            _mockTwitterHandler.Verify(x => x.GetCampaignInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Never);
            _mockFacebookHandler.Verify(x => x.GetCampaignInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Never);
        }

        [TestMethod]
        public void UpdateCampaigns_GivenNoExistingValuesInRangeFromPreviousDate_ShouldHavePopulatedProgress()
        {
            // arrange
            var progressData = new JObject();
            var metricData = new JObject();
            metricData.Add("Exposure", 25);
            metricData.Add("Influence", 24);
            metricData.Add("Engagement", 23);

            _facebookReturner.Add("Week" + (currentWeekNumber - 2), metricData);
            _facebookReturner.Add("Week" + +(currentWeekNumber - 1), metricData);
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
            Assert.IsTrue(updatedCampaign.Progress.Contains("23"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("24"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("25"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 1}"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 2}"));
            _mockTwitterHandler.Verify(x => x.GetCampaignInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Once);
            _mockFacebookHandler.Verify(x => x.GetCampaignInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Once);

            Assert.AreEqual(7, _passedFacebookHandlerDates.Count());
            Assert.AreEqual(7, _passedTwitterHandlerDates.Count());
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

            facebookData.Add("Week" + (currentWeekNumber - 2), metricData);
            twitterData.Add("Week" + (currentWeekNumber - 2), metricData);
            facebookData.Add("Week" + (currentWeekNumber - 3), metricData);
            twitterData.Add("Week" + +(currentWeekNumber - 3), metricData);

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
            Assert.IsTrue(updatedCampaign.Progress.Contains("23"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("24"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("25"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 1}"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 2}"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 3}"));
            _mockTwitterHandler.Verify(t => t.GetCampaignInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Once);
            _mockFacebookHandler.Verify(f => f.GetCampaignInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Once);
            Assert.AreEqual(1, _passedFacebookHandlerDates.Count());
            Assert.AreEqual(1, _passedTwitterHandlerDates.Count());
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
            Assert.IsTrue(!updatedCampaign.Progress.Contains("23"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("24"));
            Assert.IsTrue(!updatedCampaign.Progress.Contains("25"));
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

            facebookData.Add("Week" + (currentWeekNumber - 4), metricData);
            facebookData.Add("Week" + (currentWeekNumber - 3), metricData);

            progressData.Add("Facebook", facebookData);

            _returningCampaigns.Add(new Campaign() { StartDate = DateTime.Now.AddDays(-28).Date, EndDate = DateTime.Now.AddMonths(3).Date, Progress = progressData.ToString() });

            _facebookReturner.Add("Week" + (currentWeekNumber - 2), metricData);
            _facebookReturner.Add("Week" + +(currentWeekNumber - 1), metricData);

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
            Assert.IsTrue(updatedCampaign.Progress.Contains("23"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("24"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("25"));
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
            _mockCampaignRepo.Verify(x => x.Update(It.IsAny<Campaign>()), Times.Exactly(1));
            Assert.IsNotNull(updatedCampaign);
            Assert.IsTrue(updatedCampaign.Progress.Contains("Facebook"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Twitter"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Exposure"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Influence"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("Engagement"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("23"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("24"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("25"));
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
            Assert.IsTrue(updatedCampaign.Progress.Contains("23"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("24"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("25"));
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
            Assert.IsTrue(updatedCampaign.Progress.Contains("23"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("24"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("25"));
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
            Assert.IsTrue(updatedCampaign.Progress.Contains("23"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("24"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("25"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("46"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("48"));
            Assert.IsTrue(updatedCampaign.Progress.Contains("50"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 1}"));
            Assert.IsTrue(updatedCampaign.Progress.Contains($"Week{currentWeekNumber - 2}"));
        }
    }
}
