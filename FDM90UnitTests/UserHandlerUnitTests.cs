﻿using System;
using FDM90.Handlers;
using FDM90.Models;
using FDM90.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FDM90UnitTests
{
    [TestClass]
    public class UserHandlerUnitTests
    {
        private Mock<IRepository<User>> _mockUserRepo;
        private UserHandler _userHandler;
        private User createUser;
        private User updatedUser;

        [TestInitialize]
        public void StartUp()
        {
            _mockUserRepo = new Mock<IRepository<User>>();
            _mockUserRepo.Setup(repository => repository.Create(It.IsAny<User>())).Callback<User>(user => createUser = user).Verifiable();
            _mockUserRepo.Setup(repository => repository.Update(It.IsAny<User>())).Callback<User>(user => updatedUser = user).Verifiable();
            _mockUserRepo.Setup(repository => repository.Delete(It.IsAny<User>())).Verifiable();

            _mockUserRepo.As<IReadSpecific<User>>();
            _mockUserRepo.As<IReadSpecific<User>>().Setup(specific => specific.ReadSpecific(It.IsAny<User>())).Verifiable();

            _userHandler = new UserHandler(_mockUserRepo.Object);
        }

        [TestCleanup]
        public void CleanUp()
        {
            _mockUserRepo = null;
            _userHandler = null;
            createUser = null;
            updatedUser = null;
        }

        [TestMethod]
        public void ConstructorTest_CallToParameterlessConstructor_ReturnsTrueIfObjectIsNotNull()
        {
            //arrange

            //act
            _userHandler = new UserHandler();

            //assert
            Assert.IsNotNull(_userHandler);
        }

        [TestMethod]
        public void RegisterUser_GivenUserDetails_ReturnsTrueIfObjectUserIsReturned()
        {
            //arrange

            //act
            var result = _userHandler.RegisterUser("TestUserName", "test@email.com", "VkdWemRGQmhjM04zYjNKaw==");

            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(User));
            Assert.AreNotEqual(Guid.Empty, result.UserId);
            Assert.IsInstanceOfType(result.UserId, typeof(Guid));
            Assert.AreEqual("TestUserName", result.UserName);
            Assert.AreEqual("test@email.com", result.EmailAddress);
            Assert.AreNotEqual("VkdWemRGQmhjM04zYjNKaw==", result.Password);
            Assert.IsFalse(result.Facebook);
        }

        [TestMethod]
        public void RegisterUser_GivenUserDetails_ReturnsTrueIfRepoCeateWasCalledOnce()
        {
            //arrange

            //act
            var result = _userHandler.RegisterUser("TestUserName", "test@email.com", "VkdWemRGQmhjM04zYjNKaw==");

            //assert
            _mockUserRepo.Verify(repository => repository.Create(It.IsAny<User>()), Times.Once);
            _mockUserRepo.Verify(repository => repository.Update(It.IsAny<User>()), Times.Never);
            _mockUserRepo.Verify(repository => repository.Delete(It.IsAny<User>()), Times.Never);
            _mockUserRepo.As<IReadSpecific<User>>().Verify(specific => specific.ReadSpecific(It.IsAny<User>()), Times.Never);
        }

        [TestMethod]
        public void RegisterUser_GivenUserDetails_ReturnsTrueIfRepoWasCalledWithCorrectValues()
        {
            //arrange

            //act
            var result = _userHandler.RegisterUser("TestUserName", "test@email.com", "VkdWemRGQmhjM04zYjNKaw==");

            //assert
            Assert.IsNotNull(createUser);
            Assert.IsInstanceOfType(createUser, typeof(User));
            Assert.AreNotEqual(Guid.Empty, createUser.UserId);
            Assert.AreEqual(result.UserId, createUser.UserId);
            Assert.AreEqual(result.UserName, createUser.UserName);
            Assert.AreEqual(result.EmailAddress, createUser.EmailAddress);
            Assert.AreEqual(result.Password, createUser.Password);
            Assert.AreEqual(result.Facebook, createUser.Facebook);
        }

        [TestMethod]
        public void LoginUser_GivenIncorrectUserName_ReturnsTrueIfUserNameDoesntExist()
        {
            //arrange
            _mockUserRepo.As<IReadSpecific<User>>()
                .Setup(specific => specific.ReadSpecific(It.IsAny<User>())).Returns(() => null);
            User specificUser = new User("TestUserName", "VkdWemRGQmhjM04zYjNKaw==");

            //act
            var result = _userHandler.LoginUser(specificUser);

            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(User));
            Assert.AreEqual(Guid.Empty, result.UserId);
            Assert.AreEqual("User doesn't exist", result.UserName);
            Assert.IsNull(result.EmailAddress);
            Assert.AreNotEqual("VkdWemRGQmhjM04zYjNKaw==", result.Password);
            Assert.IsFalse(result.Facebook);
        }

        [TestMethod]
        public void LoginUser_GivenIncorrectPassword_ReturnsTrueIfUserNamePasswordIncorrect()
        {
            //arrange
            User returningUser = new User("TestUserName", "VkdWemRGQmhjM04zYjNKaw==") {UserId = Guid.NewGuid()};
            _mockUserRepo.As<IReadSpecific<User>>()
                .Setup(specific => specific.ReadSpecific(It.IsAny<User>())).Returns(() => returningUser);
            User specificUser = new User("TestUserName", "VkdWemRGQmhjM04zYjNKaw==");

            //act
            var result = _userHandler.LoginUser(specificUser);

            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(User));
            Assert.AreEqual(Guid.Empty, result.UserId);
            Assert.AreEqual("Password is incorrect", result.UserName);
            Assert.IsNull(result.EmailAddress);
            Assert.AreNotEqual("VkdWemRGQmhjM04zYjNKaw==", result.Password);
            Assert.IsFalse(result.Facebook);
        }

        [TestMethod]
        public void LoginUser_GivenCorrectCredential_ReturnsTrueIfUserReturned()
        {
            //arrange
            User returningUser = new User("TestUserName", "VkdWemRGQmhjM04zYjNKaw==") { UserId = Guid.NewGuid() };
            _mockUserRepo.As<IReadSpecific<User>>()
                .Setup(specific => specific.ReadSpecific(It.IsAny<User>())).Returns(() => returningUser);
            User specificUser = new User("TestUserName", "TestPassword");

            //act
            var result = _userHandler.LoginUser(specificUser);

            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(User));
            Assert.AreNotEqual(Guid.Empty, result.UserId);
            Assert.AreEqual("TestUserName", result.UserName);
            Assert.IsNull(result.EmailAddress);
            Assert.AreNotEqual("TestPassword", result.Password);
            Assert.IsFalse(result.Facebook);
        }

        [TestMethod]
        public void LoginUser_GivenCorrectCredential_ReturnsTrueIfReadSpecificCalledOnce()
        {
            //arrange
            User returningUser = new User("TestUserName", "VGVzdFBhc3N3b3Jk") { UserId = Guid.NewGuid() };
            _mockUserRepo.As<IReadSpecific<User>>()
                .Setup(specific => specific.ReadSpecific(It.IsAny<User>())).Returns(() => returningUser);
            User specificUser = new User("TestUserName", "VkdWemRGQmhjM04zYjNKaw==");

            //act
            var result = _userHandler.LoginUser(specificUser);

            //assert
            _mockUserRepo.Verify(repository => repository.Create(It.IsAny<User>()), Times.Never);
            _mockUserRepo.Verify(repository => repository.Update(It.IsAny<User>()), Times.Never);
            _mockUserRepo.Verify(repository => repository.Delete(It.IsAny<User>()), Times.Never);
            _mockUserRepo.As<IReadSpecific<User>>().Verify(specific => specific.ReadSpecific(It.IsAny<User>()), Times.Once);
        }

        [TestMethod]
        public void UpdateUserMediaActivation_GivenIncorrectSocialMedia_ReturnsTrueIfUserNotUpdated()
        {
            //arrange
            User returningUser = new User("TestUserName", "VGVzdFBhc3N3b3Jk") { UserId = Guid.NewGuid(), Facebook = false, Twitter = true };
            _mockUserRepo.As<IReadSpecific<User>>()
                .Setup(specific => specific.ReadSpecific(It.IsAny<User>())).Returns(() => returningUser);
            User specificUser = new User() { UserId = returningUser.UserId };

            //act
            var result = _userHandler.UpdateUserMediaActivation(specificUser, "TestSocialMedia", true);

            //assert
            Assert.AreEqual(returningUser.UserId, result.UserId);
            Assert.AreEqual(returningUser.Password, result.Password);
            Assert.AreEqual(returningUser.UserName, result.UserName);
            Assert.AreEqual(returningUser.EmailAddress, result.EmailAddress);
            Assert.AreEqual(returningUser.Twitter, result.Twitter);
            Assert.AreEqual(returningUser.Facebook, result.Facebook);

            _mockUserRepo.Verify(repository => repository.Create(It.IsAny<User>()), Times.Never);
            _mockUserRepo.Verify(repository => repository.Update(It.IsAny<User>()), Times.Never);
            _mockUserRepo.Verify(repository => repository.Delete(It.IsAny<User>()), Times.Never);
            _mockUserRepo.As<IReadSpecific<User>>().Verify(specific => specific.ReadSpecific(It.IsAny<User>()), Times.Once);
        }

        [TestMethod]
        public void UpdateUserMediaActivation_GivenCorrectCredAndTrue_ReturnsTrueIfUserUpdated()
        {
            //arrange
            User returningUser = new User("TestUserName", "VGVzdFBhc3N3b3Jk") { UserId = Guid.NewGuid(), Facebook = false, Twitter = true };
            _mockUserRepo.As<IReadSpecific<User>>()
                .Setup(specific => specific.ReadSpecific(It.IsAny<User>())).Returns(() => returningUser);
            User specificUser = new User() { UserId = returningUser.UserId };

            //act
            var result = _userHandler.UpdateUserMediaActivation(specificUser, "Facebook", true);

            //assert
            Assert.AreEqual(returningUser.UserId, result.UserId);
            Assert.AreEqual(returningUser.Password, result.Password);
            Assert.AreEqual(returningUser.UserName, result.UserName);
            Assert.AreEqual(returningUser.EmailAddress, result.EmailAddress);
            Assert.AreEqual(returningUser.Twitter, result.Twitter);
            Assert.AreEqual(returningUser.Facebook, result.Facebook);
            Assert.IsTrue(result.Facebook);
        }

        [TestMethod]
        public void UpdateUserMediaActivation_GivenCorrectCredAndTrue_ReturnsTrueIfUpdateCalledCorrectly()
        {
            //arrange
            User returningUser = new User("TestUserName", "VGVzdFBhc3N3b3Jk") { UserId = Guid.NewGuid(), Facebook = false, Twitter = true };
            _mockUserRepo.As<IReadSpecific<User>>()
                .Setup(specific => specific.ReadSpecific(It.IsAny<User>())).Returns(() => returningUser);
            User specificUser = new User() { UserId = returningUser.UserId };

            //act
            var result = _userHandler.UpdateUserMediaActivation(specificUser, "Facebook", true);

            //assert
            _mockUserRepo.Verify(repository => repository.Create(It.IsAny<User>()), Times.Never);
            _mockUserRepo.Verify(repository => repository.Update(It.IsAny<User>()), Times.Once);
            _mockUserRepo.Verify(repository => repository.Delete(It.IsAny<User>()), Times.Never);
            _mockUserRepo.As<IReadSpecific<User>>().Verify(specific => specific.ReadSpecific(It.IsAny<User>()), Times.Once);
        }

        [TestMethod]
        public void UpdateUserMediaActivation_GivenCorrectCredAndFalse_ReturnsTrueIfUserUpdated()
        {
            //arrange
            User returningUser = new User("TestUserName", "VGVzdFBhc3N3b3Jk") { UserId = Guid.NewGuid(), Facebook = true, Twitter = true };
            _mockUserRepo.As<IReadSpecific<User>>()
                .Setup(specific => specific.ReadSpecific(It.IsAny<User>())).Returns(() => returningUser);
            User specificUser = new User() { UserId = returningUser.UserId };

            //act
            var result = _userHandler.UpdateUserMediaActivation(specificUser, "Facebook", false);

            //assert
            Assert.AreEqual(returningUser.UserId, result.UserId);
            Assert.AreEqual(returningUser.Password, result.Password);
            Assert.AreEqual(returningUser.UserName, result.UserName);
            Assert.AreEqual(returningUser.EmailAddress, result.EmailAddress);
            Assert.AreEqual(returningUser.Twitter, result.Twitter);
            Assert.AreEqual(returningUser.Facebook, result.Facebook);
            Assert.IsFalse(result.Facebook);
        }

        [TestMethod]
        public void UpdateUserMediaActivation_GivenCorrectCredAndFalse_ReturnsTrueIfUpdateCalledCorrectly()
        {
            //arrange
            User returningUser = new User("TestUserName", "VGVzdFBhc3N3b3Jk") { UserId = Guid.NewGuid(), Facebook = true, Twitter = true };
            _mockUserRepo.As<IReadSpecific<User>>()
                .Setup(specific => specific.ReadSpecific(It.IsAny<User>())).Returns(() => returningUser);
            User specificUser = new User() { UserId = returningUser.UserId };

            //act
            var result = _userHandler.UpdateUserMediaActivation(specificUser, "Facebook", false);

            //assert
            _mockUserRepo.Verify(repository => repository.Create(It.IsAny<User>()), Times.Never);
            _mockUserRepo.Verify(repository => repository.Update(It.IsAny<User>()), Times.Once);
            _mockUserRepo.Verify(repository => repository.Delete(It.IsAny<User>()), Times.Never);
            _mockUserRepo.As<IReadSpecific<User>>().Verify(specific => specific.ReadSpecific(It.IsAny<User>()), Times.Once);
        }

        [TestMethod]
        public void GetUser_GivenParameter_ReturnsTrueIfUserReturned()
        {
            //arrange
            User returningUser = new User("TestUserName", "VGVzdFBhc3N3b3Jk") { UserId = Guid.NewGuid(), Facebook = false, Twitter = true };
            _mockUserRepo.As<IReadSpecific<User>>()
                .Setup(specific => specific.ReadSpecific(It.IsAny<User>())).Returns(() => returningUser);

            //act
            var result = _userHandler.GetUser(returningUser.UserId.ToString());

            //assert
            Assert.AreEqual(returningUser.UserId, result.UserId);
            Assert.AreEqual(returningUser.Password, result.Password);
            Assert.AreEqual(returningUser.UserName, result.UserName);
            Assert.AreEqual(returningUser.EmailAddress, result.EmailAddress);
            Assert.AreEqual(returningUser.Twitter, result.Twitter);
            Assert.AreEqual(returningUser.Facebook, result.Facebook);
        }

        [TestMethod]
        public void GetUser_GivenParameter_ReturnsTrueIfReadSpecificCalledCorrectly()
        {
            //arrange
            User returningUser = new User("TestUserName", "VGVzdFBhc3N3b3Jk") { UserId = Guid.NewGuid(), Facebook = false, Twitter = true };
            _mockUserRepo.As<IReadSpecific<User>>()
                .Setup(specific => specific.ReadSpecific(It.IsAny<User>())).Returns(() => returningUser);

            //act
            var result = _userHandler.GetUser(returningUser.UserId.ToString());

            //assert
            _mockUserRepo.Verify(repository => repository.Create(It.IsAny<User>()), Times.Never);
            _mockUserRepo.Verify(repository => repository.Update(It.IsAny<User>()), Times.Never);
            _mockUserRepo.Verify(repository => repository.Delete(It.IsAny<User>()), Times.Never);
            _mockUserRepo.As<IReadSpecific<User>>().Verify(specific => specific.ReadSpecific(It.IsAny<User>()), Times.Once);
        }

        [TestMethod]
        public void UpdateUser_GivenParameter_ReturnsTrueIfUpdatedUserPassed()
        {
            //arrange
            User returningUser = new User("TestUserName", "VGVzdFBhc3N3b3Jk") { UserId = Guid.NewGuid(), Facebook = false, Twitter = true };

            //act
            _userHandler.UpdateUser(returningUser);

            //assert
            Assert.AreEqual(returningUser.UserId, updatedUser.UserId);
            Assert.AreEqual(returningUser.Password, updatedUser.Password);
            Assert.AreEqual(returningUser.UserName, updatedUser.UserName);
            Assert.AreEqual(returningUser.EmailAddress, updatedUser.EmailAddress);
            Assert.AreEqual(returningUser.Twitter, updatedUser.Twitter);
            Assert.AreEqual(returningUser.Facebook, updatedUser.Facebook);
        }

        [TestMethod]
        public void UpdateUser_GivenParameter_ReturnsTrueIfUpdateCalledCorrectly()
        {
            //arrange
            User returningUser = new User("TestUserName", "VGVzdFBhc3N3b3Jk") { UserId = Guid.NewGuid(), Facebook = false, Twitter = true };

            //act
            _userHandler.UpdateUser(returningUser);

            //assert
            _mockUserRepo.Verify(repository => repository.Create(It.IsAny<User>()), Times.Never);
            _mockUserRepo.Verify(repository => repository.Update(It.IsAny<User>()), Times.Once);
            _mockUserRepo.Verify(repository => repository.Delete(It.IsAny<User>()), Times.Never);
            _mockUserRepo.As<IReadSpecific<User>>().Verify(specific => specific.ReadSpecific(It.IsAny<User>()), Times.Never);
        }
    }
}
