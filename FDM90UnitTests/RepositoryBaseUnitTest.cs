using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using FDM90.Model;
using FDM90.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FDM90UnitTests
{
    [TestClass]
    public class RepositoryBaseUnitTest
    {
        private Mock<IDbConnection> _mockIDbConnection;
        private Mock<IDbCommand> _mockIDbCommand;
        private Mock<IDataParameterCollection> _mockIDataParameters;
        private Mock<IDataReader> _mockIDataReader;
        private IList<object> _parameterObjects = new List<object>();
        private TestDerivedClass _derivedClass;
        private string setSqlString = String.Empty;

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
            _mockIDbCommand.Setup(command => command.ExecuteNonQuery()).Verifiable();
            _mockIDbCommand.Setup(command => command.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(_mockIDataReader.Object).Verifiable();
            _mockIDbCommand.Setup(command => command.Parameters).Returns(_mockIDataParameters.Object);
            _mockIDbCommand.SetupSet(command => command.CommandText = It.IsAny<string>())
                .Callback((string stringValue) => setSqlString = stringValue);

            _mockIDbConnection = new Mock<IDbConnection>();
            _mockIDbConnection.Setup(connection => connection.Open()).Verifiable();
            _mockIDbConnection.Setup(connection => connection.CreateCommand()).Returns(_mockIDbCommand.Object).Verifiable();
            _mockIDbConnection.Setup(connection => connection.Dispose()).Verifiable();

            _derivedClass = new TestDerivedClass(_mockIDbConnection.Object);
        }

        [TestCleanup]
        public void CleanUp()
        {
            _mockIDbConnection = null;
            _mockIDbCommand = null;
            _mockIDataParameters = null;
            _mockIDataReader = null;
            _parameterObjects = null;
            _derivedClass = null;
            setSqlString = null;
        }

        [TestMethod]
        public void RepositoryBase_SendVoidCommand_MethodsVerfied()
        {
            //arrange

            //act
            _derivedClass.NonQuery(new TestObject("1230", "Test Name"));

            //assert
            _mockIDbConnection.Verify(connection => connection.Open(), Times.Once);
            _mockIDbConnection.Verify(connection => connection.CreateCommand(), Times.Once);
            _mockIDbConnection.Verify(connection => connection.Dispose(), Times.Once);

            _mockIDbCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);
        }

        [TestMethod]
        public void RepositoryBase_SendVoidCommand_ValuesCorrect()
        {
            //arrange

            //act
            _derivedClass.NonQuery(new TestObject("1230", "Test Name"));

            //assert
            StringAssert.Contains(setSqlString, "Test_Table");
            StringAssert.Contains(setSqlString, "Id");
            StringAssert.Contains(setSqlString, "Name");
            StringAssert.Contains(setSqlString, "@Id");
            StringAssert.Contains(setSqlString, "@Name");

            CollectionAssert.AllItemsAreInstancesOfType((ICollection)_parameterObjects, typeof(SqlParameter));
            Assert.AreEqual(_parameterObjects.Count, 2);
            //CollectionAssert.AreEqual((ICollection)_parameterObjects, new [] { new SqlParameter("@Id", "1230"), new SqlParameter("@Name", "Test Name") });
            //CollectionAssert.Contains((ICollection)_parameterObjects, new SqlParameter("@Id", "1230"));
            //CollectionAssert.Contains((ICollection)_parameterObjects, new SqlParameter("@Name", "Test Name"));

            //Assert.IsTrue(Regex.IsMatch(setSqlString, ".*?\bTest_Table\b.*?\bId\b.*?\bName\b.*?\\s.*"));
            //StringAssert.Matches(setSqlString, new Regex(".*?\bTest_Table\b.*?\bId\b.*?\bName\b.*?\\s.*"));
        }

        [TestMethod]
        public void RepositoryBase_SendReaderCommand_MethodsVerfied()
        {
            //arrange

            //act
            _derivedClass.ReturnQuery("Test Name");

            //assert
            _mockIDbConnection.Verify(connection => connection.Open(), Times.Once);
            _mockIDbConnection.Verify(connection => connection.CreateCommand(), Times.Once);
            _mockIDbConnection.Verify(connection => connection.Dispose(), Times.Once);

            _mockIDbCommand.Verify(command => command.ExecuteReader(It.IsAny<CommandBehavior>()), Times.Once);
        }

        [TestMethod]
        public void RepositoryBase_SendReaderCommand_ValuesCorrect()
        {
            //arrange

            //act
            var result = _derivedClass.ReturnQuery("Test Name");

            //assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("123", result[0].Id);
            Assert.AreEqual("Test Name", result[0].Name);
            Assert.AreEqual("312", result[1].Id);
            Assert.AreEqual("Test Name", result[1].Name);


            StringAssert.Contains(setSqlString, "Test_Table");
            StringAssert.Contains(setSqlString, "WHERE");
            StringAssert.Contains(setSqlString, "Name");
            StringAssert.Contains(setSqlString, "@Name");

            CollectionAssert.AllItemsAreInstancesOfType((ICollection)_parameterObjects, typeof(SqlParameter));
            Assert.AreEqual(_parameterObjects.Count, 1);
            //CollectionAssert.AreEqual((ICollection)_parameterObjects, new [] { new SqlParameter("@Id", "1230"), new SqlParameter("@Name", "Test Name") });
            //CollectionAssert.Contains((ICollection)_parameterObjects, new SqlParameter("@Id", "1230"));
            //CollectionAssert.Contains((ICollection)_parameterObjects, new SqlParameter("@Name", "Test Name"));

            //Assert.IsTrue(Regex.IsMatch(setSqlString, ".*?\bTest_Table\b.*?\bId\b.*?\bName\b.*?\\s.*"));
            //StringAssert.Matches(setSqlString, new Regex(".*?\bTest_Table\b.*?\bId\b.*?\bName\b.*?\\s.*"));
        }
    }

    public class TestDerivedClass : RepositoryBase<TestObject>
    {
        protected override string _table => "Test_Table";

        public TestDerivedClass(IDbConnection connection):base(connection)
        {
                
        }

        public void NonQuery(TestObject objectToCreate)
        {
            string sqlText = SQLHelper.Insert + _table + SQLHelper.OpenBracket +
                        "[Id], [Name]"
                        + SQLHelper.CloseBracket + SQLHelper.Values + SQLHelper.OpenBracket
                        + "@Id, @Name"
                        + SQLHelper.CloseBracket + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = {
                            new SqlParameter("@Id", objectToCreate.Id),
                            new SqlParameter("@Name", objectToCreate.Name),
                        };

            SendVoidCommand(sqlText, parameters);
        }

        public List<TestObject> ReturnQuery(string name)
        {
            string sql = SQLHelper.SelectAll
                    + _table + SQLHelper.Where +
                     " [Name] = @Name " + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@Name", name),
                        };

            return SendReaderCommand(sql, parameters);

        }

        public override TestObject SetProperties(IDataReader reader)
        {
            return new TestObject(reader["Id"].ToString(), reader["Name"].ToString());
        }
    }

    public class TestObject
    {
        public TestObject(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; set; }
        public string Name { get; set; }
    }
}
