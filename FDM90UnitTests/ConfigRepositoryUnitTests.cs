using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FDM90.Repository;
using System.Data;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using FDM90.Models;

namespace FDM90UnitTests
{
    [TestClass]
    public class ConfigRepositoryUnitTests
    {
        private Mock<IDbConnection> _mockIDbConnection;
        private Mock<IDbCommand> _mockIDbCommand;
        private Mock<IDataParameterCollection> _mockIDataParameters;
        private Mock<IDataReader> _mockIDataReader;
        private IList<object> _parameterObjects = new List<object>();
        private string setSqlString = String.Empty;
        private int count = -1;
        private List<ConfigItem> _returningConfig = new List<ConfigItem>()
        {
            new ConfigItem()
            {
                Name = "Name1",
                Value = "Page1"
            },
            new ConfigItem()
            {
                Name = "Name2",
                Value = "Page2"
            },
            new ConfigItem()
            {
                Name = "Name3",
                Value = "Page3"
            }
        };

        private ConfigRepository _configRepo;

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

            _configRepo = new ConfigRepository(_mockIDbConnection.Object);
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
            _configRepo = null;
            count = -1;
            _returningConfig = null;
        }

        [TestMethod]
        public void ConstructorTest_CallToParameterlessConstructor_ReturnsTrueIfObjectIsNotNull()
        {
            //act
            _configRepo = new ConfigRepository();

            //arrange
            Assert.IsNotNull(_configRepo);
        }

        [TestMethod]
        public void ReadAllCreds_GivenMethodCall_CorrectValuesSentToConnection()
        {
            //arrange
            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < _returningConfig.Count - 1).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["Name"]).Returns(() => _returningConfig[count].Name);
            _mockIDataReader.Setup(reader => reader["Value"]).Returns(() => _returningConfig[count].Value);

            //act
            var result = _configRepo.ReadAll();

            //assert
            Assert.AreEqual(0, _parameterObjects.Count);

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Select,
                    "[FDM90].[dbo].[Configuration]",
                    new string[0],
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }

        [TestMethod]
        public void ReadAllCreds_GivenUserId_CorrectValueReturned()
        {
            //arrange
            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < _returningConfig.Count - 1).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["Name"]).Returns(() => _returningConfig[count].Name);
            _mockIDataReader.Setup(reader => reader["Value"]).Returns(() => _returningConfig[count].Value);

            //act
            var result = _configRepo.ReadAll().ToList();

            //assert
            Assert.AreEqual(_returningConfig.Count, result.Count);

            for (int i = 0; i < _returningConfig.Count; i++)
            {
                Assert.AreEqual(_returningConfig[i].Name, result[i].Name);
                Assert.AreEqual(_returningConfig[i].Value, result[i].Value);
            }
        }
    }
}
