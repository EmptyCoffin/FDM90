using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Data;
using System.Collections.Generic;
using FDM90.Repository;
using System.Data.SqlClient;
using System.Linq;

namespace FDM90UnitTests
{
    [TestClass]
    public class ProfanityRepositoryUnitTests
    {
        private Mock<IDbConnection> _mockIDbConnection;
        private Mock<IDbCommand> _mockIDbCommand;
        private Mock<IDataParameterCollection> _mockIDataParameters;
        private Mock<IDataReader> _mockIDataReader;
        private IList<object> _parameterObjects = new List<object>();
        private string setSqlString = String.Empty;
        private int count = -1;
        private List<string> _returningConfig = new List<string>()
        {
            "BadWord1",
            "BadWord2",
            "BadWord3",
        };

        private ProfanityRepository _profanityRepo;

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

            _profanityRepo = new ProfanityRepository(_mockIDbConnection.Object);
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
            _profanityRepo = null;
            count = -1;
            _returningConfig = null;
        }

        [TestMethod]
        public void ConstructorTest_CallToParameterlessConstructor_ReturnsTrueIfObjectIsNotNull()
        {
            //act
            _profanityRepo = new ProfanityRepository();

            //arrange
            Assert.IsNotNull(_profanityRepo);
        }

        [TestMethod]
        public void ReadAll_GivenMethodCall_CorrectValuesSentToConnection()
        {
            //arrange
            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < _returningConfig.Count - 1).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["Value"]).Returns(() => _returningConfig[count]);

            //act
            var result = _profanityRepo.ReadAll();

            //assert
            Assert.AreEqual(0, _parameterObjects.Count);

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Select,
                    "[FDM90].[dbo].[Profanity]",
                    new string[0],
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }

        [TestMethod]
        public void ReadAll_GivenMethodCall_CorrectValueReturned()
        {
            //arrange
            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < _returningConfig.Count - 1).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["Value"]).Returns(() => _returningConfig[count]);

            //act
            var result = _profanityRepo.ReadAll().ToList();

            //assert
            Assert.AreEqual(_returningConfig.Count, result.Count);

            for (int i = 0; i < _returningConfig.Count; i++)
            {
                Assert.AreEqual(_returningConfig[i], result[i]);
            }
        }
    }
}
