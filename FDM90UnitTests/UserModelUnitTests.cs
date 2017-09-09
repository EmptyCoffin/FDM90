using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FDM90.Models;

namespace FDM90UnitTests
{
    [TestClass]
    public class UserModelUnitTests
    {
        [TestMethod]
        public void GetIntegratedMediaChannels_GivenMediaAttribute_ReturnsTrueIfCorrectMediasReturned()
        {
            // arrange
            User userTest1 = new User("TestUserName1", "TestPassword1") { Facebook = true, Twitter = true };
            User userTest2 = new User("TestUserName2", "TestPassword2") { Facebook = false, Twitter = true };
            User userTest3 = new User("TestUserName3", "TestPassword3") { Facebook = true, Twitter = false };
            User userTest4 = new User("TestUserName4", "TestPassword4") { Facebook = false, Twitter = false };

            foreach (var item in new[] { userTest1, userTest2, userTest3, userTest4 })
            {
                // act
                var result = item.GetIntegratedMediaChannels();

                // assert
                Assert.IsTrue(item.Facebook ? result.Contains("Facebook") : !result.Contains("Facebook"));
                Assert.IsTrue(item.Twitter ? result.Contains("Twitter") : !result.Contains("Twitter"));
            }
        }
    }
}
