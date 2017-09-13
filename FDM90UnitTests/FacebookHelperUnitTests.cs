using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FDM90.Models.Helpers;

namespace FDM90UnitTests
{
    [TestClass]
    public class FacebookHelperUnitTests
    {
        string testId = string.Empty;
        string[] fields = null;

        [TestInitialize]
        public void StartUp()
        {
            testId = "123456789";
            fields = new string[0];
        }

        [TestCleanup]
        public void CleanUp()
        {
            testId = null;
            fields = null;
        }

        [TestMethod]
        public void UrlBuilder_GivenIdParameter_ReturnsTrueIfUrlContainsIdParameterAndValue()
        {
            // arrange
            fields = new[] { FacebookHelper.AccessToken, FacebookHelper.Id, FacebookHelper.Name };

            // act
            var result = FacebookHelper.UrlBuilder(FacebookParameters.Id, testId, fields);

            // assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains(FacebookHelper.BaseUrl));
            Assert.IsTrue(result.Contains(FacebookHelper.IdParameter));
            Assert.IsFalse(result.Contains(testId));

            foreach (string field in fields)
            {
                Assert.IsTrue(result.Contains(field));
            }
        }

        [TestMethod]
        public void UrlBuilder_GivenFieldParameter_ReturnsTrueIfUrlContainsFieldParameterAndValue()
        {
            // arrange
            fields = new[] { FacebookHelper.FanCount, FacebookHelper.TalkingAboutCount, FacebookHelper.PageFansCity };

            // act
            var result = FacebookHelper.UrlBuilder(FacebookParameters.Field, testId, fields);

            // assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains(FacebookHelper.BaseUrl));
            Assert.IsTrue(result.Contains(FacebookHelper.FieldParameter));
            Assert.IsFalse(result.Contains(testId));

            foreach (string field in fields)
            {
                Assert.IsTrue(result.Contains(field));
            }
        }

        [TestMethod]
        public void UrlBuilder_GivenInsightParameterWithoutIdParameter_ReturnsTrueIfUrlContainsInsightAndIdParameterAndValue()
        {
            // arrange
            fields = new[] { FacebookHelper.Posts, FacebookHelper.PostReach, FacebookHelper.PostNegativity };

            // act
            var result = FacebookHelper.UrlBuilder(FacebookParameters.Insight, string.Empty, fields);

            // assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains(FacebookHelper.BaseUrl));
            Assert.IsTrue(result.Contains(FacebookHelper.InsightParameter));
            Assert.IsFalse(result.Contains(testId));

            foreach (string field in fields)
            {
                Assert.IsTrue(result.Contains(field));
            }
        }

        [TestMethod]
        public void UrlBuilder_GivenInsightParameterWithIdParameter_ReturnsTrueIfUrlContainsInsightParameterAndValue()
        {
            // arrange
            fields = new[] { FacebookHelper.Posts, FacebookHelper.PostReach, FacebookHelper.PostNegativity };

            // act
            var result = FacebookHelper.UrlBuilder(FacebookParameters.Insight, testId, fields);

            // assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains(FacebookHelper.BaseUrl));
            Assert.IsTrue(result.Contains(FacebookHelper.InsightParameter));
            Assert.IsTrue(result.Contains(FacebookHelper.InsightIdParameter));
            Assert.IsTrue(result.Contains(testId));

            foreach (string field in fields)
            {
                Assert.IsTrue(result.Contains(field));
            }
        }

        [TestMethod]
        public void UrlBuilder_GivenAccountParameter_ReturnsTrueIfUrlContainsAccountParameterAndValue()
        {
            // arrange
            fields = new[] { FacebookHelper.Posts, FacebookHelper.PostReach, FacebookHelper.PostNegativity };

            // act
            var result = FacebookHelper.UrlBuilder(FacebookParameters.Account, testId, fields);

            // assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains(FacebookHelper.BaseUrl));
            Assert.IsTrue(result.Contains(FacebookHelper.AccountParameter));
            Assert.IsTrue(result.Contains(testId));

            for (int i = 0; i < fields.Length; i++)
            {
                if (i > 0)
                {
                    Assert.IsFalse(result.Contains(fields[i]));
                }
                else
                {
                    Assert.IsTrue(result.Contains(fields[i]));
                }
            }
        }
    }
}
