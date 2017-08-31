using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Data;
using FDM90.Repository;
using System.Collections.Generic;
using FDM90.Models;
using System.Linq;
using System.Data.SqlClient;

namespace FDM90UnitTests
{
    [TestClass]
    public class MarketingModelRepositoryUnitTests
    {
        private Mock<IDbConnection> _mockIDbConnection;
        private Mock<IDbCommand> _mockIDbCommand;
        private Mock<IDataParameterCollection> _mockIDataParameters;
        private Mock<IDataReader> _mockIDataReader;
        private IList<object> _parameterObjects = new List<object>();
        private string setSqlString = String.Empty;
        private int count = -1;
        private List<MarketingModel> _returningModels = new List<MarketingModel>()
        {
            new MarketingModel()
            {
                Name = "Name1",
                Description = "Description1",
                MetricsUsed = "One,Two",
                ResultMetric = "Three",
                CalculationExpression = "(value1)/(value2)"
            },
            new MarketingModel()
            {
                Name = "Name2",
                Description = "Description2",
                MetricsUsed = "Three,Two",
                ResultMetric = "One",
                CalculationExpression = "(value2)/(value3)"
            },
            new MarketingModel()
            {
                Name = "Name3",
                Description = "Description2",
                MetricsUsed = "One,Three",
                ResultMetric = "Two",
                CalculationExpression = "(value3)/(value1)"
            }
        };

        private MarketingModelRepository _marketingModelRepo;

        [TestInitialize]
        public void StartUp()
        {
            _mockIDataParameters = new Mock<IDataParameterCollection>();
            _mockIDataParameters.Setup(parameters => parameters.Add(It.IsAny<object>())).Callback((object value) =>
            {
                _parameterObjects.Add(value);
            });

            _mockIDataReader = new Mock<IDataReader>();

            _mockIDbCommand = new Mock<IDbCommand>();
            _mockIDbCommand.Setup(command => command.ExecuteNonQuery());
            _mockIDbCommand.Setup(command => command.ExecuteReader(It.IsAny<CommandBehavior>()))
                .Returns(_mockIDataReader.Object);
            _mockIDbCommand.Setup(command => command.Parameters).Returns(_mockIDataParameters.Object);
            _mockIDbCommand.SetupSet(command => command.CommandText = It.IsAny<string>())
                .Callback((string stringValue) => setSqlString = stringValue);

            _mockIDbConnection = new Mock<IDbConnection>();
            _mockIDbConnection.Setup(connection => connection.Open());
            _mockIDbConnection.Setup(connection => connection.CreateCommand()).Returns(_mockIDbCommand.Object);
            _mockIDbConnection.Setup(connection => connection.Dispose());

            _marketingModelRepo = new MarketingModelRepository(_mockIDbConnection.Object);
        }

        [TestCleanup]
        public void CleanUp()
        {
            setSqlString = null;
            _parameterObjects = null;
            _mockIDataReader = null;
            _mockIDataParameters = null;
            _mockIDbCommand = null;
            _mockIDbConnection = null;
            _marketingModelRepo = null;
            count = -1;
            _returningModels = null;
        }

        [TestMethod]
        public void ConstructorTest_CallToParameterlessConstructor_ReturnsTrueIfObjectIsNotNull()
        {
            //act
            _marketingModelRepo = new MarketingModelRepository();

            //arrange
            Assert.IsNotNull(_marketingModelRepo);
        }

        [TestMethod]
        public void ReadAllMarketingModels_GivenMethodCall_CorrectValuesSentToConnection()
        {
            //arrange
            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < _returningModels.Count - 1).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["Name"]).Returns(() => _returningModels[count].Name);
            _mockIDataReader.Setup(reader => reader["Description"]).Returns(() => _returningModels[count].Description);
            _mockIDataReader.Setup(reader => reader["MetricsUsed"]).Returns(() => _returningModels[count].MetricsUsed);
            _mockIDataReader.Setup(reader => reader["ResultMetric"]).Returns(() => _returningModels[count].ResultMetric);
            _mockIDataReader.Setup(reader => reader["CalculationExpression"]).Returns(() => _returningModels[count].CalculationExpression);

            //act
            var result = _marketingModelRepo.ReadAll();

            //assert
            Assert.AreEqual(0, _parameterObjects.Count);

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Select,
                    "[FDM90].[dbo].[MarketingModel]",
                    new string[0],
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }

        [TestMethod]
        public void ReadAllMarketingModels_GivenUserId_CorrectValueReturned()
        {
            //arrange
            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < _returningModels.Count - 1).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["Name"]).Returns(() => _returningModels[count].Name);
            _mockIDataReader.Setup(reader => reader["Description"]).Returns(() => _returningModels[count].Description);
            _mockIDataReader.Setup(reader => reader["MetricsUsed"]).Returns(() => _returningModels[count].MetricsUsed);
            _mockIDataReader.Setup(reader => reader["ResultMetric"]).Returns(() => _returningModels[count].ResultMetric);
            _mockIDataReader.Setup(reader => reader["CalculationExpression"]).Returns(() => _returningModels[count].CalculationExpression);

            //act
            var result = _marketingModelRepo.ReadAll().ToList();

            //assert
            Assert.AreEqual(_returningModels.Count, result.Count);

            for (int i = 0; i < _returningModels.Count; i++)
            {
                Assert.AreEqual(_returningModels[i].Name, result[i].Name);
                Assert.AreEqual(_returningModels[i].Description, result[i].Description);
                Assert.AreEqual(_returningModels[i].MetricsUsed, result[i].MetricsUsed);
                Assert.AreEqual(_returningModels[i].ResultMetric, result[i].ResultMetric);
                Assert.AreEqual(_returningModels[i].CalculationExpression, result[i].CalculationExpression);
            }
        }
    }
}
