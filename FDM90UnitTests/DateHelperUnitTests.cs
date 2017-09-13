using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FDM90.Models.Helpers;
using System.Linq;

namespace FDM90UnitTests
{
    [TestClass]
    public class DateHelperUnitTests
    {
        int endDateDifference = -1;
        int startDateDifference = -1;
        DateTime startDate;
        DateTime endDate;

        [TestInitialize]
        public void StartUp()
        {
            endDateDifference = 3;
            startDateDifference = 15;
            startDate = DateTime.Now.AddDays(-startDateDifference);
            endDate = DateTime.Now.AddDays(-endDateDifference);
        }

        [TestCleanup]
        public void CleanUp()
        {
            endDateDifference = -1;
            startDateDifference = -1;
            startDate = new DateTime();
            endDate = new DateTime();
        }

        [TestMethod]
        public void GetDates_GivenDateRangeImplicitIncludeCurrentWeek_ReturnsTrueIfValuesIncludeCurrentWeek()
        {
            // arrange

            // act
            var result = DateHelper.GetDates(startDate, endDate);

            // assert
            Assert.AreEqual(startDateDifference - endDateDifference + 1, result.Count());

            for (int i = endDateDifference; i < startDateDifference; i++)
            {
                Assert.IsTrue(result.Select(s => s.Date).Contains(DateTime.Now.AddDays(-i).Date));
            }
        }

        [TestMethod]
        public void GetDates_GivenDateRangeExplicitIncludeCurrentWeek_ReturnsTrueIfValuesIncludeCurrentWeek()
        {
            // arrange

            // act
            var result = DateHelper.GetDates(startDate, endDate, true);

            // assert
            Assert.AreEqual(startDateDifference - endDateDifference + 1, result.Count());

            for (int i = endDateDifference; i < startDateDifference; i++)
            {
                Assert.IsTrue(result.Select(s => s.Date).Contains(DateTime.Now.AddDays(-i).Date));
            }
        }

        [TestMethod]
        public void GetDates_GivenDateRangeAndNotIncludeCurrentWeek_ReturnsTrueIfValuesDontCurrentWeek()
        {
            // arrange

            // act
            var result = DateHelper.GetDates(startDate, endDate, false);

            // assert
            Assert.AreEqual(startDateDifference - 7, result.Count());

            for (int i = endDateDifference; i < startDateDifference; i++)
            {
                if (i <= 7)
                {
                    Assert.IsFalse(result.Select(s => s.Date).Contains(DateTime.Now.AddDays(-i).Date));
                }
                else
                {
                    Assert.IsTrue(result.Select(s => s.Date).Contains(DateTime.Now.AddDays(-i).Date));
                }
            }
        }
    }
}
