using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FDM90.Models;
using FDM90.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FDM90UnitTests
{
    [TestClass]
    public class FacebookRepositoryUnitTests
    {
        private Mock<IDbConnection> _mockIDbConnection;
        private Mock<IDbCommand> _mockIDbCommand;
        private Mock<IDataParameterCollection> _mockIDataParameters;
        private Mock<IDataReader> _mockIDataReader;
        private IList<object> _parameterObjects = new List<object>();
        private string setSqlString = String.Empty;
        private static Guid _specificGuid = Guid.NewGuid();
        private List<FacebookCredentials> _returningCreds = new List<FacebookCredentials>()
        {
            new FacebookCredentials()
            {
                UserId = Guid.NewGuid(),
                PageName = "Page1"
            },
            new FacebookCredentials()
            {
                UserId = _specificGuid,
                PageName = "Page2"
            },
            new FacebookCredentials()
            {
                UserId = Guid.NewGuid(),
                PageName = "Page3"
            },
            new FacebookCredentials()
            {
                UserId = Guid.NewGuid(),
                PageName = "Page4",
                PermanentAccessToken = "ThisIsAPermanentAccessToken",
                FacebookData = "This IS Facebook Data"
            },
        };

        private FacebookRepository _facebookRepo;
        private int count = -1;

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

            _facebookRepo = new FacebookRepository(_mockIDbConnection.Object);
        }

        [TestCleanup]
        public void CleanUp()
        {
            _mockIDbConnection = null;
            _mockIDbCommand = null;
            _mockIDataParameters = null;
            _mockIDataReader = null;
            setSqlString = string.Empty;
            _facebookRepo = null;
            count = -1;
        }

        [TestMethod]
        public void ConstructorTest_CallToParameterlessConstructor_ReturnsTrueIfObjectIsNotNull()
        {
            //arrange

            //act
            _facebookRepo = new FacebookRepository();

            //assert
            Assert.IsNotNull(_facebookRepo);
        }

        [TestMethod]
        public void CreateCreds_GivenValues_CorrectValuesSentToConnection()
        {
            //arrange
            FacebookCredentials testCreds = new FacebookCredentials()
            {
                UserId = new Guid(),
                PageName = "TestPageName"
            };

            //act
            _facebookRepo.Create(testCreds);

            //assert
            Assert.AreEqual(2, _parameterObjects.Count);
            foreach (var property in testCreds.GetType().GetProperties())
            {
                if (property.GetValue(testCreds) == null) continue;
                foreach (var parameter in _parameterObjects)
                {
                    var sqlParameter = (SqlParameter)parameter;

                    if (!sqlParameter.ParameterName.Contains(property.Name)) continue;
                    Assert.AreEqual("@" + property.Name, sqlParameter.ParameterName);
                    Assert.AreEqual(property.GetValue(testCreds), sqlParameter.Value);
                }
            }

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Insert,
                    "[FDM90].[dbo].[Facebook]",
                    new string[] { "UserId", "PageName" }, 
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }

        [TestMethod]
        public void ReadSpecificCreds_GivenUserId_CorrectValuesSentToConnection()
        {
            //arrange
            FacebookCredentials specificCreds = _returningCreds.First(x => x.UserId == _specificGuid);

            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < 0).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(specificCreds.UserId);
            _mockIDataReader.Setup(reader => reader["PageName"]).Returns(specificCreds.PageName);
            _mockIDataReader.Setup(reader => reader["PermanentAccessToken"]).Returns(specificCreds.PermanentAccessToken);

            //act
            var result = _facebookRepo.ReadSpecific(specificCreds);

            //assert
            Assert.AreEqual(1, _parameterObjects.Count);
            Assert.AreEqual("@UserId", _parameterObjects.Cast<SqlParameter>().ToArray()[0].ParameterName);
            Assert.AreEqual(_specificGuid, _parameterObjects.Cast<SqlParameter>().ToArray()[0].Value);

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Select,
                    "[FDM90].[dbo].[Facebook]",
                    new[] { "UserId" },
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }

        [TestMethod]
        public void ReadSpecificCreds_GivenUserId_CorrectValueReturned()
        {
            //arrange
            FacebookCredentials specificCreds = _returningCreds.First(x => x.UserId == _specificGuid);

            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < 0).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(specificCreds.UserId);
            _mockIDataReader.Setup(reader => reader["PageName"]).Returns(specificCreds.PageName);
            _mockIDataReader.Setup(reader => reader["PermanentAccessToken"]).Returns(specificCreds.PermanentAccessToken);

            //act
            var result = _facebookRepo.ReadSpecific(specificCreds);

            //assert
            Assert.AreEqual(specificCreds.UserId, result.UserId);
            Assert.AreEqual(specificCreds.PageName, result.PageName);
            Assert.AreEqual(specificCreds.PermanentAccessToken, result.PermanentAccessToken);
        }

        [TestMethod]
        public void ReadAllCreds_GivenMethodCall_CorrectValuesSentToConnection()
        {
            //arrange
            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < _returningCreds.Count - 1).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(() => _returningCreds[count].UserId);
            _mockIDataReader.Setup(reader => reader["PageName"]).Returns(() => _returningCreds[count].PageName);
            _mockIDataReader.Setup(reader => reader["PermanentAccessToken"]).Returns(() => _returningCreds[count].PermanentAccessToken);

            //act
            var result = _facebookRepo.ReadAll();

            //assert
            Assert.AreEqual(0, _parameterObjects.Count);

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Select,
                    "[FDM90].[dbo].[Facebook]",
                    new[] { "UserId" },
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }

        [TestMethod]
        public void ReadAllCreds_GivenUserId_CorrectValueReturned()
        {
            //arrange
            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < _returningCreds.Count - 1).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(() => _returningCreds[count].UserId);
            _mockIDataReader.Setup(reader => reader["PageName"]).Returns(() => _returningCreds[count].PageName);
            _mockIDataReader.Setup(reader => reader["PermanentAccessToken"]).Returns(() => _returningCreds[count].PermanentAccessToken);

            //act
            var result = _facebookRepo.ReadAll().ToList();

            //assert
            Assert.AreEqual(_returningCreds.Count, result.Count);

            for(int i = 0; i < _returningCreds.Count; i++)
            {
                Assert.AreEqual(_returningCreds[i].UserId, result[i].UserId);
                Assert.AreEqual(_returningCreds[i].PageName, result[i].PageName);
                Assert.AreEqual(_returningCreds[i].PermanentAccessToken, result[i].PermanentAccessToken);
            }
        }

        [TestMethod]
        public void UpdateCreds_GivenUpdatedCreds_CorrectValuesSentToConnection()
        {
            //arrange
            FacebookCredentials specificCreds = _returningCreds.First(x => x.UserId == _specificGuid);

            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < 0).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(specificCreds.UserId);
            _mockIDataReader.Setup(reader => reader["PageName"]).Returns(specificCreds.PageName);
            _mockIDataReader.Setup(reader => reader["PermanentAccessToken"]).Returns(specificCreds.PermanentAccessToken);

            specificCreds.PermanentAccessToken = "TESTACCESSTOKEN";

            //act
            _facebookRepo.Update(specificCreds);

            //assert
            Assert.AreEqual(3, _parameterObjects.Count);

            //remove parameter for read specific
            _parameterObjects.RemoveAt(0);

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Update,
                    "[FDM90].[dbo].[Facebook]",
                    new[] { "PermanentAccessToken", "UserId" },
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString, 1));
        }

        [TestMethod]
        public void DeleteCreds_GivenCredsId_CorrectValuesSentToConnection()
        {
            //arrange
            FacebookCredentials deleteCreds = new FacebookCredentials()
            {
                UserId = Guid.NewGuid()
            };

            //act
            _facebookRepo.Delete(deleteCreds);

            //assert
            Assert.AreEqual(1, _parameterObjects.Count);

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Delete,
                    "[FDM90].[dbo].[Facebook]",
                    new[] { "UserId" },
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }
    }
}
