using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FDM90.Models.Helpers;

namespace FDM90UnitTests
{
    [TestClass]
    public class EncryptionHelperUnitTest
    {
        string testString;
        string encryptedString;

        [TestInitialize]
        public void StartUp()
        {
            testString = "TestString";
            encryptedString = "VGVzdFN0cmluZw==";
        }

        [TestCleanup]
        public void CleanUp()
        {
            testString = null;
            encryptedString = null;
        }

        [TestMethod]
        public void EncryptString_GivenStringInput_ReturnsTrueIfValueIsDifferent()
        {
            // arrange

            // act
            var result = EncryptionHelper.EncryptString(testString);

            // assert
            Assert.AreNotEqual(testString, result);
        }

        [TestMethod]
        public void DecryptString_GivenStringInput_ReturnsTrueIfValueIsDifferent()
        {
            // arrange

            // act
            var result = EncryptionHelper.DecryptString(encryptedString);

            // assert
            Assert.AreNotEqual(encryptedString, result);
        }

        [TestMethod]
        public void DecryptEncryptedString_GivenStringInputValue_ReturnsTrueIfValueIsSameAfterDecryption()
        {
            // arrange

            // act
            var encrypted = EncryptionHelper.EncryptString(testString);
            var decrypted = EncryptionHelper.DecryptString(encrypted);

            // assert
            Assert.AreEqual(testString, decrypted);
        }
    }
}
