using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FDM90.Handlers;
using Moq;
using FDM90.Repository;
using FDM90.Models;
using System.Collections.Generic;
using System.Linq;

namespace FDM90UnitTests
{
    [TestClass]
    public class SchedulerHandlerTests
    {
        private Mock<IRepository<ScheduledPost>> _schedulerRepoMock;
        private Mock<IFacebookHandler> _facebookHandlerMock;
        private Mock<ITwitterHandler> _twitterHandlerMock;
        private Mock<IUserHandler> _userHandlerMock;
        private SchedulerHandler _schedulerHandler;
        private ScheduledPost _pastSchedulerPost;
        private static Guid _specificGuid;
        private Guid _pastGuid;
        private string _pastValue;
        private List<ScheduledPost> _returningScheduledPosts;
        private Dictionary<string, Dictionary<string, string>> _pastPostParameters;

        [TestInitialize]
        public void StartUp()
        {
            _specificGuid = Guid.NewGuid();
            _pastPostParameters = new Dictionary<string, Dictionary<string, string>>();
            _returningScheduledPosts = new List<ScheduledPost>()
            {
                new ScheduledPost()
                {
                    PostId = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    PostText = "Test Post Text1",
                    AttachmentPath = "TestPath1",
                    MediaChannels = "Facebook,Twitter",
                    PostTime = DateTime.Now.AddDays(-2)
                },
                new ScheduledPost()
                {
                    PostId = Guid.NewGuid(),
                    UserId = _specificGuid,
                    AttachmentPath = "TestPath2",
                    MediaChannels = "Facebook",
                    PostTime = DateTime.Now.AddDays(-3)
                },
                new ScheduledPost()
                {
                    PostId = Guid.NewGuid(),
                    UserId = _specificGuid,
                    PostText = "Test Post Text3",
                    MediaChannels = "Facebook,Twitter",
                    PostTime = DateTime.Now
                },
                new ScheduledPost()
                {
                    PostId = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    PostText = "Test Post Text4",
                    AttachmentPath = "TestPath2",
                    MediaChannels = "Twitter",
                    PostTime = DateTime.Now
                },
                new ScheduledPost()
                {
                    PostId = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    PostText = "Test Post Text5",
                    AttachmentPath = "TestPath3",
                    MediaChannels = "AnotherChannel",
                    PostTime = DateTime.Now
                }
            };

            _schedulerRepoMock = new Mock<IRepository<ScheduledPost>>();
            _schedulerRepoMock.As<IReadMultipleSpecific<ScheduledPost>>()
                .Setup(x => x.ReadMultipleSpecific(It.IsAny<string>())).Callback((string pastValue) =>
                {
                    Guid testGuid = Guid.Empty;

                    if (Guid.TryParse(pastValue, out testGuid))
                    {
                        _pastGuid = testGuid;
                    }
                    else
                    {
                        _pastValue = pastValue;
                    }
                }).Returns(() =>
                {
                    if (_pastGuid != Guid.Empty)
                    {
                        return _returningScheduledPosts.Where(x => x.UserId == _pastGuid);
                    }
                    else
                    {
                        return _returningScheduledPosts.Where(x => x.PostTime.Date == DateTime.Parse(_pastValue));
                    }
                }).Verifiable();
            _schedulerRepoMock.Setup(x => x.Create(It.IsAny<ScheduledPost>())).Callback((ScheduledPost post) => _pastSchedulerPost = post).Verifiable();
            _schedulerRepoMock.Setup(x => x.Update(It.IsAny<ScheduledPost>())).Callback((ScheduledPost post) => _pastSchedulerPost = post).Verifiable();
            _schedulerRepoMock.Setup(x => x.Delete(It.IsAny<ScheduledPost>())).Callback((ScheduledPost post) => _pastSchedulerPost = post).Verifiable();

            _facebookHandlerMock = new Mock<IFacebookHandler>();
            _facebookHandlerMock.As<IMediaHandler>();
            _facebookHandlerMock.Setup(p => p.MediaName).Returns("Facebook");
            _facebookHandlerMock.As<IMediaHandler>().Setup(p => p.PostData(It.IsAny<Dictionary<string, string>>(), It.IsAny<Guid>()))
                .Callback((Dictionary<string, string> pastParams, Guid userId) => _pastPostParameters.Add("Facebook_" + userId, pastParams)).Verifiable();

            _twitterHandlerMock = new Mock<ITwitterHandler>();
            _twitterHandlerMock.As<IMediaHandler>();
            _twitterHandlerMock.Setup(p => p.MediaName).Returns("Twitter");
            _twitterHandlerMock.As<IMediaHandler>().Setup(p => p.PostData(It.IsAny<Dictionary<string, string>>(), It.IsAny<Guid>()))
                .Callback((Dictionary<string, string> pastParams, Guid userId) => _pastPostParameters.Add("Twitter_" + userId, pastParams)).Verifiable();

            _userHandlerMock = new Mock<IUserHandler>();

            _schedulerHandler = new SchedulerHandler(_schedulerRepoMock.Object, _facebookHandlerMock.Object, _twitterHandlerMock.Object, _userHandlerMock.Object);
        }

        [TestCleanup]
        public void CleanUp()
        {
            _specificGuid = Guid.Empty;
            _pastGuid = Guid.Empty;
            _pastPostParameters = null;
            _returningScheduledPosts = null;
            _pastSchedulerPost = null;
            _schedulerRepoMock = null;
            _facebookHandlerMock = null;
            _twitterHandlerMock = null;
            _userHandlerMock = null;
            _schedulerHandler = null;
        }

        [TestMethod]
        public void ConstructorTest_CallToParameterlessConstructor_ReturnsTrueIfObjectIsNotNull()
        {
            //arrange

            //act
            _schedulerHandler = new SchedulerHandler();

            //assert
            Assert.IsNotNull(_schedulerHandler);
        }

        [TestMethod]
        public void CreateScheduledPost_GivenParameters_ReturnsTrueIfObjectPassed()
        {
            //arrange
            ScheduledPost newPost = new ScheduledPost()
            {
                UserId = Guid.NewGuid(),
                PostText = "Test Post Text",
                AttachmentPath = "TestPath",
                MediaChannels = "MediaChannel1,MedialChannel2",
                PostTime = DateTime.Now
            };

            //act
            _schedulerHandler.CreateScheduledPost(newPost);

            //assert
            Assert.AreNotEqual(Guid.Empty, _pastSchedulerPost.PostId);
            Assert.AreEqual(newPost.PostText, _pastSchedulerPost.PostText);
            Assert.AreEqual(newPost.MediaChannels, _pastSchedulerPost.MediaChannels);
            Assert.AreEqual(newPost.PostTime, _pastSchedulerPost.PostTime);
            Assert.AreEqual(newPost.AttachmentPath, _pastSchedulerPost.AttachmentPath);
            Assert.AreEqual(newPost.UserId, _pastSchedulerPost.UserId);
        }

        [TestMethod]
        public void CreateScheduledPost_GivenParameters_ReturnsTrueCreateMethodCalled()
        {
            //arrange
            ScheduledPost newPost = new ScheduledPost()
            {
                UserId = Guid.NewGuid(),
                PostText = "Test Post Text",
                AttachmentPath = "TestPath",
                MediaChannels = "MediaChannel1,MedialChannel2",
                PostTime = DateTime.Now
            };

            //act
            _schedulerHandler.CreateScheduledPost(newPost);

            //assert
            _schedulerRepoMock.Verify(x => x.Create(It.IsAny<ScheduledPost>()), Times.Once);
            _schedulerRepoMock.Verify(x => x.Update(It.IsAny<ScheduledPost>()), Times.Never);
            _schedulerRepoMock.Verify(x => x.Delete(It.IsAny<ScheduledPost>()), Times.Never);
            _schedulerRepoMock.As<IReadMultipleSpecific<ScheduledPost>>().Verify(x => x.ReadMultipleSpecific(It.IsAny<string>()), Times.Never);
            _facebookHandlerMock.As<IMediaHandler>().Verify(p => p.PostData(It.IsAny<Dictionary<string, string>>(), It.IsAny<Guid>()), Times.Never);
            _twitterHandlerMock.As<IMediaHandler>().Verify(p => p.PostData(It.IsAny<Dictionary<string, string>>(), It.IsAny<Guid>()), Times.Never);
        }

        [TestMethod]
        public void UpdateScheduledPost_GivenParameters_ReturnsTrueIfObjectPassed()
        {
            //arrange
            ScheduledPost updatePost = new ScheduledPost()
            {
                PostId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                PostText = "Test Post Text Updated",
                AttachmentPath = "TestPath",
                MediaChannels = "MediaChannel1,MedialChannel2",
                PostTime = DateTime.Now
            };

            //act
            _schedulerHandler.UpdateScheduledPost(updatePost);

            //assert
            Assert.AreEqual(updatePost.PostId, _pastSchedulerPost.PostId);
            Assert.AreEqual(updatePost.PostText, _pastSchedulerPost.PostText);
            Assert.AreEqual(updatePost.MediaChannels, _pastSchedulerPost.MediaChannels);
            Assert.AreEqual(updatePost.PostTime, _pastSchedulerPost.PostTime);
            Assert.AreEqual(updatePost.AttachmentPath, _pastSchedulerPost.AttachmentPath);
            Assert.AreEqual(updatePost.UserId, _pastSchedulerPost.UserId);
        }

        [TestMethod]
        public void UpdateScheduledPost_GivenParameters_ReturnsTrueCreateMethodCalled()
        {
            //arrange
            ScheduledPost newPost = new ScheduledPost()
            {
                UserId = Guid.NewGuid(),
                PostText = "Test Post Text Updated",
                AttachmentPath = "TestPath",
                MediaChannels = "MediaChannel1,MedialChannel2",
                PostTime = DateTime.Now
            };

            //act
            _schedulerHandler.UpdateScheduledPost(newPost);

            //assert
            _schedulerRepoMock.Verify(x => x.Create(It.IsAny<ScheduledPost>()), Times.Never);
            _schedulerRepoMock.Verify(x => x.Update(It.IsAny<ScheduledPost>()), Times.Once);
            _schedulerRepoMock.Verify(x => x.Delete(It.IsAny<ScheduledPost>()), Times.Never);
            _schedulerRepoMock.As<IReadMultipleSpecific<ScheduledPost>>().Verify(x => x.ReadMultipleSpecific(It.IsAny<string>()), Times.Never);
            _facebookHandlerMock.As<IMediaHandler>().Verify(p => p.PostData(It.IsAny<Dictionary<string, string>>(), It.IsAny<Guid>()), Times.Never);
            _twitterHandlerMock.As<IMediaHandler>().Verify(p => p.PostData(It.IsAny<Dictionary<string, string>>(), It.IsAny<Guid>()), Times.Never);
        }

        [TestMethod]
        public void GetSchedulerPostsForUser_GivenParameters_ReturnsTrueIfObjectPassed()
        {
            //arrange

            //act
            var result = _schedulerHandler.GetSchedulerPostsForUser(_specificGuid).ToList();

            //assert
            Assert.AreEqual(_returningScheduledPosts.Count(x => x.UserId == _specificGuid), result.Count);

            for (int i = 0; i < result.Count; i++)
            {
                Assert.AreEqual(_returningScheduledPosts.Where(x => x.UserId == _specificGuid).ToList()[i].PostId, result[i].PostId);
                Assert.AreEqual(_returningScheduledPosts.Where(x => x.UserId == _specificGuid).ToList()[i].PostText, result[i].PostText);
                Assert.AreEqual(_returningScheduledPosts.Where(x => x.UserId == _specificGuid).ToList()[i].MediaChannels, result[i].MediaChannels);
                Assert.AreEqual(_returningScheduledPosts.Where(x => x.UserId == _specificGuid).ToList()[i].PostTime, result[i].PostTime);
                Assert.AreEqual(_returningScheduledPosts.Where(x => x.UserId == _specificGuid).ToList()[i].AttachmentPath, result[i].AttachmentPath);
                Assert.AreEqual(_returningScheduledPosts.Where(x => x.UserId == _specificGuid).ToList()[i].UserId, result[i].UserId);
            }
        }

        [TestMethod]
        public void GetSchedulerPostsForUser_GivenParameters_ReturnsTrueCreateMethodCalled()
        {
            //arrange

            //act
            var result = _schedulerHandler.GetSchedulerPostsForUser(_specificGuid).ToList();

            //assert
            _schedulerRepoMock.Verify(x => x.Create(It.IsAny<ScheduledPost>()), Times.Never);
            _schedulerRepoMock.Verify(x => x.Update(It.IsAny<ScheduledPost>()), Times.Never);
            _schedulerRepoMock.Verify(x => x.Delete(It.IsAny<ScheduledPost>()), Times.Never);
            _schedulerRepoMock.As<IReadMultipleSpecific<ScheduledPost>>().Verify(x => x.ReadMultipleSpecific(_specificGuid.ToString()), Times.Once);
            _facebookHandlerMock.As<IMediaHandler>().Verify(p => p.PostData(It.IsAny<Dictionary<string, string>>(), It.IsAny<Guid>()), Times.Never);
            _twitterHandlerMock.As<IMediaHandler>().Verify(p => p.PostData(It.IsAny<Dictionary<string, string>>(), It.IsAny<Guid>()), Times.Never);
        }

        [TestMethod]
        public void SchedulerPostsForTime_GivenMultiplePostsForMediaChannels_ReturnsTrueIfHandlerPostsCalled()
        {
            // arrange
            User testSpecificUser = new User()
            {
                UserId = _specificGuid,
                Facebook = true,
                Twitter = true
            };

            _userHandlerMock.Setup(s => s.GetUser(It.Is<string>(x => x == testSpecificUser.UserId.ToString()))).Returns(testSpecificUser).Verifiable();
            _userHandlerMock.Setup(s => s.GetUser(It.Is<string>(x => x != testSpecificUser.UserId.ToString()))).Returns(new User()
            {
                UserId = Guid.NewGuid(),
                Facebook = false,
                Twitter = true
            }).Verifiable();

            // act
            _schedulerHandler.SchedulerPostsForTime(DateTime.Now.Date);

            // assert
            _userHandlerMock.Verify(x => x.GetUser(It.IsAny<string>()), Times.Exactly(3));
            _schedulerRepoMock.As<IReadMultipleSpecific<ScheduledPost>>().Verify(p => p.ReadMultipleSpecific(It.IsAny<string>()), Times.Once);
            _schedulerRepoMock.Verify(x => x.Create(It.IsAny<ScheduledPost>()), Times.Never);
            _schedulerRepoMock.Verify(x => x.Update(It.IsAny<ScheduledPost>()), Times.Never);
            _schedulerRepoMock.Verify(x => x.Delete(It.IsAny<ScheduledPost>()), Times.Exactly(3));
            _facebookHandlerMock.As<IMediaHandler>().Verify(p => p.PostData(It.IsAny<Dictionary<string, string>>(), It.IsAny<Guid>()), Times.Once);
            _twitterHandlerMock.As<IMediaHandler>().Verify(p => p.PostData(It.IsAny<Dictionary<string, string>>(), It.IsAny<Guid>()), Times.Exactly(2));
        }

        [TestMethod]
        public void SchedulerPostsForTime_GivenMultiplePostsForMediaChannels_ReturnsTrueCorrectParametersPassedCorrectly()
        {
            // arrange
            User testSpecificUser = new User()
            {
                UserId = _specificGuid,
                Facebook = true,
                Twitter = true
            };

            _userHandlerMock.Setup(s => s.GetUser(It.Is<string>(x => x == testSpecificUser.UserId.ToString()))).Returns(testSpecificUser).Verifiable();
            _userHandlerMock.Setup(s => s.GetUser(It.Is<string>(x => x != testSpecificUser.UserId.ToString()))).Returns(new User()
            {
                UserId = Guid.NewGuid(),
                Facebook = false,
                Twitter = true
            }).Verifiable();

            // act
            _schedulerHandler.SchedulerPostsForTime(DateTime.Now.Date);

            // assert
            foreach (var value in _pastPostParameters)
            {
                string userId = value.Key.Split('_')[1];

                foreach (var userValue in _returningScheduledPosts.Where(x => x.PostTime.Date == DateTime.Now.Date && x.UserId == Guid.Parse(userId)))
                {
                    if (!string.IsNullOrWhiteSpace(userValue.PostText))
                    {
                        Assert.IsTrue(value.Value.ContainsKey("message"));
                        Assert.AreEqual(userValue.PostText, value.Value["message"]);
                    }
                    if (!string.IsNullOrWhiteSpace(userValue.AttachmentPath))
                    {
                        Assert.IsTrue(value.Value.ContainsKey("picture"));
                        Assert.AreEqual(userValue.AttachmentPath, value.Value["picture"]);
                    }

                    Assert.IsTrue(userValue.MediaChannels.Split(',').Any(x => value.Key.Contains(x)));
                }
            }
        }
    }
}
