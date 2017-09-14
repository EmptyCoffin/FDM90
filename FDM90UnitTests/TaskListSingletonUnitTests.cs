using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FDM90.Singleton;
using System.Threading.Tasks;

namespace FDM90UnitTests
{
    [TestClass]
    public class TaskListSingletonUnitTests
    {
        [TestMethod]
        public void ConstructorTests_GivenSingletonCalled_ReturnsTrueIfInstanceIsNotNull()
        {
            // arrange

            // act
            var result = TaskListSingleton.Instance;

            // assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void SingletonTest_GivenSingletonAssigned_ReturnsTrueIfValuesMatch()
        {
            // arrange
            TaskListSingleton.Instance.CurrentTasks.Add(new Task<string>(() => { return string.Empty; }));

            // act
            var result = TaskListSingleton.Instance.CurrentTasks;

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }
    }
}
