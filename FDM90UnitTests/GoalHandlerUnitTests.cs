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
    public class GoalHandlerUnitTests
    {
        private Mock<IRepository<Goals>> _mockGoalRepo;
        private Mock<IFacebookHandler> _mockFacebookHandler;
        private Mock<ITwitterHandler> _mockTwitterHandler;
        private Mock<IUserHandler> _mockUserHandler;

        private GoalHandler _goalHandler;
        private static Goals updatedGoal;
        private static Goals createdGoal;
        private List<Goals> _returningGoals;
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
            _returningGoals = new List<Goals>();
            _returningUser = new User();
            _facebookReturner = new JObject();
            _twitterReturner = new JObject();
            _mockGoalRepo = new Mock<IRepository<Goals>>();
            _mockGoalRepo.Setup(x => x.Create(It.IsAny<Goals>())).Callback<Goals>(goal => createdGoal = goal).Verifiable();
            _mockGoalRepo.Setup(x => x.Update(It.IsAny<Goals>())).Callback<Goals>(goal => updatedGoal = goal).Verifiable();
            _mockGoalRepo.As<IReadMultipleSpecific<Goals>>().Setup(s => s.ReadMultipleSpecific(It.IsAny<string>())).Returns(_returningGoals).Verifiable();
            _mockGoalRepo.Setup(s => s.ReadAll()).Returns(_returningGoals).Verifiable();

            _mockFacebookHandler = new Mock<IFacebookHandler>();
            _mockFacebookHandler.Setup(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()))
                .Callback<Guid, DateTime[]>((passedGuid, passedDates) => _passedFacebookHandlerDates = passedDates)
                .Returns(_facebookReturner.Values()).Verifiable();
            _mockFacebookHandler.Setup(x => x.MediaName).Returns("Facebook");
            _mockTwitterHandler = new Mock<ITwitterHandler>();
            _mockTwitterHandler.Setup(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()))
                .Callback<Guid, DateTime[]>((passedGuid, passedDates) => _passedTwitterHandlerDates = passedDates)
                .Returns(_twitterReturner.Values()).Verifiable();
            _mockTwitterHandler.Setup(x => x.MediaName).Returns("Twitter");
            _mockUserHandler = new Mock<IUserHandler>();
            _mockUserHandler.Setup(x => x.GetUser(It.IsAny<string>())).Returns(_returningUser);
            currentWeekNumber = calendar.GetWeekOfYear(DateTime.Now, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);

            _goalHandler = new GoalHandler(_mockGoalRepo.Object, _mockFacebookHandler.Object, _mockTwitterHandler.Object, _mockUserHandler.Object);
        }

        [TestMethod]
        public void GoalHandler_ConstructorTest()
        {
            //act
            _goalHandler = new GoalHandler();

            //assert
            Assert.IsNotNull(_goalHandler);
        }

        [TestMethod]
        public void CreateGoal_GivenValues_ShouldCallCreateAndUpdate()
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
            _goalHandler.CreateGoal(Guid.NewGuid(), "TestName", 
                DateTime.Now.AddDays(-7).Date.ToShortDateString(), DateTime.Now.AddMonths(7).Date.ToShortDateString(), progressData.ToString());
            _goalHandler.updateGoalsTask.Wait();

            // assert
            Assert.IsNotNull(createdGoal.UserId);
            Assert.AreEqual("TestName", createdGoal.GoalName);
            Assert.IsNotNull(createdGoal.StartDate);
            Assert.IsNotNull(createdGoal.EndDate);
            Assert.IsNotNull(createdGoal.Targets);
            _mockGoalRepo.Verify(x => x.Create(It.IsAny<Goals>()), Times.Once());
            _mockGoalRepo.Verify(x => x.Update(It.IsAny<Goals>()), Times.Once());
            Assert.AreEqual(createdGoal.UserId, updatedGoal.UserId);
            Assert.AreEqual(createdGoal.GoalName, updatedGoal.GoalName);
            Assert.AreEqual(createdGoal.StartDate, updatedGoal.StartDate);
            Assert.AreEqual(createdGoal.EndDate, updatedGoal.EndDate);
            Assert.AreEqual(createdGoal.Targets, updatedGoal.Targets);
            Assert.IsNotNull(updatedGoal.Progress);
        }

        // starting date = now
        [TestMethod]
        public void UpdateGoals_GivenNotInRangeOrEmptyExistingValuesFromToday_ShouldHaveEmptyProgress()
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

            _returningGoals.Add(new Goals() { StartDate = DateTime.Now.AddDays(-7).Date, EndDate = DateTime.Now.AddMonths(3).Date, Progress = progressData.ToString() });
            _returningGoals.Add(new Goals() { StartDate = DateTime.Now.AddDays(4).Date, EndDate = DateTime.Now.AddMonths(3).Date, Progress = string.Empty });

            _returningUser.Facebook = true;
            _returningUser.Twitter = true;

            // act
            _goalHandler.UpdateGoals(Guid.NewGuid(), new Goals() { StartDate = DateTime.Now.Date, EndDate = DateTime.Now.AddMonths(3).Date }).Wait();

            // assert
            Assert.IsNotNull(updatedGoal);
            Assert.IsTrue(updatedGoal.Progress.Contains("Facebook"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Twitter"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("Exposure"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("Influence"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("Engagement"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("23"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("24"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("25"));
            Assert.IsTrue(!updatedGoal.Progress.Contains($"Week{currentWeekNumber - 1}"));
            Assert.IsTrue(!updatedGoal.Progress.Contains($"Week{currentWeekNumber - 2}"));
            _mockTwitterHandler.Verify(t => t.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Never);
            _mockFacebookHandler.Verify(f => f.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Never);
        }

        [TestMethod]
        public void UpdateGoals_GivenNoExistingValuesInRangeFromToday_ShouldHaveEmptyProgress()
        {
            // arrange

            _returningUser.Facebook = true;
            _returningUser.Twitter = true;

            // act
            _goalHandler.UpdateGoals(Guid.NewGuid(), new Goals() { StartDate = DateTime.Now.Date, EndDate = DateTime.Now.AddMonths(3).Date }).Wait();

            // assert
            Assert.IsNotNull(updatedGoal);
            Assert.IsTrue(!updatedGoal.Progress.Contains("Facebook"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("Twitter"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("Exposure"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("Influence"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("Engagement"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("23"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("24"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("25"));
            Assert.IsTrue(!updatedGoal.Progress.Contains($"Week{currentWeekNumber - 1}"));
            Assert.IsTrue(!updatedGoal.Progress.Contains($"Week{currentWeekNumber - 2}"));
            _mockTwitterHandler.Verify(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Never);
            _mockFacebookHandler.Verify(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Never);
        }

        // starting date = previous date
        [TestMethod]
        public void UpdateGoals_GivenExistingValuesInRangeFromPreviousDate_ShouldAddValuesToProgress()
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

            _returningGoals.Add(new Goals() { StartDate = DateTime.Now.AddDays(-14).Date, EndDate = DateTime.Now.AddMonths(3).Date, Progress = progressData.ToString() });

            _returningUser.Facebook = true;
            _returningUser.Twitter = true;

            // act
            _goalHandler.UpdateGoals(Guid.NewGuid(), new Goals() { StartDate = DateTime.Now.AddDays(-14).Date, EndDate = DateTime.Now.AddMonths(3).Date }).Wait();

            // assert
            Assert.IsNotNull(updatedGoal);
            Assert.IsTrue(updatedGoal.Progress.Contains("Facebook"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Twitter"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Exposure"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Influence"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Engagement"));
            Assert.IsTrue(updatedGoal.Progress.Contains("23"));
            Assert.IsTrue(updatedGoal.Progress.Contains("24"));
            Assert.IsTrue(updatedGoal.Progress.Contains("25"));
            Assert.IsTrue(updatedGoal.Progress.Contains($"Week{currentWeekNumber - 1}"));
            Assert.IsTrue(updatedGoal.Progress.Contains($"Week{currentWeekNumber - 2}"));
            _mockTwitterHandler.Verify(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Never);
            _mockFacebookHandler.Verify(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Never);
        }

        [TestMethod]
        public void UpdateGoals_GivenNoExistingValuesInRangeFromPreviousDate_ShouldHavePopulatedProgress()
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
            _goalHandler.UpdateGoals(Guid.NewGuid(), new Goals() { StartDate = DateTime.Now.AddDays(-14).Date, EndDate = DateTime.Now.AddMonths(3).Date }).Wait();

            // assert
            Assert.IsNotNull(updatedGoal);
            Assert.IsTrue(updatedGoal.Progress.Contains("Facebook"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Twitter"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Exposure"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Influence"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Engagement"));
            Assert.IsTrue(updatedGoal.Progress.Contains("23"));
            Assert.IsTrue(updatedGoal.Progress.Contains("24"));
            Assert.IsTrue(updatedGoal.Progress.Contains("25"));
            Assert.IsTrue(updatedGoal.Progress.Contains($"Week{currentWeekNumber - 1}"));
            Assert.IsTrue(updatedGoal.Progress.Contains($"Week{currentWeekNumber - 2}"));
            _mockTwitterHandler.Verify(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Once);
            _mockFacebookHandler.Verify(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Once);

            Assert.AreEqual(7, _passedFacebookHandlerDates.Count());
            Assert.AreEqual(7, _passedTwitterHandlerDates.Count());
        }

        // starting date = previous date, filtered existing goals
        [TestMethod]
        public void UpdateGoals_GivenExistingValuesPartInRangeFromPreviousDate_ShouldAddValuesToProgress()
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

            _returningGoals.Add(new Goals() { StartDate = DateTime.Now.AddDays(-21).Date, EndDate = DateTime.Now.AddDays(-14).Date, Progress = progressData.ToString() });

            _facebookReturner.Add("Week" + +(currentWeekNumber - 1), metricData);
            _twitterReturner.Add("Week" + (currentWeekNumber - 1), metricData);

            _returningUser.Facebook = true;
            _returningUser.Twitter = true;

            // act
            _goalHandler.UpdateGoals(Guid.NewGuid(), new Goals() { StartDate = DateTime.Now.AddDays(-15).Date, EndDate = DateTime.Now.AddMonths(3).Date }).Wait();

            // assert
            Assert.IsNotNull(updatedGoal);
            Assert.IsTrue(updatedGoal.Progress.Contains("Facebook"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Twitter"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Exposure"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Influence"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Engagement"));
            Assert.IsTrue(updatedGoal.Progress.Contains("23"));
            Assert.IsTrue(updatedGoal.Progress.Contains("24"));
            Assert.IsTrue(updatedGoal.Progress.Contains("25"));
            Assert.IsTrue(updatedGoal.Progress.Contains($"Week{currentWeekNumber - 1}"));
            Assert.IsTrue(updatedGoal.Progress.Contains($"Week{currentWeekNumber - 2}"));
            Assert.IsTrue(!updatedGoal.Progress.Contains($"Week{currentWeekNumber - 3}"));
            _mockTwitterHandler.Verify(t => t.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Once);
            _mockFacebookHandler.Verify(f => f.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Once);
            Assert.AreEqual(1, _passedFacebookHandlerDates.Count());
            Assert.AreEqual(1, _passedTwitterHandlerDates.Count());
        }

        // starting date = future date
        [TestMethod]
        public void UpdateGoals_GivenStartingDateFutureDate_ShouldHaveEmptyProgress()
        {
            // arrange
            _returningUser.Facebook = true;
            _returningUser.Twitter = true;

            // act
            _goalHandler.UpdateGoals(Guid.NewGuid(), new Goals() { StartDate = DateTime.Now.AddDays(7).Date, EndDate = DateTime.Now.AddMonths(3).Date }).Wait();
            
            // assert
            Assert.IsNotNull(updatedGoal);
            Assert.IsTrue(!updatedGoal.Progress.Contains("Facebook"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("Twitter"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("Exposure"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("Influence"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("Engagement"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("23"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("24"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("25"));
            Assert.IsTrue(!updatedGoal.Progress.Contains($"Week{currentWeekNumber - 1}"));
            Assert.IsTrue(!updatedGoal.Progress.Contains($"Week{currentWeekNumber - 2}"));
            Assert.IsTrue(!updatedGoal.Progress.Contains($"Week{currentWeekNumber - 3}"));
            _mockTwitterHandler.Verify(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Never);
            _mockFacebookHandler.Verify(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Never);
        }

        // extra tests
        [TestMethod]
        public void UpdateGoals_GivenUserHasNoTwitter_ShouldNotCallTwitter()
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

            _returningGoals.Add(new Goals() { StartDate = DateTime.Now.AddDays(-28).Date, EndDate = DateTime.Now.AddMonths(3).Date, Progress = progressData.ToString() });

            _facebookReturner.Add("Week" + (currentWeekNumber - 2), metricData);
            _facebookReturner.Add("Week" + +(currentWeekNumber - 1), metricData);

            _returningUser.Facebook = true;
            _returningUser.Twitter = false;

            // act
            _goalHandler.UpdateGoals(Guid.NewGuid(), new Goals() { StartDate = DateTime.Now.AddDays(-28).Date, EndDate = DateTime.Now.AddMonths(3).Date }).Wait();

            // assert
            Assert.IsNotNull(updatedGoal);
            Assert.IsTrue(updatedGoal.Progress.Contains("Facebook"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("Twitter"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Exposure"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Influence"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Engagement"));
            Assert.IsTrue(updatedGoal.Progress.Contains("23"));
            Assert.IsTrue(updatedGoal.Progress.Contains("24"));
            Assert.IsTrue(updatedGoal.Progress.Contains("25"));
            Assert.IsTrue(updatedGoal.Progress.Contains($"Week{currentWeekNumber - 1}"));
            Assert.IsTrue(updatedGoal.Progress.Contains($"Week{currentWeekNumber - 2}"));
            Assert.IsTrue(updatedGoal.Progress.Contains($"Week{currentWeekNumber - 4}"));
            Assert.IsTrue(updatedGoal.Progress.Contains($"Week{currentWeekNumber - 3}"));
            _mockTwitterHandler.Verify(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Never);
            _mockFacebookHandler.Verify(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime[]>()), Times.Once);
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

            _returningGoals.Add(new Goals() { UserId = _returningUser.UserId, StartDate = DateTime.Now.AddMonths(-1).Date, EndDate = DateTime.Now.Date.Date, Progress = progressData.ToString() });
            _returningGoals.Add(new Goals() { UserId = _returningUser.UserId, StartDate = DateTime.Now.AddDays(-5).Date, EndDate = DateTime.Now.Date.AddDays(-8).Date, Progress = progressData.ToString() });

            // act
            _goalHandler.DailyUpdate().Wait();
            _mockGoalRepo.Verify(x => x.Update(It.IsAny<Goals>()), Times.Exactly(1));
            Assert.IsNotNull(updatedGoal);
            Assert.IsTrue(updatedGoal.Progress.Contains("Facebook"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Twitter"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Exposure"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Influence"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Engagement"));
            Assert.IsTrue(updatedGoal.Progress.Contains("23"));
            Assert.IsTrue(updatedGoal.Progress.Contains("24"));
            Assert.IsTrue(updatedGoal.Progress.Contains("25"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("46"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("48"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("50"));
            Assert.IsTrue(updatedGoal.Progress.Contains($"Week{currentWeekNumber - 1}"));
            Assert.IsTrue(updatedGoal.Progress.Contains($"Week{currentWeekNumber - 2}"));
            Assert.IsTrue(updatedGoal.Progress.Contains($"Week{currentWeekNumber - 3}"));
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

            _returningGoals.Add(new Goals() { UserId = _returningUser.UserId, StartDate = DateTime.Now.AddMonths(-1).Date, EndDate = DateTime.Now.Date.Date, Progress = progressData.ToString() });

            // act
            _goalHandler.DailyUpdate().Wait();
            _mockGoalRepo.Verify(x => x.Update(It.IsAny<Goals>()), Times.Exactly(1));
            Assert.IsNotNull(updatedGoal);
            Assert.IsTrue(updatedGoal.Progress.Contains("Facebook"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Twitter"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Exposure"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Influence"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Engagement"));
            Assert.IsTrue(updatedGoal.Progress.Contains("23"));
            Assert.IsTrue(updatedGoal.Progress.Contains("24"));
            Assert.IsTrue(updatedGoal.Progress.Contains("25"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("46"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("48"));
            Assert.IsTrue(!updatedGoal.Progress.Contains("50"));
            Assert.IsTrue(updatedGoal.Progress.Contains($"Week{currentWeekNumber - 1}"));
            Assert.IsTrue(updatedGoal.Progress.Contains($"Week{currentWeekNumber - 2}"));
            Assert.IsTrue(updatedGoal.Progress.Contains($"Week{currentWeekNumber - 3}"));
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

            _returningGoals.Add(new Goals() { UserId = _returningUser.UserId, StartDate = DateTime.Now.AddMonths(-1).Date, EndDate = DateTime.Now.Date, Progress = progressData.ToString() });

            // act
            _goalHandler.DailyUpdate().Wait();
            _mockGoalRepo.Verify(x => x.Update(It.IsAny<Goals>()), Times.Exactly(1));
            Assert.IsNotNull(updatedGoal);
            Assert.IsTrue(updatedGoal.Progress.Contains("Facebook"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Twitter"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Exposure"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Influence"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Engagement"));
            Assert.IsTrue(updatedGoal.Progress.Contains("23"));
            Assert.IsTrue(updatedGoal.Progress.Contains("24"));
            Assert.IsTrue(updatedGoal.Progress.Contains("25"));
            Assert.IsTrue(updatedGoal.Progress.Contains("46"));
            Assert.IsTrue(updatedGoal.Progress.Contains("48"));
            Assert.IsTrue(updatedGoal.Progress.Contains("50"));
            Assert.IsTrue(updatedGoal.Progress.Contains($"Week{currentWeekNumber - 1}"));
            Assert.IsTrue(updatedGoal.Progress.Contains($"Week{currentWeekNumber - 2}"));
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

            _returningGoals.Add(new Goals() { UserId = _returningUser.UserId, StartDate = DateTime.Now.AddMonths(-1).Date, EndDate = DateTime.Now.Date, Progress = progressData.ToString() });

            // act
            _goalHandler.DailyUpdate().Wait();
            _mockGoalRepo.Verify(x => x.Update(It.IsAny<Goals>()), Times.Exactly(1));
            Assert.IsNotNull(updatedGoal);
            Assert.IsTrue(updatedGoal.Progress.Contains("Facebook"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Twitter"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Exposure"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Influence"));
            Assert.IsTrue(updatedGoal.Progress.Contains("Engagement"));
            Assert.IsTrue(updatedGoal.Progress.Contains("23"));
            Assert.IsTrue(updatedGoal.Progress.Contains("24"));
            Assert.IsTrue(updatedGoal.Progress.Contains("25"));
            Assert.IsTrue(updatedGoal.Progress.Contains("46"));
            Assert.IsTrue(updatedGoal.Progress.Contains("48"));
            Assert.IsTrue(updatedGoal.Progress.Contains("50"));
            Assert.IsTrue(updatedGoal.Progress.Contains($"Week{currentWeekNumber - 1}"));
            Assert.IsTrue(updatedGoal.Progress.Contains($"Week{currentWeekNumber - 2}"));
        }
    }
}
