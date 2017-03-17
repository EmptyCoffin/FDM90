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

            _mockIDbCommand = new Mock<IDbCommand>();
            _mockIDbCommand.Setup(command => command.ExecuteNonQuery()).Verifiable();
            _mockIDbCommand.Setup(command => command.Parameters).Returns(_mockIDataParameters.Object);
            _mockIDbCommand.SetupSet(command => command.CommandText = It.IsAny<string>())
                .Callback((string stringValue) => setSqlString = stringValue);

            _mockIDbConnection = new Mock<IDbConnection>();
            _mockIDbConnection.Setup(connection => connection.Open()).Verifiable();
            _mockIDbConnection.Setup(connection => connection.CreateCommand()).Returns(_mockIDbCommand.Object).Verifiable();
            _mockIDbConnection.Setup(connection => connection.Dispose()).Verifiable();

            _derivedClass = new TestDerivedClass(_mockIDbConnection.Object);
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

        public override TestObject SetProperties(IDataReader reader)
        {
            throw new NotImplementedException();
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
