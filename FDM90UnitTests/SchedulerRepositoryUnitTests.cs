using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using FDM90.Models;
using FDM90.Repository;
using System.Linq;

namespace FDM90UnitTests
{
    [TestClass]
    public class SchedulerRepositoryUnitTests
    {
        private Mock<IDbConnection> _mockIDbConnection;
        private Mock<IDbCommand> _mockIDbCommand;
        private Mock<IDataParameterCollection> _mockIDataParameters;
        private Mock<IDataReader> _mockIDataReader;
        private IList<object> _parameterObjects = new List<object>();
        private string setSqlString = String.Empty;
        private int count = -1;
        private static Guid specificGuid = Guid.NewGuid();
        private List<ScheduledPost> _returningScheduledPosts = new List<ScheduledPost>()
        {
            new ScheduledPost()
            {
                UserId = specificGuid,
                AttachmentPath = "TestPath1",
                PostTime = DateTime.Now.AddDays(1),
                MediaChannels = "Channel1",
                PostId = Guid.NewGuid()
            },
            new ScheduledPost()
            {
                UserId = Guid.NewGuid(),
                PostTime = DateTime.Now.AddDays(3),
                PostText = "Test Post 1",
                MediaChannels = "Channel2",
                PostId = Guid.NewGuid()
            },
            new ScheduledPost()
            {
                UserId = specificGuid,
                PostTime = DateTime.Now.AddDays(4),
                PostText = "Test Post 2",
                MediaChannels = "Channel1,Channel2",
                PostId = Guid.NewGuid()
            },
            new ScheduledPost()
            {
                UserId = Guid.NewGuid(),
                AttachmentPath = "TestPath2",
                PostTime = DateTime.Now.AddDays(2),
                MediaChannels = "Channel1,Channel2",
                PostId = Guid.NewGuid()
            }
        };

        private SchedulerRepository _scheduleRepo;

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

            _scheduleRepo = new SchedulerRepository(_mockIDbConnection.Object);
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
            _scheduleRepo = null;
            count = -1;
            _returningScheduledPosts = null;
        }

        [TestMethod]
        public void ConstructorTest_CallToParameterlessConstructor_ReturnsTrueIfObjectIsNotNull()
        {
            //arrange

            //act
            _scheduleRepo = new SchedulerRepository();

            //assert
            Assert.IsNotNull(_scheduleRepo);
        }

        [TestMethod]
        public void CreatePost_GivenValuesAndOptionalAttachementNull_CorrectValuesSentToConnection()
        {
            //arrange
            ScheduledPost post = new ScheduledPost()
            {
                UserId = specificGuid,
                PostTime = DateTime.Now.AddDays(4),
                PostText = "Test Post 2",
                MediaChannels = "Channel1,Channel2",
                PostId = Guid.NewGuid()
            };

            //act
            _scheduleRepo.Create(post);

            //assert
            Assert.AreEqual(6, _parameterObjects.Count);
            foreach (var property in post.GetType().GetProperties())
            {
                if (property.GetValue(post) == null) continue;
                foreach (var parameter in _parameterObjects)
                {
                    var sqlParameter = (SqlParameter)parameter;

                    if (!sqlParameter.ParameterName.Contains(property.Name)) continue;
                    Assert.AreEqual("@" + property.Name, sqlParameter.ParameterName);
                    Assert.AreEqual(property.GetValue(post), sqlParameter.Value);
                }
            }

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Insert,
                    "[FDM90].[dbo].[ScheduledPosts]",
                    new string[] { "PostId", "UserId", "PostText", "AttachmentPath", "PostTime", "MediaChannels" },
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }

        [TestMethod]
        public void CreatePost_GivenValuesAndOptionalTextNull_CorrectValuesSentToConnection()
        {
            //arrange
            ScheduledPost post = new ScheduledPost()
            {
                UserId = specificGuid,
                PostTime = DateTime.Now.AddDays(4),
                AttachmentPath = "TestPath2",
                MediaChannels = "Channel1,Channel2",
                PostId = Guid.NewGuid()
            };

            //act
            _scheduleRepo.Create(post);

            //assert
            Assert.AreEqual(6, _parameterObjects.Count);
            foreach (var property in post.GetType().GetProperties())
            {
                if (property.GetValue(post) == null) continue;
                foreach (var parameter in _parameterObjects)
                {
                    var sqlParameter = (SqlParameter)parameter;

                    if (!sqlParameter.ParameterName.Contains(property.Name)) continue;
                    Assert.AreEqual("@" + property.Name, sqlParameter.ParameterName);
                    Assert.AreEqual(property.GetValue(post), sqlParameter.Value);
                }
            }

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Insert,
                    "[FDM90].[dbo].[ScheduledPosts]",
                    new string[] { "PostId", "UserId", "PostText", "AttachmentPath", "PostTime", "MediaChannels" },
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }

        [TestMethod]
        public void CreatePost_GivenValuesAndOptionalPopulated_CorrectValuesSentToConnection()
        {
            //arrange
            ScheduledPost post = new ScheduledPost()
            {
                UserId = specificGuid,
                PostTime = DateTime.Now.AddDays(4),
                PostText = "Test Post 2",
                AttachmentPath = "TestPath2",
                MediaChannels = "Channel1,Channel2",
                PostId = Guid.NewGuid()
            };

            //act
            _scheduleRepo.Create(post);

            //assert
            Assert.AreEqual(6, _parameterObjects.Count);
            foreach (var property in post.GetType().GetProperties())
            {
                if (property.GetValue(post) == null) continue;
                foreach (var parameter in _parameterObjects)
                {
                    var sqlParameter = (SqlParameter)parameter;

                    if (!sqlParameter.ParameterName.Contains(property.Name)) continue;
                    Assert.AreEqual("@" + property.Name, sqlParameter.ParameterName);
                    Assert.AreEqual(property.GetValue(post), sqlParameter.Value);
                }
            }

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Insert,
                    "[FDM90].[dbo].[ScheduledPosts]",
                    new string[] { "PostId", "UserId", "PostText", "AttachmentPath", "PostTime", "MediaChannels" },
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }


        [TestMethod]
        public void ReadMultipleSpecificPosts_GivenMethodCallWithUserId_CorrectValuesSentToConnection()
        {
            //arrange
            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < _returningScheduledPosts.Count(x => x.UserId == specificGuid) - 1).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(() => _returningScheduledPosts.Where(x => x.UserId == specificGuid).ToArray()[count].UserId);
            _mockIDataReader.Setup(reader => reader["AttachmentPath"]).Returns(() => _returningScheduledPosts.Where(x => x.UserId == specificGuid).ToArray()[count].AttachmentPath);
            _mockIDataReader.Setup(reader => reader["MediaChannels"]).Returns(() => _returningScheduledPosts.Where(x => x.UserId == specificGuid).ToArray()[count].MediaChannels);
            _mockIDataReader.Setup(reader => reader["PostTime"]).Returns(() => _returningScheduledPosts.Where(x => x.UserId == specificGuid).ToArray()[count].PostTime);
            _mockIDataReader.Setup(reader => reader["PostText"]).Returns(() => _returningScheduledPosts.Where(x => x.UserId == specificGuid).ToArray()[count].PostText);
            _mockIDataReader.Setup(reader => reader["PostId"]).Returns(() => _returningScheduledPosts.Where(x => x.UserId == specificGuid).ToArray()[count].PostId);

            //act
            var result = _scheduleRepo.ReadMultipleSpecific(specificGuid.ToString());

            //assert
            Assert.AreEqual(1, _parameterObjects.Count);

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Select,
                    "[FDM90].[dbo].[ScheduledPosts]",
                    new[] { "UserId" },
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }

        [TestMethod]
        public void ReadMultipleSpecificCampaigns_GivenMethodCallWithUserId_CorrectValueReturned()
        {
            //arrange
            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < _returningScheduledPosts.Count(x => x.UserId == specificGuid) - 1).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(() => _returningScheduledPosts.Where(x => x.UserId == specificGuid).ToArray()[count].UserId);
            _mockIDataReader.Setup(reader => reader["AttachmentPath"]).Returns(() => _returningScheduledPosts.Where(x => x.UserId == specificGuid).ToArray()[count].AttachmentPath);
            _mockIDataReader.Setup(reader => reader["MediaChannels"]).Returns(() => _returningScheduledPosts.Where(x => x.UserId == specificGuid).ToArray()[count].MediaChannels);
            _mockIDataReader.Setup(reader => reader["PostTime"]).Returns(() => _returningScheduledPosts.Where(x => x.UserId == specificGuid).ToArray()[count].PostTime);
            _mockIDataReader.Setup(reader => reader["PostText"]).Returns(() => _returningScheduledPosts.Where(x => x.UserId == specificGuid).ToArray()[count].PostText);
            _mockIDataReader.Setup(reader => reader["PostId"]).Returns(() => _returningScheduledPosts.Where(x => x.UserId == specificGuid).ToArray()[count].PostId);

            //act
            var result = _scheduleRepo.ReadMultipleSpecific(specificGuid.ToString()).ToList();

            //assert
            Assert.AreEqual(_returningScheduledPosts.Count(x => x.UserId == specificGuid), result.Count);

            for (int i = 0; i < _returningScheduledPosts.Count(x => x.UserId == specificGuid); i++)
            {
                Assert.AreEqual(_returningScheduledPosts.Where(x => x.UserId == specificGuid).ToArray()[i].UserId, result[i].UserId);
                Assert.AreEqual(_returningScheduledPosts.Where(x => x.UserId == specificGuid).ToArray()[i].AttachmentPath, result[i].AttachmentPath);
                Assert.AreEqual(_returningScheduledPosts.Where(x => x.UserId == specificGuid).ToArray()[i].MediaChannels, result[i].MediaChannels);
                Assert.AreEqual(_returningScheduledPosts.Where(x => x.UserId == specificGuid).ToArray()[i].PostTime.Date, result[i].PostTime.Date);
                Assert.AreEqual(_returningScheduledPosts.Where(x => x.UserId == specificGuid).ToArray()[i].PostText, result[i].PostText);
                Assert.AreEqual(_returningScheduledPosts.Where(x => x.UserId == specificGuid).ToArray()[i].PostId, result[i].PostId);
            }
        }

        [TestMethod]
        public void ReadMultipleSpecificPosts_GivenMethodCallWithDateTime_CorrectValuesSentToConnection()
        {
            //arrange
            Random rdm = new Random();
            DateTime specificTime = _returningScheduledPosts[(int)Math.Round(double.Parse(rdm.Next(0, _returningScheduledPosts.Count).ToString()))].PostTime;

            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < _returningScheduledPosts.Count(x => x.PostTime == specificTime) - 1).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(() => _returningScheduledPosts.Where(x => x.PostTime == specificTime).ToArray()[count].UserId);
            _mockIDataReader.Setup(reader => reader["AttachmentPath"]).Returns(() => _returningScheduledPosts.Where(x => x.PostTime == specificTime).ToArray()[count].AttachmentPath);
            _mockIDataReader.Setup(reader => reader["MediaChannels"]).Returns(() => _returningScheduledPosts.Where(x => x.PostTime == specificTime).ToArray()[count].MediaChannels);
            _mockIDataReader.Setup(reader => reader["PostTime"]).Returns(() => _returningScheduledPosts.Where(x => x.PostTime == specificTime).ToArray()[count].PostTime);
            _mockIDataReader.Setup(reader => reader["PostText"]).Returns(() => _returningScheduledPosts.Where(x => x.PostTime == specificTime).ToArray()[count].PostText);
            _mockIDataReader.Setup(reader => reader["PostId"]).Returns(() => _returningScheduledPosts.Where(x => x.PostTime == specificTime).ToArray()[count].PostId);

            //act
            var result = _scheduleRepo.ReadMultipleSpecific(specificTime.ToString());

            //assert
            Assert.AreEqual(1, _parameterObjects.Count);

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Select,
                    "[FDM90].[dbo].[ScheduledPosts]",
                    new[] { "PostTime" },
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }

        [TestMethod]
        public void ReadMultipleSpecificCampaigns_GivenMethodCallWithDateTime_CorrectValueReturned()
        {
            //arrange
            Random rdm = new Random();
            DateTime specificTime = _returningScheduledPosts[(int)Math.Round(double.Parse(rdm.Next(0, _returningScheduledPosts.Count).ToString()))].PostTime;

            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < _returningScheduledPosts.Count(x => x.PostTime == specificTime) - 1).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(() => _returningScheduledPosts.Where(x => x.PostTime == specificTime).ToArray()[count].UserId);
            _mockIDataReader.Setup(reader => reader["AttachmentPath"]).Returns(() => _returningScheduledPosts.Where(x => x.PostTime == specificTime).ToArray()[count].AttachmentPath);
            _mockIDataReader.Setup(reader => reader["MediaChannels"]).Returns(() => _returningScheduledPosts.Where(x => x.PostTime == specificTime).ToArray()[count].MediaChannels);
            _mockIDataReader.Setup(reader => reader["PostTime"]).Returns(() => _returningScheduledPosts.Where(x => x.PostTime == specificTime).ToArray()[count].PostTime);
            _mockIDataReader.Setup(reader => reader["PostText"]).Returns(() => _returningScheduledPosts.Where(x => x.PostTime == specificTime).ToArray()[count].PostText);
            _mockIDataReader.Setup(reader => reader["PostId"]).Returns(() => _returningScheduledPosts.Where(x => x.PostTime == specificTime).ToArray()[count].PostId);

            //act
            var result = _scheduleRepo.ReadMultipleSpecific(specificTime.ToString()).ToList();

            //assert
            Assert.AreEqual(_returningScheduledPosts.Count(x => x.PostTime == specificTime), result.Count);

            for (int i = 0; i < _returningScheduledPosts.Count(x => x.PostTime == specificTime); i++)
            {
                Assert.AreEqual(_returningScheduledPosts.Where(x => x.PostTime == specificTime).ToArray()[i].UserId, result[i].UserId);
                Assert.AreEqual(_returningScheduledPosts.Where(x => x.PostTime == specificTime).ToArray()[i].AttachmentPath, result[i].AttachmentPath);
                Assert.AreEqual(_returningScheduledPosts.Where(x => x.PostTime == specificTime).ToArray()[i].MediaChannels, result[i].MediaChannels);
                Assert.AreEqual(_returningScheduledPosts.Where(x => x.PostTime == specificTime).ToArray()[i].PostTime.Date, result[i].PostTime.Date);
                Assert.AreEqual(_returningScheduledPosts.Where(x => x.PostTime == specificTime).ToArray()[i].PostText, result[i].PostText);
                Assert.AreEqual(_returningScheduledPosts.Where(x => x.PostTime == specificTime).ToArray()[i].PostId, result[i].PostId);
            }
        }

        [TestMethod]
        public void UpdateCampaigns_GivenUpdatedCreds_CorrectValuesSentToConnection()
        {
            //arrange
            Random rdm = new Random();
            ScheduledPost specificPost = _returningScheduledPosts[(int)Math.Round(double.Parse(rdm.Next(0, _returningScheduledPosts.Count).ToString()))];
            ScheduledPost updatedPost = new ScheduledPost() { PostId = specificPost.PostId };

            _mockIDataReader.Setup(reader => reader.Read()).Returns(() => count < 0).Callback(() => count++);
            _mockIDataReader.Setup(reader => reader["UserId"]).Returns(specificPost.UserId);
            _mockIDataReader.Setup(reader => reader["AttachmentPath"]).Returns(specificPost.AttachmentPath);
            _mockIDataReader.Setup(reader => reader["MediaChannels"]).Returns(specificPost.MediaChannels);
            _mockIDataReader.Setup(reader => reader["PostTime"]).Returns(specificPost.PostTime);
            _mockIDataReader.Setup(reader => reader["PostText"]).Returns(specificPost.PostText);
            _mockIDataReader.Setup(reader => reader["PostId"]).Returns(specificPost.PostId);

            updatedPost.PostText = "UPDATED POST TEXT";

            //act
            _scheduleRepo.Update(updatedPost);

            //assert
            _parameterObjects.RemoveAt(0);
            Assert.AreEqual(2, _parameterObjects.Count);

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Update,
                    "[FDM90].[dbo].[ScheduledPosts]",
                    new[] { "PostText", "PostId" },
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString, 1));
        }

        [TestMethod]
        public void DeleteCampaigns_GivenCredsId_CorrectValuesSentToConnection()
        {
            //arrange
            ScheduledPost deletePost = new ScheduledPost() { PostId = Guid.NewGuid() };

            //act
            _scheduleRepo.Delete(deletePost);

            //assert
            Assert.AreEqual(1, _parameterObjects.Count);

            Assert.IsTrue(
                TestHelper.CheckSqlStatementString(
                    StatementType.Delete,
                    "[FDM90].[dbo].[ScheduledPosts]",
                    new[] { "PostId" },
                    _parameterObjects.Cast<SqlParameter>().Select(x => x.ParameterName).ToArray(), setSqlString));
        }
    }
}
