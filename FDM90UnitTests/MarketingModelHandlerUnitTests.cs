using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FDM90.Repository;
using Moq;
using FDM90.Models;
using FDM90.Handlers;
using System.Collections.Generic;
using System.Linq;

namespace FDM90UnitTests
{
    [TestClass]
    public class MarketingModelHandlerUnitTests
    {
        private Mock<IReadAll<MarketingModel>> _mockReadAllMarketingModel;
        private MarketingModelHandler _marketingModelHandler;
        private List<MarketingModel> _marketingModels = new List<MarketingModel>()
        {
            new MarketingModel()
            {
                Name = "TestName1",
                Description = "TestDescription1",
                ResultMetric = "TestMetric1",
                MetricsUsed = "TestMetricsUsed1",
                CalculationExpression = "TestCalculationExpression1"
            },
            new MarketingModel()
            {
                Name = "TestName2",
                Description = "TestDescription2",
                ResultMetric = "TestMetric2",
                MetricsUsed = "TestMetricsUsed2",
                CalculationExpression = "TestCalculationExpression2"
            },
            new MarketingModel()
            {
                Name = "TestName3",
                Description = "TestDescription3",
                ResultMetric = "TestMetric3",
                MetricsUsed = "TestMetricsUsed3",
                CalculationExpression = "TestCalculationExpression3"
            }
        };

        [TestInitialize]
        public void StartUp()
        {
            _mockReadAllMarketingModel = new Mock<IReadAll<MarketingModel>>();
            _mockReadAllMarketingModel.Setup(x => x.ReadAll()).Returns(_marketingModels).Verifiable();

            _marketingModelHandler = new MarketingModelHandler(_mockReadAllMarketingModel.Object);
        }

        [TestCleanup]
        public void CleanUp()
        {
            _mockReadAllMarketingModel = null;
            _marketingModelHandler = null;
        }

        [TestMethod]
        public void ConstructorTest_CallToParameterlessConstructor_ReturnsTrueIfObjectIsNotNull()
        {
            //arrange

            //act
            _marketingModelHandler = new MarketingModelHandler();

            //assert
            Assert.IsNotNull(_marketingModelHandler);
        }

        [TestMethod]
        public void GetAllMarketingModels_GivenMethodCall_ReturnsTrueIfObjectsMatch()
        {
            //arrange

            //act
            var result = _marketingModelHandler.GetAllMarketingModels().ToList();

            //assert
            for(int i=0; i < result.Count; i++)
            {
                Assert.AreEqual(_marketingModels[i].Name, result[i].Name);
                Assert.AreEqual(_marketingModels[i].Description, result[i].Description);
                Assert.AreEqual(_marketingModels[i].ResultMetric, result[i].ResultMetric);
                Assert.AreEqual(_marketingModels[i].MetricsUsed, result[i].MetricsUsed);
                Assert.AreEqual(_marketingModels[i].CalculationExpression, result[i].CalculationExpression);
            }
        }

        [TestMethod]
        public void GetAllMarketingModels_GivenMethodCall_ReturnsTrueIfRepoMethodCalled()
        {
            //arrange

            //act
            var result = _marketingModelHandler.GetAllMarketingModels().ToList();

            //assert
            _mockReadAllMarketingModel.Verify(x => x.ReadAll(), Times.Once);
        }
    }
}
