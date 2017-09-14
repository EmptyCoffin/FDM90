using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FDM90.Repository;
using Moq;
using System.Data;
using System.Collections.Generic;
using FDM90.Models;
using System.Data.SqlClient;
using System.Linq;

namespace FDM90UnitTests
{
    [TestClass]
    public class CampaignRepositoryUnitTests
    {
        private Mock<IDbConnection> _mockIDbConnection;
        private Mock<IDbCommand> _mockIDbCommand;
        private Mock<IDataParameterCollection> _mockIDataParameters;
        private Mock<IDataReader> _mockIDataReader;
        private IList<object> _parameterObjects = new List<object>();
        private string setSqlString = String.Empty;
        private int count = -1;
        private static Guid specificGuid = Guid.NewGuid();
        private List<Campaign> _returningCampaigns = new List<Campaign>()
        {
            new Campaign()
            {
                UserId = specificGuid,
                CampaignName = "CampaignName1",
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now.AddMonths(3),
                Targets = "Test Targets 1",
                Progress = "Test Progress 1"
            },
            new Campaign()
            {
                UserId = Guid.NewGuid(),
                CampaignName = "CampaignName2",
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now.AddMonths(2),
                Targets = "Test Targets 2"
            },
            new Campaign()
            {
                UserId = specificGuid,
                CampaignName = "CampaignName3",
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now.AddMonths(4),
                Targets = "Test Targets 3"
            },
            new Campaign()
            {
                UserId = Guid.NewGuid(),
                CampaignName = "CampaignName2",
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now.AddMonths(2),
                Targets = "Test Targets 2"
            }
        };

        private CampaignRepository _campaignRepo;

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

            _campaignRepo = new CampaignRepository(_mockIDbConnection.Object);
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
            _campaignRepo = null;
            count = -1;
            _returningCampaigns = null;
        }

        [TestMethod]
        public void ConstructorTest_CallToParameterlessConstructor_ReturnsTrueIfObjectIsNotNull()
        {
            //arrange

            //act
            _campaignRepo = new CampaignRepository();

            //assert
            Assert.IsNotNull(_campaignRepo);
        }

        [TestMethod]
        public void CreateCreds_GivenValuesAndOptionalNull_CorrectValuesSentToConnection()
        {
            //arrange
            Campaign campaign = new Campaign()
            {
                UserId = new Guid(),
                CampaignName = "TestCampaignName",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(2),
                Targets = "Create Targets"
            };

            //act
            _campaignRepo.Create(campaign);

            //assert
            Assert.AreEqual(6, _parameterObjects.Count);
            foreach (var property in campaign.GetType().GetProperties())
            {
                if (property.GetValue(campaign) == null) continue;
                foreach (var parameter in _parameterObjects)
                {
                    var sqlParameter = (SqlParameter)parameter;

                    if (!sqlParameter.ParameterName.Contains(property.Name)) continue;
                    Assert.AreEqual("@" + property.Name, sqlParameter.ParameterName);
                    Assert.AreEqual(property.GetValue(campaign), sqlParameter.Value);
                }
            }

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Insert,
                    "[FDM90].[dbo].[Campaigns]",
                    new string[] { "UserId", "CampaignName", "StartDate", "EndDate", "Targets", "Progress" },
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }

        [TestMethod]
        public void CreateCreds_GivenValuesAndOptionalPopulated_CorrectValuesSentToConnection()
        {
            //arrange
            Campaign campaign = new Campaign()
            {
                UserId = new Guid(),
                CampaignName = "TestCampaignName",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(2),
                Targets = "Create Targets",
                Progress = "Create Progress"
            };

            //act
            _campaignRepo.Create(campaign);

            //assert
            Assert.AreEqual(6, _parameterObjects.Count);
            foreach (var property in campaign.GetType().GetProperties())
            {
                if (property.GetValue(campaign) == null) continue;
                foreach (var parameter in _parameterObjects)
                {
                    var sqlParameter = (SqlParameter)parameter;

                    if (!sqlParameter.ParameterName.Contains(property.Name)) continue;
                    Assert.AreEqual("@" + property.Name, sqlParameter.ParameterName);
                    Assert.AreEqual(property.GetValue(campaign), sqlParameter.Value);
                }
            }

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Insert,
                    "[FDM90].[dbo].[Campaigns]",
                    new string[] { "UserId", "CampaignName", "StartDate", "EndDate", "Targets", "Progress" },
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }

        [TestMethod]
        public void ReadSpecificCampaign_GivenUserId_CorrectValuesSentToConnection()
        {
            //arrange
            Random rdm = new Random();
            Campaign specificCampaign = _returningCampaigns[(int)Math.Round(double.Parse(rdm.Next(0, _returningCampaigns.Count).ToString()))];

            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < 0).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(specificCampaign.UserId);
            _mockIDataReader.Setup(reader => reader["CampaignName"]).Returns(specificCampaign.CampaignName);
            _mockIDataReader.Setup(reader => reader["StartDate"]).Returns(specificCampaign.StartDate);
            _mockIDataReader.Setup(reader => reader["EndDate"]).Returns(specificCampaign.EndDate);
            _mockIDataReader.Setup(reader => reader["Targets"]).Returns(specificCampaign.Targets);
            _mockIDataReader.Setup(reader => reader["Progress"]).Returns(specificCampaign.Progress);

            //act
            var result = _campaignRepo.ReadSpecific(new Campaign() { UserId = specificCampaign.UserId, CampaignName = specificCampaign.CampaignName });

            //assert
            Assert.AreEqual(2, _parameterObjects.Count);
            Assert.AreEqual("@UserId", _parameterObjects.Cast<SqlParameter>().ToArray()[0].ParameterName);
            Assert.AreEqual(specificCampaign.UserId, _parameterObjects.Cast<SqlParameter>().ToArray()[0].Value);
            Assert.AreEqual("@CampaignName", _parameterObjects.Cast<SqlParameter>().ToArray()[1].ParameterName);
            Assert.AreEqual(specificCampaign.CampaignName, _parameterObjects.Cast<SqlParameter>().ToArray()[1].Value);

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Select,
                    "[FDM90].[dbo].[Campaigns]",
                    new[] { "UserId", "CampaignName" },
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }

        [TestMethod]
        public void ReadSpecificCampaign_GivenUserId_CorrectValueReturned()
        {
            //arrange
            Random rdm = new Random();
            Campaign specificCampaign = _returningCampaigns[(int)Math.Round(double.Parse(rdm.Next(0, _returningCampaigns.Count).ToString()))];

            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < 0).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(specificCampaign.UserId);
            _mockIDataReader.Setup(reader => reader["CampaignName"]).Returns(specificCampaign.CampaignName);
            _mockIDataReader.Setup(reader => reader["StartDate"]).Returns(specificCampaign.StartDate);
            _mockIDataReader.Setup(reader => reader["EndDate"]).Returns(specificCampaign.EndDate);
            _mockIDataReader.Setup(reader => reader["Targets"]).Returns(specificCampaign.Targets);
            _mockIDataReader.Setup(reader => reader["Progress"]).Returns(specificCampaign.Progress);

            //act
            var result = _campaignRepo.ReadSpecific(new Campaign() { UserId = specificCampaign.UserId });

            //assert
            Assert.AreEqual(specificCampaign.UserId, result.UserId);
            Assert.AreEqual(specificCampaign.CampaignName, result.CampaignName);
            Assert.AreEqual(specificCampaign.StartDate.Date, result.StartDate.Date);
            Assert.AreEqual(specificCampaign.EndDate.Date, result.EndDate.Date);
            Assert.AreEqual(specificCampaign.Targets, result.Targets);
            Assert.AreEqual(specificCampaign.Progress, result.Progress);
        }

        [TestMethod]
        public void ReadAllCampaigns_GivenMethodCall_CorrectValuesSentToConnection()
        {
            //arrange
            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < _returningCampaigns.Count - 1).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(() => _returningCampaigns[count].UserId);
            _mockIDataReader.Setup(reader => reader["CampaignName"]).Returns(() => _returningCampaigns[count].CampaignName);
            _mockIDataReader.Setup(reader => reader["StartDate"]).Returns(() => _returningCampaigns[count].StartDate);
            _mockIDataReader.Setup(reader => reader["EndDate"]).Returns(() => _returningCampaigns[count].EndDate);
            _mockIDataReader.Setup(reader => reader["Targets"]).Returns(() => _returningCampaigns[count].Targets);
            _mockIDataReader.Setup(reader => reader["Progress"]).Returns(() => _returningCampaigns[count].Progress);

            //act
            var result = _campaignRepo.ReadAll();

            //assert
            Assert.AreEqual(0, _parameterObjects.Count);

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Select,
                    "[FDM90].[dbo].[Campaigns]",
                    new string[0],
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }

        [TestMethod]
        public void ReadAllCampaigns_GivenMethodCall_CorrectValueReturned()
        {
            //arrange
            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < _returningCampaigns.Count - 1).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(() => _returningCampaigns[count].UserId);
            _mockIDataReader.Setup(reader => reader["CampaignName"]).Returns(() => _returningCampaigns[count].CampaignName);
            _mockIDataReader.Setup(reader => reader["StartDate"]).Returns(() => _returningCampaigns[count].StartDate);
            _mockIDataReader.Setup(reader => reader["EndDate"]).Returns(() => _returningCampaigns[count].EndDate);
            _mockIDataReader.Setup(reader => reader["Targets"]).Returns(() => _returningCampaigns[count].Targets);
            _mockIDataReader.Setup(reader => reader["Progress"]).Returns(() => _returningCampaigns[count].Progress);

            //act
            var result = _campaignRepo.ReadAll().ToList();

            //assert
            Assert.AreEqual(_returningCampaigns.Count, result.Count);

            for (int i = 0; i < _returningCampaigns.Count; i++)
            {
                Assert.AreEqual(_returningCampaigns[i].UserId, result[i].UserId);
                Assert.AreEqual(_returningCampaigns[i].CampaignName, result[i].CampaignName);
                Assert.AreEqual(_returningCampaigns[i].StartDate.Date, result[i].StartDate.Date);
                Assert.AreEqual(_returningCampaigns[i].EndDate.Date, result[i].EndDate.Date);
                Assert.AreEqual(_returningCampaigns[i].Targets, result[i].Targets);
                Assert.AreEqual(_returningCampaigns[i].Progress, result[i].Progress);
            }
        }

        [TestMethod]
        public void ReadMultipleSpecificCampaigns_GivenMethodCall_CorrectValuesSentToConnection()
        {
            //arrange
            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < _returningCampaigns.Count(x => x.UserId == specificGuid) - 1).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(() => _returningCampaigns.Where(x => x.UserId == specificGuid).ToArray()[count].UserId);
            _mockIDataReader.Setup(reader => reader["CampaignName"]).Returns(() => _returningCampaigns.Where(x => x.UserId == specificGuid).ToArray()[count].CampaignName);
            _mockIDataReader.Setup(reader => reader["StartDate"]).Returns(() => _returningCampaigns.Where(x => x.UserId == specificGuid).ToArray()[count].StartDate);
            _mockIDataReader.Setup(reader => reader["EndDate"]).Returns(() => _returningCampaigns.Where(x => x.UserId == specificGuid).ToArray()[count].EndDate);
            _mockIDataReader.Setup(reader => reader["Targets"]).Returns(() => _returningCampaigns.Where(x => x.UserId == specificGuid).ToArray()[count].Targets);
            _mockIDataReader.Setup(reader => reader["Progress"]).Returns(() => _returningCampaigns.Where(x => x.UserId == specificGuid).ToArray()[count].Progress);

            //act
            var result = _campaignRepo.ReadMultipleSpecific(specificGuid.ToString());

            //assert
            Assert.AreEqual(1, _parameterObjects.Count);

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Select,
                    "[FDM90].[dbo].[Campaigns]",
                    new[] { "UserId" },
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }

        [TestMethod]
        public void ReadMultipleSpecificCampaigns_GivenMethodCall_CorrectValueReturned()
        {
            //arrange
            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < _returningCampaigns.Count(x => x.UserId == specificGuid) - 1).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(() => _returningCampaigns.Where(x => x.UserId == specificGuid).ToArray()[count].UserId);
            _mockIDataReader.Setup(reader => reader["CampaignName"]).Returns(() => _returningCampaigns.Where(x => x.UserId == specificGuid).ToArray()[count].CampaignName);
            _mockIDataReader.Setup(reader => reader["StartDate"]).Returns(() => _returningCampaigns.Where(x => x.UserId == specificGuid).ToArray()[count].StartDate);
            _mockIDataReader.Setup(reader => reader["EndDate"]).Returns(() => _returningCampaigns.Where(x => x.UserId == specificGuid).ToArray()[count].EndDate);
            _mockIDataReader.Setup(reader => reader["Targets"]).Returns(() => _returningCampaigns.Where(x => x.UserId == specificGuid).ToArray()[count].Targets);
            _mockIDataReader.Setup(reader => reader["Progress"]).Returns(() => _returningCampaigns.Where(x => x.UserId == specificGuid).ToArray()[count].Progress);

            //act
            var result = _campaignRepo.ReadMultipleSpecific(specificGuid.ToString()).ToList();

            //assert
            Assert.AreEqual(_returningCampaigns.Count(x => x.UserId == specificGuid), result.Count);

            for (int i = 0; i < _returningCampaigns.Count(x => x.UserId == specificGuid); i++)
            {
                Assert.AreEqual(_returningCampaigns.Where(x => x.UserId == specificGuid).ToArray()[i].UserId, result[i].UserId);
                Assert.AreEqual(_returningCampaigns.Where(x => x.UserId == specificGuid).ToArray()[i].CampaignName, result[i].CampaignName);
                Assert.AreEqual(_returningCampaigns.Where(x => x.UserId == specificGuid).ToArray()[i].StartDate.Date, result[i].StartDate.Date);
                Assert.AreEqual(_returningCampaigns.Where(x => x.UserId == specificGuid).ToArray()[i].EndDate.Date, result[i].EndDate.Date);
                Assert.AreEqual(_returningCampaigns.Where(x => x.UserId == specificGuid).ToArray()[i].Targets, result[i].Targets);
                Assert.AreEqual(_returningCampaigns.Where(x => x.UserId == specificGuid).ToArray()[i].Progress, result[i].Progress);
            }
        }

        [TestMethod]
        public void UpdateCampaigns_GivenUpdatedCreds_CorrectValuesSentToConnection()
        {
            //arrange
            Random rdm = new Random();
            Campaign specificCampaign = _returningCampaigns[(int)Math.Round(double.Parse(rdm.Next(0, _returningCampaigns.Count).ToString()))];
            Campaign updatedCampaign = new Campaign() { UserId = specificCampaign.UserId };

            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < 0).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(specificCampaign.UserId);
            _mockIDataReader.Setup(reader => reader["CampaignName"]).Returns(specificCampaign.CampaignName);
            _mockIDataReader.Setup(reader => reader["StartDate"]).Returns(specificCampaign.StartDate);
            _mockIDataReader.Setup(reader => reader["EndDate"]).Returns(specificCampaign.EndDate);
            _mockIDataReader.Setup(reader => reader["Targets"]).Returns(specificCampaign.Targets);
            _mockIDataReader.Setup(reader => reader["Progress"]).Returns(specificCampaign.Progress);

            updatedCampaign.Progress = "UPDATED PROGRESS";

            //act
            _campaignRepo.Update(updatedCampaign);

            //assert
            _parameterObjects.RemoveAt(0);
            _parameterObjects.RemoveAt(0);
            Assert.AreEqual(3, _parameterObjects.Count);

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Update,
                    "[FDM90].[dbo].[Campaigns]",
                    new[] { "Progress", "UserId", "CampaignName" },
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString, 2));
        }

        [TestMethod]
        public void DeleteCampaigns_GivenCredsId_CorrectValuesSentToConnection()
        {
            //arrange
            Campaign deleteCampaign = new Campaign() { UserId = Guid.NewGuid() };

            //act
            _campaignRepo.Delete(deleteCampaign);

            //assert
            Assert.AreEqual(1, _parameterObjects.Count);

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Delete,
                    "[FDM90].[dbo].[Campaigns]",
                    new[] { "UserId" },
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }
    }
}
