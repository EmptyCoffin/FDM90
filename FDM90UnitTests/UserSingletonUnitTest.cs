using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FDM90.Singleton;
using FDM90.Models;

namespace FDM90UnitTests
{
    [TestClass]
    public class UserSingletonUnitTest
    {
        [TestMethod]
        public void ConstructorTests_GivenSingletonCalled_ReturnsTrueIfInstanceIsNotNull()
        {
            // arrange

            // act
            var result = UserSingleton.Instance;

            // assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void SingletonTest_GivenSingletonAssigned_ReturnsTrueIfValuesMatch()
        {
            // arrange
            Guid specificGuid = Guid.NewGuid();
            UserSingleton.Instance.CurrentUser = new User(specificGuid);

            // act
            var result = UserSingleton.Instance.CurrentUser;

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(specificGuid, result.UserId);
        }
    }
}
