using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using FDM90.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FDM90.Repository;
using Moq;

namespace FDM90UnitTests
{
    [TestClass]
    public class UserRepositoryUnitTests
    {
        private Mock<IDbConnection> _mockIDbConnection;
        private Mock<IDbCommand> _mockIDbCommand;
        private Mock<IDataParameterCollection> _mockIDataParameters;
        private Mock<IDataReader> _mockIDataReader;
        private IList<object> _parameterObjects = new List<object>();
        private string setSqlString = String.Empty;
        private static Guid _specificGuid = Guid.NewGuid();

        private List<User> _returningUsers = new List<User>()
        {
            new User()
            {
                UserId = new Guid(),
                UserName = "Test User 1",
                Password = "Test1",
                EmailAddress = "TestUser1@tests.com"
            },
            new User()
            {
                UserId = new Guid(),
                UserName = "Test User 2",
                Password = "Test2",
                EmailAddress = "TestUser2@tests.com"
            },
            new User()
            {
                UserId = _specificGuid,
                UserName = "Test User 3",
                Password = "Test3",
                EmailAddress = "TestUser3@tests.com"
            },
            new User()
            {
                UserId = new Guid(),
                UserName = "Test User 4",
                Password = "Test4",
                EmailAddress = "TestUser4@tests.com"
            },
        };

        private UserRepository _userRepo;
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

            _userRepo = new UserRepository(_mockIDbConnection.Object);
        }

        [TestCleanup]
        public void CleanUp()
        {
            _mockIDbConnection = null;
            _mockIDbCommand = null;
            _mockIDataParameters = null;
            _mockIDataReader = null;
            setSqlString = string.Empty;
            _userRepo = null;
            count = -1;
        }

        [TestMethod]
        public void ConstructorTest_CallToParameterlessConstructor_ReturnsTrueIfObjectIsNotNull()
        {
            //arrange

            //act
            _userRepo = new UserRepository();

            //assert
            Assert.IsNotNull(_userRepo);
        }

        [TestMethod]
        public void CreateUser_GivenValues_CorrectValuesSentToConnection()
        {
            //arrange
            User testUser = new User()
            {
                UserId = new Guid(),
                UserName = "TestUserName",
                EmailAddress = "Test@Email.com",
                Password = "TestPassword"
            };

            //act
            _userRepo.Create(testUser);

            //assert
            Assert.AreEqual(7, _parameterObjects.Count);
            foreach (var property in testUser.GetType().GetProperties())
            {
                if (property.GetValue(testUser) == null) continue;
                foreach (var parameter in _parameterObjects)
                {
                    var sqlParameter = (SqlParameter) parameter;

                    if (!sqlParameter.ParameterName.Contains(property.Name)) continue;
                    Assert.AreEqual("@" + property.Name, sqlParameter.ParameterName);
                    Assert.AreEqual(property.GetValue(testUser), sqlParameter.Value);
                }
            }

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Insert,
                    "[FDM90].[dbo].[User]",
                    testUser.GetType()
                        .GetProperties()
                        .Where(x => x.GetValue(testUser) != null)
                        .Select(s => s.Name)
                        .ToArray(),
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }

        [TestMethod]
        public void ReadSpecificUser_GivenUserId_CorrectValuesSentToConnection()
        {
            //arrange
            User specificUser = _returningUsers.First(x => x.UserId == _specificGuid);

            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < 0).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(specificUser.UserId);
            _mockIDataReader.Setup(reader => reader["UserName"]).Returns(specificUser.UserName);
            _mockIDataReader.Setup(reader => reader["EmailAddress"]).Returns(specificUser.EmailAddress);
            _mockIDataReader.Setup(reader => reader["Password"]).Returns(specificUser.Password);
            _mockIDataReader.Setup(reader => reader["Facebook"]).Returns(specificUser.Facebook);
            _mockIDataReader.Setup(reader => reader["Twitter"]).Returns(() => _returningUsers[count].Twitter);
            _mockIDataReader.Setup(reader => reader["Campaigns"]).Returns(() => _returningUsers[count].Campaigns);

            //act
            var result = _userRepo.ReadSpecific(specificUser);

            //assert
            Assert.AreEqual(1, _parameterObjects.Count);
            Assert.AreEqual("@SpecificUser", _parameterObjects.Cast<SqlParameter>().ToArray()[0].ParameterName);
            Assert.AreEqual(_specificGuid.ToString(), _parameterObjects.Cast<SqlParameter>().ToArray()[0].Value);

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Select,
                    "[FDM90].[dbo].[User]",
                    new[] {"UserId"},
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }

        [TestMethod]
        public void ReadSpecificUser_GivenUserId_CorrectValueReturned()
        {
            //arrange
            User specificUser = _returningUsers.First(x => x.UserId == _specificGuid);

            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < 0).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(specificUser.UserId);
            _mockIDataReader.Setup(reader => reader["UserName"]).Returns(specificUser.UserName);
            _mockIDataReader.Setup(reader => reader["EmailAddress"]).Returns(specificUser.EmailAddress);
            _mockIDataReader.Setup(reader => reader["Password"]).Returns(specificUser.Password);
            _mockIDataReader.Setup(reader => reader["Facebook"]).Returns(specificUser.Facebook);
            _mockIDataReader.Setup(reader => reader["Twitter"]).Returns(() => _returningUsers[count].Twitter);
            _mockIDataReader.Setup(reader => reader["Campaigns"]).Returns(() => _returningUsers[count].Campaigns);

            //act
            var result = _userRepo.ReadSpecific(specificUser);

            //assert
            Assert.AreEqual(specificUser.UserId, result.UserId);
            Assert.AreEqual(specificUser.UserName, result.UserName);
            Assert.AreEqual(specificUser.Password, result.Password);
            Assert.AreEqual(specificUser.EmailAddress, result.EmailAddress);
        }

        [TestMethod]
        public void ReadSpecificUser_GivenUserName_CorrectValuesSentToConnection()
        {
            //arrange
            User specificUser = _returningUsers.First(x => x.UserName == "Test User 2");

            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < 0).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(specificUser.UserId);
            _mockIDataReader.Setup(reader => reader["UserName"]).Returns(specificUser.UserName);
            _mockIDataReader.Setup(reader => reader["EmailAddress"]).Returns(specificUser.EmailAddress);
            _mockIDataReader.Setup(reader => reader["Password"]).Returns(specificUser.Password);
            _mockIDataReader.Setup(reader => reader["Facebook"]).Returns(specificUser.Facebook);
            _mockIDataReader.Setup(reader => reader["Twitter"]).Returns(() => _returningUsers[count].Twitter);
            _mockIDataReader.Setup(reader => reader["Campaigns"]).Returns(() => _returningUsers[count].Campaigns);

            //act
            var result = _userRepo.ReadSpecific(specificUser);

            //assert
            Assert.AreEqual(1, _parameterObjects.Count);
            Assert.AreEqual("@SpecificUser", _parameterObjects.Cast<SqlParameter>().ToArray()[0].ParameterName);
            Assert.AreEqual("Test User 2", _parameterObjects.Cast<SqlParameter>().ToArray()[0].Value);

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Select,
                    "[FDM90].[dbo].[User]",
                    new[] {"UserName"},
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }

        [TestMethod]
        public void ReadSpecificUser_GivenUserName_CorrectValueReturned()
        {
            //arrange
            User specificUser = _returningUsers.First(x => x.UserName == "Test User 2");

            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < 0).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(specificUser.UserId);
            _mockIDataReader.Setup(reader => reader["UserName"]).Returns(specificUser.UserName);
            _mockIDataReader.Setup(reader => reader["EmailAddress"]).Returns(specificUser.EmailAddress);
            _mockIDataReader.Setup(reader => reader["Password"]).Returns(specificUser.Password);
            _mockIDataReader.Setup(reader => reader["Facebook"]).Returns(specificUser.Facebook);
            _mockIDataReader.Setup(reader => reader["Twitter"]).Returns(() => _returningUsers[count].Twitter);
            _mockIDataReader.Setup(reader => reader["Campaigns"]).Returns(() => _returningUsers[count].Campaigns);

            //act
            var result = _userRepo.ReadSpecific(specificUser);

            //assert
            Assert.AreEqual(specificUser.UserId, result.UserId);
            Assert.AreEqual(specificUser.UserName, result.UserName);
            Assert.AreEqual(specificUser.Password, result.Password);
            Assert.AreEqual(specificUser.EmailAddress, result.EmailAddress);
        }

        [TestMethod]
        public void UpdateUser_GivenUpdatedUser_CorrectValuesSentToConnection()
        {
            //arrange
            User specificUser = _returningUsers.First(x => x.UserId == _specificGuid);

            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < 0).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(specificUser.UserId);
            _mockIDataReader.Setup(reader => reader["UserName"]).Returns(specificUser.UserName);
            _mockIDataReader.Setup(reader => reader["EmailAddress"]).Returns(specificUser.EmailAddress);
            _mockIDataReader.Setup(reader => reader["Password"]).Returns(specificUser.Password);
            _mockIDataReader.Setup(reader => reader["Facebook"]).Returns(specificUser.Facebook);
            _mockIDataReader.Setup(reader => reader["Twitter"]).Returns(() => _returningUsers[count].Twitter);
            _mockIDataReader.Setup(reader => reader["Campaigns"]).Returns(() => _returningUsers[count].Campaigns);

            specificUser.UserName = "Updated UserName";

            //act
            _userRepo.Update(specificUser);

            //assert
            Assert.AreEqual(3, _parameterObjects.Count);

            //remove parameter for read specific
            _parameterObjects.RemoveAt(0);

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Update,
                    "[FDM90].[dbo].[User]",
                    new[] { "UserName", "UserId" },
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString, 1));
        }

        [TestMethod]
        public void DeleteUser_GivenUser_CorrectValuesSentToConnection()
        {
            //arrange
            User specificUser = _returningUsers.First(x => x.UserId == _specificGuid);

            //act
            _userRepo.Delete(specificUser);

            //assert
            Assert.AreEqual(1, _parameterObjects.Count);

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Delete,
                    "[FDM90].[dbo].[User]",
                    new[] { "UserId" },
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }
    }
}
