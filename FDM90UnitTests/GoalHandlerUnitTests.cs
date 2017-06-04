using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FDM90.Handlers;
using FDM90.Models;
using Moq;
using FDM90.Repository;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Globalization;

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
        private Goals passedGoal;
        private List<Goals> _returningGoals;
        private User _returningUser;
        private JObject _facebookReturner;
        private JObject _twitterReturner;
        private DateTime _passedFacebookHandlerStartDate;
        private DateTime _passedTwitterHandlerStartDate;

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
            _mockGoalRepo.Setup(x => x.Update(It.IsAny<Goals>())).Callback<Goals>(goal => passedGoal = goal).Verifiable();
            _mockGoalRepo.As<IReadMultipleSpecific<Goals>>().Setup(s => s.ReadMultipleSpecific(It.IsAny<string>())).Returns(_returningGoals).Verifiable();

            _mockFacebookHandler = new Mock<IFacebookHandler>();
            _mockFacebookHandler.Setup(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback<Guid, DateTime, DateTime>((passedGuid, passedStartDate, passedEndDate) => _passedFacebookHandlerStartDate = passedStartDate)
                .Returns(_facebookReturner.Values()).Verifiable();
            _mockFacebookHandler.Setup(x => x.MediaName).Returns("Facebook");
            _mockTwitterHandler = new Mock<ITwitterHandler>();
            _mockTwitterHandler.Setup(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback<Guid, DateTime, DateTime>((passedGuid, passedStartDate, passedEndDate) => _passedTwitterHandlerStartDate = passedStartDate)
                .Returns(_twitterReturner.Values()).Verifiable();
            _mockTwitterHandler.Setup(x => x.MediaName).Returns("Twitter");
            _mockUserHandler = new Mock<IUserHandler>();
            _mockUserHandler.Setup(x => x.GetUser(It.IsAny<string>())).Returns(_returningUser);
            currentWeekNumber = calendar.GetWeekOfYear(DateTime.Now, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);

            _goalHandler = new GoalHandler(_mockGoalRepo.Object, _mockFacebookHandler.Object, _mockTwitterHandler.Object, _mockUserHandler.Object);
        }

        // starting date = now
        [TestMethod]
        public void UpdateGoals_GivenExistingValuesFromToday_ShouldAddValuesToProgress()
        {
            // arrange
            var progressData = new JObject();
            var facebookData = new JObject();
            var twitterData = new JObject();
            var metricData = new JObject();
            metricData.Add("Exposure", 25);
            metricData.Add("Influence", 24);
            metricData.Add("Engagement", 23);

            facebookData.Add("Week" + (currentWeekNumber - 4), metricData);
            twitterData.Add("Week" + (currentWeekNumber - 4), metricData);
            facebookData.Add("Week" + (currentWeekNumber - 3), metricData);
            twitterData.Add("Week" + +(currentWeekNumber - 3), metricData);

            progressData.Add("Facebook", facebookData);
            progressData.Add("Twitter", twitterData);

            _returningGoals.Add(new Goals() { StartDate = DateTime.Now.AddDays(-7), EndDate = DateTime.Now.AddMonths(3), Progress = progressData.ToString() });

            _facebookReturner.Add("Week" + (currentWeekNumber - 2), metricData);
            _facebookReturner.Add("Week" + +(currentWeekNumber - 1), metricData);
            _twitterReturner.Add("Week" + (currentWeekNumber - 2), metricData);
            _twitterReturner.Add("Week" + (currentWeekNumber - 1), metricData);

            _returningUser.Facebook = true;
            _returningUser.Twitter = true;

            // act
            _goalHandler.UpdateGoals(Guid.NewGuid(), new Goals() { StartDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(3) })
                            .ContinueWith(task =>
                            {
                                // assert
                                Assert.IsNotNull(passedGoal);
                                _mockTwitterHandler.Verify(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
                                _mockFacebookHandler.Verify(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
                            });
        }

        [TestMethod]
        public void UpdateGoals_GivenNoExistingValuesInRangeFromToday_ShouldHaveEmptyProgress()
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
            _goalHandler.UpdateGoals(Guid.NewGuid(), new Goals() { StartDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(3) })
                            .ContinueWith(task =>
                            {
                                // assert
                                Assert.IsNotNull(passedGoal);
                                _mockTwitterHandler.Verify(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
                                _mockFacebookHandler.Verify(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
                            });
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

            facebookData.Add("Week" + (currentWeekNumber - 4), metricData);
            twitterData.Add("Week" + (currentWeekNumber - 4), metricData);
            facebookData.Add("Week" + (currentWeekNumber - 3), metricData);
            twitterData.Add("Week" + +(currentWeekNumber - 3), metricData);

            progressData.Add("Facebook", facebookData);
            progressData.Add("Twitter", twitterData);

            _returningGoals.Add(new Goals() { StartDate = DateTime.Now.AddDays(-7), EndDate = DateTime.Now.AddMonths(3), Progress = progressData.ToString() });

            _facebookReturner.Add("Week" + (currentWeekNumber - 2), metricData);
            _facebookReturner.Add("Week" + +(currentWeekNumber - 1), metricData);
            _twitterReturner.Add("Week" + (currentWeekNumber - 2), metricData);
            _twitterReturner.Add("Week" + (currentWeekNumber - 1), metricData);

            _returningUser.Facebook = true;
            _returningUser.Twitter = true;

            // act
            _goalHandler.UpdateGoals(Guid.NewGuid(), new Goals() { StartDate = DateTime.Now.AddDays(-7), EndDate = DateTime.Now.AddMonths(3) })
                            .ContinueWith(task =>
                            {
                                // assert
                                Assert.IsNotNull(passedGoal);
                                _mockTwitterHandler.Verify(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
                                _mockFacebookHandler.Verify(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
                            });
        }

        [TestMethod]
        public void UpdateGoals_GivenNoExistingValuesInRangeFromPreviousDate_ShouldHaveEmptyProgress()
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
            _goalHandler.UpdateGoals(Guid.NewGuid(), new Goals() { StartDate = DateTime.Now.AddDays(-7), EndDate = DateTime.Now.AddMonths(3) })
                            .ContinueWith(task =>
                            {
                                // assert
                                Assert.IsNotNull(passedGoal);
                                _mockTwitterHandler.Verify(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
                                _mockFacebookHandler.Verify(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
                            });
        }

        // starting date = future date
        [TestMethod]
        public void UpdateGoals_GivenExistingValuesNotInRangeFromFutureDate_ShouldAddValuesToProgress()
        {
            // arrange
            var progressData = new JObject();
            var facebookData = new JObject();
            var twitterData = new JObject();
            var metricData = new JObject();
            metricData.Add("Exposure", 25);
            metricData.Add("Influence", 24);
            metricData.Add("Engagement", 23);

            facebookData.Add("Week" + (currentWeekNumber - 4), metricData);
            twitterData.Add("Week" + (currentWeekNumber - 4), metricData);
            facebookData.Add("Week" + (currentWeekNumber - 3), metricData);
            twitterData.Add("Week" + +(currentWeekNumber - 3), metricData);

            progressData.Add("Facebook", facebookData);
            progressData.Add("Twitter", twitterData);

            _returningGoals.Add(new Goals() { StartDate = DateTime.Now.AddDays(7), EndDate = DateTime.Now.AddMonths(3), Progress = progressData.ToString() });

            _facebookReturner.Add("Week" + (currentWeekNumber - 2), metricData);
            _facebookReturner.Add("Week" + +(currentWeekNumber - 1), metricData);
            _twitterReturner.Add("Week" + (currentWeekNumber - 2), metricData);
            _twitterReturner.Add("Week" + (currentWeekNumber - 1), metricData);

            _returningUser.Facebook = true;
            _returningUser.Twitter = true;

            // act
            _goalHandler.UpdateGoals(Guid.NewGuid(), new Goals() { StartDate = DateTime.Now.AddDays(-7), EndDate = DateTime.Now.AddMonths(3) })
                            .ContinueWith(task =>
                            {
                                // assert
                                Assert.IsNotNull(passedGoal);
                                _mockTwitterHandler.Verify(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
                                _mockFacebookHandler.Verify(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
                            });
        }

        [TestMethod]
        public void UpdateGoals_GivenNoExistingValuesInRangeFromFutureDate_ShouldHaveEmptyProgress()
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
            _goalHandler.UpdateGoals(Guid.NewGuid(), new Goals() { StartDate = DateTime.Now.AddDays(7), EndDate = DateTime.Now.AddMonths(3) })
                            .ContinueWith(task =>
                            {
                                // assert
                                Assert.IsNotNull(passedGoal);
                                _mockTwitterHandler.Verify(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
                                _mockFacebookHandler.Verify(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
                            });
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

            _returningGoals.Add(new Goals() { StartDate = DateTime.Now.AddDays(-28), EndDate = DateTime.Now.AddMonths(3), Progress = progressData.ToString() });

            _facebookReturner.Add("Week" + (currentWeekNumber - 2), metricData);
            _facebookReturner.Add("Week" + +(currentWeekNumber - 1), metricData);

            _returningUser.Facebook = true;
            _returningUser.Twitter = false;

            // act
            _goalHandler.UpdateGoals(Guid.NewGuid(), new Goals() { StartDate = DateTime.Now.AddDays(-28), EndDate = DateTime.Now.AddMonths(3) })
                            .ContinueWith(task =>
                            {
                                // assert
                                Assert.IsNotNull(passedGoal);
                                _mockTwitterHandler.Verify(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
                                _mockFacebookHandler.Verify(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
                            });
        }

        [TestMethod]
        public void UpdateGoals_GivenExistingValuesPartlyInRangeFromFutureDate_ShouldAddValuesToProgress()
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

            _returningGoals.Add(new Goals() { StartDate = DateTime.Now.AddMonths(-3), EndDate = DateTime.Now, Progress = progressData.ToString() });

            _facebookReturner.Add("Week" + +(currentWeekNumber - 1), metricData);
            _twitterReturner.Add("Week" + (currentWeekNumber - 1), metricData);

            _returningUser.Facebook = true;
            _returningUser.Twitter = true;

            // act
            _goalHandler.UpdateGoals(Guid.NewGuid(), new Goals() { StartDate = DateTime.Now.AddDays(-14), EndDate = DateTime.Now.AddMonths(3) })
                            .ContinueWith(task =>
                            {
                                // assert
                                Assert.IsNotNull(passedGoal);
                                Assert.AreNotEqual(DateTime.Now.AddDays(-7), _passedFacebookHandlerStartDate);
                                Assert.AreNotEqual(DateTime.Now.AddDays(-7), _passedTwitterHandlerStartDate);
                                _mockTwitterHandler.Verify(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
                                _mockFacebookHandler.Verify(x => x.GetGoalInfo(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
                            });
        }
    }
}
