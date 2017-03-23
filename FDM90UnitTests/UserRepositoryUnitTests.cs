using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using FDM90.Model;
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

        private UserRepository _userRepo;

        [TestInitialize]
        public void StartUp()
        {
            _mockIDataParameters = new Mock<IDataParameterCollection>();
            _mockIDataParameters.Setup(parameters => parameters.Add(It.IsAny<object>())).Callback((object value) =>
            {
                _parameterObjects.Add(value);
            });

            _mockIDataReader = new Mock<IDataReader>();
            _mockIDataReader.SetupSequence(reader => reader.Read()).Returns(true).Returns(true).Returns(false);
            _mockIDataReader.SetupSequence(reader => reader["Id"]).Returns(123).Returns(312);
            _mockIDataReader.SetupSequence(reader => reader["Name"]).Returns("Test Name").Returns("Test Name");

            _mockIDbCommand = new Mock<IDbCommand>();
            _mockIDbCommand.Setup(command => command.ExecuteNonQuery());
            _mockIDbCommand.Setup(command => command.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(_mockIDataReader.Object);
            _mockIDbCommand.Setup(command => command.Parameters).Returns(_mockIDataParameters.Object);
            _mockIDbCommand.SetupSet(command => command.CommandText = It.IsAny<string>())
                .Callback((string stringValue) => setSqlString = stringValue);

            _mockIDbConnection = new Mock<IDbConnection>();
            _mockIDbConnection.Setup(connection => connection.Open());
            _mockIDbConnection.Setup(connection => connection.CreateCommand()).Returns(_mockIDbCommand.Object);
            _mockIDbConnection.Setup(connection => connection.Dispose());

            _userRepo = new UserRepository(_mockIDbConnection.Object);
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
            Assert.AreEqual(_parameterObjects.Count, 4);
            foreach (var property in testUser.GetType().GetProperties())
            {
                if (property.GetValue(testUser) == null) continue;
                foreach (var parameter in _parameterObjects)
                {
                    var sqlParameter = (SqlParameter)parameter;

                    if (!sqlParameter.ParameterName.Contains(property.Name)) continue;
                    Assert.AreEqual(sqlParameter.ParameterName, "@" + property.Name);
                    Assert.AreEqual(sqlParameter.Value, property.GetValue(testUser));
                }
            }

            Assert.IsTrue(CheckSqlStatementString(StatementType.Insert, "[FDM90].[dbo].[User]", new []{ "UserId", "UserName", "EmailAddress", "Password" }
            , _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }

        private bool CheckSqlStatementString(StatementType statementType, string sqlTable, string[] columns,
            string[] parameters, string stringToTest)
        {
            string columnString = string.Empty;

            foreach (var col in columns)
            {
                columnString += @"\[" + col +@"\].\s";
            }
            columnString = columnString.Substring(0, columnString.LastIndexOf('.'));

            string parameterString = string.Empty;

            foreach (var param in parameters)
            {
                parameterString += param + @".\s";
            }

            Regex format = null;

            switch (statementType)
            {
                case StatementType.Select:
                    break;
                case StatementType.Insert:
                    format = new Regex(@"INSERT INTO\s.*" + sqlTable.Replace("[", @"\[").Replace("]", @"\]").Replace(".", @"\.") + @"." + columnString + @". VALUES ." + parameterString + @";");
                    break;
                case StatementType.Update:
                    break;
                case StatementType.Delete:
                    break;
                case StatementType.Batch:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(statementType), statementType, null);
            }

            return format.IsMatch(stringToTest);
        }
    }
}
