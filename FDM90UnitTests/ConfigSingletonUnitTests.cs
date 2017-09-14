using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FDM90.Singleton;
using Moq;
using FDM90.Repository;
using FDM90.Models;
using System.Collections.Generic;
using System.Linq;

namespace FDM90UnitTests
{
    [TestClass]
    public class ConfigSingletonUnitTests
    {
        private Mock<IReadAll<ConfigItem>> _mockReadAllRepo;
        private List<ConfigItem> _returningList;

        [TestInitialize]
        public void StartUp()
        {
            _returningList = new List<ConfigItem>()
            {
                new ConfigItem()
                {
                    Name = "FacebookClientId",
                    Value = "FacebookClientIdValue"
                },
                new ConfigItem()
                {
                    Name = "FacebookClientSecret",
                    Value = "FacebookClientSecretValue"
                },
                new ConfigItem()
                {
                    Name = "TwitterConsumerKey",
                    Value = "TwitterConsumerKeyValue"
                },
                new ConfigItem()
                {
                    Name = "TwitterConsumerSecret",
                    Value = "TwitterConsumerSecretValue"
                },
                new ConfigItem()
                {
                    Name = "FileSaveLocation",
                    Value = "FileSaveLocationValue"
                }
            };

            _mockReadAllRepo = new Mock<IReadAll<ConfigItem>>();
            _mockReadAllRepo.Setup(x => x.ReadAll()).Returns(_returningList).Verifiable();          
        }

        [TestCleanup]
        public void CleanUp()
        {
            _returningList = null;
            _mockReadAllRepo = null;
        }

        [TestMethod]
        public void ConstructorTest_GivenParameterlessConstructorCall_ReturnsTrueIfObjectIsNotNull()
        {
            // arrange

           // act
           var result = new ConfigSingleton();

           // assert
           Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ConstructorTest_GivenParameterlessConstructorCall_ReturnsTrueIfInstanceIsNotNull()
        {
            // arrange

            // act
            var result = ConfigSingleton.Instance;

            // assert
            Assert.IsNotNull(result);
        }


        [TestMethod]
        public void GetValues_GivenMockRepo_ReturnsTrueIfValuesAreCorrectlyPopulated()
        {
            // arrange

            // act
            var result = new ConfigSingleton(_mockReadAllRepo.Object);

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(_returningList.First(x => x.Name == "FacebookClientId").Value, result.FacebookClientId);
            Assert.AreEqual(_returningList.First(x => x.Name == "FacebookClientSecret").Value, result.FacebookClientSecret);
            Assert.AreEqual(_returningList.First(x => x.Name == "TwitterConsumerKey").Value, result.TwitterConsumerKey);
            Assert.AreEqual(_returningList.First(x => x.Name == "TwitterConsumerSecret").Value, result.TwitterConsumerSecret);
            Assert.AreEqual(_returningList.First(x => x.Name == "FileSaveLocation").Value, result.FileSaveLocation);
        }
    }
}
