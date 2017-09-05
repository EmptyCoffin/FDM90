using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using Moq;
using System.Collections.Generic;
using FDM90.Models;
using FDM90.Repository;
using System.Data.SqlClient;
using System.Linq;

namespace FDM90UnitTests
{
    [TestClass]
    public class TwitterRepositoryUnitTests
    {
        private Mock<IDbConnection> _mockIDbConnection;
        private Mock<IDbCommand> _mockIDbCommand;
        private Mock<IDataParameterCollection> _mockIDataParameters;
        private Mock<IDataReader> _mockIDataReader;
        private IList<object> _parameterObjects = new List<object>();
        private string setSqlString = String.Empty;
        private static Guid _specificGuid = Guid.NewGuid();
        private List<TwitterCredentials> _returningCreds = new List<TwitterCredentials>()
        {
            new TwitterCredentials()
            {
                UserId = Guid.NewGuid(),
                ScreenName = "Screen Name 1",
                AccessToken = "ThisIsAPermanentAccessToken1",
                AccessTokenSecret = "ThisIsAccessTokenSecret1"
            },
            new TwitterCredentials()
            {
                UserId = _specificGuid,
                ScreenName = "Screen Name 2",
                AccessToken = "ThisIsAPermanentAccessToken2",
                AccessTokenSecret = "ThisIsAccessTokenSecret2"
            },
            new TwitterCredentials()
            {
                UserId = Guid.NewGuid(),
                AccessToken = "ThisIsAPermanentAccessToken3",
                AccessTokenSecret = "ThisIsAccessTokenSecret3"
            },
            new TwitterCredentials()
            {
                UserId = Guid.NewGuid(),
                ScreenName = "Screen Name 4",
                AccessToken = "ThisIsAPermanentAccessToken4",
                AccessTokenSecret = "ThisIsAccessTokenSecret4",
                TwitterData = "This is Twitter Data"
            },
        };

        private TwitterRepository _twitterRepo;
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

            _twitterRepo = new TwitterRepository(_mockIDbConnection.Object);
        }

        [TestCleanup]
        public void CleanUp()
        {
            _mockIDbConnection = null;
            _mockIDbCommand = null;
            _mockIDataParameters = null;
            _mockIDataReader = null;
            setSqlString = string.Empty;
            _twitterRepo = null;
            count = -1;
        }

        [TestMethod]
        public void ConstructorTest_CallToParameterlessConstructor_ReturnsTrueIfObjectIsNotNull()
        {
            //arrange

            //act
            _twitterRepo = new TwitterRepository();

            //assert
            Assert.IsNotNull(_twitterRepo);
        }

        [TestMethod]
        public void CreateCreds_GivenValues_CorrectValuesSentToConnection()
        {
            //arrange
            TwitterCredentials testCreds = new TwitterCredentials()
            {
                UserId = new Guid(),
                AccessToken = "TestAccessToken",
                AccessTokenSecret = "TestAccessTokenSecret",
                ScreenName = "TestPageName"
            };

            //act
            _twitterRepo.Create(testCreds);

            //assert
            Assert.AreEqual(4, _parameterObjects.Count);
            foreach (var property in testCreds.GetType().GetProperties().OrderBy(x => x.Name))
            {
                if (property.GetValue(testCreds) == null) continue;
                foreach (var parameter in _parameterObjects.Cast<SqlParameter>().OrderBy(x => x.ParameterName))
                {
                    var sqlParameter = (SqlParameter)parameter;

                    if (!sqlParameter.ParameterName.Equals(property.Name)) continue;
                    Assert.AreEqual("@" + property.Name, sqlParameter.ParameterName);
                    Assert.AreEqual(property.GetValue(testCreds), sqlParameter.Value);
                }
            }

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Insert,
                    "[FDM90].[dbo].[Twitter]",
                    new string[] { "UserId", "AccessToken", "AccessTokenSecret", "ScreenName" },
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }

        [TestMethod]
        public void ReadSpecificCreds_GivenUserId_CorrectValuesSentToConnection()
        {
            //arrange
            TwitterCredentials specificCreds = _returningCreds.First(x => x.UserId == _specificGuid);

            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < 0).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(specificCreds.UserId);
            _mockIDataReader.Setup(reader => reader["ScreenName"]).Returns(specificCreds.ScreenName);
            _mockIDataReader.Setup(reader => reader["AccessToken"]).Returns(specificCreds.AccessToken);
            _mockIDataReader.Setup(reader => reader["AccessTokenSecret"]).Returns(specificCreds.AccessTokenSecret);

            //act
            var result = _twitterRepo.ReadSpecific(specificCreds);

            //assert
            Assert.AreEqual(1, _parameterObjects.Count);
            Assert.AreEqual("@UserId", _parameterObjects.Cast<SqlParameter>().ToArray()[0].ParameterName);
            Assert.AreEqual(_specificGuid, _parameterObjects.Cast<SqlParameter>().ToArray()[0].Value);

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Select,
                    "[FDM90].[dbo].[Twitter]",
                    new[] { "UserId" },
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }

        [TestMethod]
        public void ReadSpecificCreds_GivenUserId_CorrectValueReturned()
        {
            //arrange
            TwitterCredentials specificCreds = _returningCreds.First(x => x.UserId == _specificGuid);

            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < 0).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(specificCreds.UserId);
            _mockIDataReader.Setup(reader => reader["ScreenName"]).Returns(specificCreds.ScreenName);
            _mockIDataReader.Setup(reader => reader["AccessToken"]).Returns(specificCreds.AccessToken);
            _mockIDataReader.Setup(reader => reader["AccessTokenSecret"]).Returns(specificCreds.AccessTokenSecret);

            //act
            var result = _twitterRepo.ReadSpecific(specificCreds);

            //assert
            Assert.AreEqual(specificCreds.UserId, result.UserId);
            Assert.AreEqual(specificCreds.ScreenName, result.ScreenName);
            Assert.AreEqual(specificCreds.AccessToken, result.AccessToken);
            Assert.AreEqual(specificCreds.AccessTokenSecret, result.AccessTokenSecret);
        }

        [TestMethod]
        public void ReadAllCreds_GivenMethodCall_CorrectValuesSentToConnection()
        {
            //arrange
            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < _returningCreds.Count - 1).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(() => _returningCreds[count].UserId);
            _mockIDataReader.Setup(reader => reader["ScreenName"]).Returns(() => _returningCreds[count].ScreenName);
            _mockIDataReader.Setup(reader => reader["AccessToken"]).Returns(() => _returningCreds[count].AccessToken);
            _mockIDataReader.Setup(reader => reader["AccessTokenSecret"]).Returns(() => _returningCreds[count].AccessTokenSecret);

            //act
            var result = _twitterRepo.ReadAll();

            //assert
            Assert.AreEqual(0, _parameterObjects.Count);

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Select,
                    "[FDM90].[dbo].[Twitter]",
                    new string[0],
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }

        [TestMethod]
        public void ReadAllCreds_GivenUserId_CorrectValueReturned()
        {
            //arrange
            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < _returningCreds.Count - 1).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(() => _returningCreds[count].UserId);
            _mockIDataReader.Setup(reader => reader["ScreenName"]).Returns(() => _returningCreds[count].ScreenName);
            _mockIDataReader.Setup(reader => reader["AccessToken"]).Returns(() => _returningCreds[count].AccessToken);
            _mockIDataReader.Setup(reader => reader["AccessTokenSecret"]).Returns(() => _returningCreds[count].AccessTokenSecret);
            _mockIDataReader.Setup(reader => reader["TwitterData"]).Returns(() => _returningCreds[count].TwitterData);

            //act
            var result = _twitterRepo.ReadAll().ToList();

            //assert
            Assert.AreEqual(_returningCreds.Count, result.Count);

            for (int i = 0; i < _returningCreds.Count; i++)
            {
                Assert.AreEqual(_returningCreds[i].UserId, result[i].UserId);
                Assert.AreEqual(_returningCreds[i].ScreenName, result[i].ScreenName);
                Assert.AreEqual(_returningCreds[i].AccessToken, result[i].AccessToken);
                Assert.AreEqual(_returningCreds[i].AccessTokenSecret, result[i].AccessTokenSecret);
                Assert.AreEqual(_returningCreds[i].TwitterData, result[i].TwitterData);
            }
        }

        [TestMethod]
        public void UpdateCreds_GivenUpdatedCreds_CorrectValuesSentToConnection()
        {
            //arrange
            TwitterCredentials specificCreds = _returningCreds.First(x => x.UserId == _specificGuid);

            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < 0).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(specificCreds.UserId);
            _mockIDataReader.Setup(reader => reader["ScreenName"]).Returns(specificCreds.ScreenName);
            _mockIDataReader.Setup(reader => reader["AccessToken"]).Returns(specificCreds.AccessToken);
            _mockIDataReader.Setup(reader => reader["AccessTokenSecret"]).Returns(specificCreds.AccessTokenSecret);

            specificCreds.AccessToken = "TESTACCESSTOKEN";

            //act
            _twitterRepo.Update(specificCreds);

            //assert
            Assert.AreEqual(3, _parameterObjects.Count);

            //remove parameter for read specific
            _parameterObjects.RemoveAt(0);

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Update,
                    "[FDM90].[dbo].[Twitter]",
                    new[] { "AccessToken", "UserId" },
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString, 1));
        }

        [TestMethod]
        public void DeleteCreds_GivenCredsId_CorrectValuesSentToConnection()
        {
            //arrange
            TwitterCredentials deleteCreds = new TwitterCredentials()
            {
                UserId = Guid.NewGuid()
            };

            //act
            _twitterRepo.Delete(deleteCreds);

            //assert
            Assert.AreEqual(1, _parameterObjects.Count);

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Delete,
                    "[FDM90].[dbo].[Twitter]",
                    new[] { "UserId" },
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }
    }
}
