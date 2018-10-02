using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Agero.Core.Lazy.Tests.Tests
{
    [TestClass]
    public class SyncLazyTests
    {
        [TestMethod]
        public void Test_No_Exception()
        {
            // Arrange
            const string result = "Test";
            var lazy = new SyncLazy<string>(() => result);

            // Act
            var value = lazy.Value;

            // Assert
            Assert.AreEqual(result, value);
            Assert.IsTrue(lazy.IsValueCreated);
        }

        [TestMethod]
        public void Test_Exception()
        {
            // Arrange
            const string exceptionMessage = "Wrong operation";

            var lazy = new SyncLazy<string>(() => throw new InvalidOperationException(exceptionMessage));

            try
            {
                // Act
                var unused = lazy.Value;
                
                Assert.Fail("No exception");
            }
            catch (InvalidOperationException ex)
            {
                // Assert
                Assert.AreEqual(exceptionMessage, ex.Message);
                Assert.IsFalse(lazy.IsValueCreated);
            }
        }

        [TestMethod]
        public void Test_Reinitialization_On_Exception()
        {
            // Arrange
            const string exceptionMessage = "Wrong operation";
            
            string result = null;
            var lazy = new SyncLazy<string>(() => result ?? throw new InvalidOperationException("Wrong operation"));
            
            try
            {
                var unused = lazy.Value;
                
                Assert.Fail("no exception");
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual(exceptionMessage, ex.Message);
            }

            result = "Some result";
            
            // Act
            var value = lazy.Value;
            
            // Assert
            Assert.AreEqual(result, value);
            Assert.IsTrue(lazy.IsValueCreated);
        }
        
        [TestMethod]
        public void Test_Value_Cache()
        {
            // Arrange
            string result = "Initial value";
            var lazy = new SyncLazy<string>(() => result);
            
            var initialValue = lazy.Value;
            Assert.AreEqual(result, initialValue);

            result = "Modified value";
            
            // Act
            var value = lazy.Value;
            
            // Assert
            Assert.AreEqual(initialValue, value);
            Assert.IsTrue(lazy.IsValueCreated);
        }
        
        [TestMethod]
        public void Test_Clear_Value()
        {
            // Arrange
            string result = "Initial value";
            var lazy = new SyncLazy<string>(() => result);
            
            var initialValue = lazy.Value;
            Assert.AreEqual(result, initialValue);

            result = "Modified value";
            
            // Act
            lazy.ClearValue();
            
            // Assert
            Assert.IsFalse(lazy.IsValueCreated);
            
            var value = lazy.Value;
            
            Assert.AreEqual(result, value);
            Assert.IsTrue(lazy.IsValueCreated);
        }
    }
}
