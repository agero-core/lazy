using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Agero.Core.Lazy.Tests.Tests
{
    [TestClass]
    public class LazyTests
    {
        [TestMethod]
        public void Test_No_Exception()
        {
            // Arrange
            const string RESULT = "Test";
            var lazy = new SyncLazy<string>(() => RESULT);

            // Act
            var value = lazy.Value;

            // Assert
            Assert.AreEqual(RESULT, value);
            Assert.IsTrue(lazy.IsValueCreated);
        }

        [TestMethod]
        public void Test_Exception()
        {
            // Arrange
            var lazy = new SyncLazy<string>(() => throw new NullReferenceException("Result cannot be null"));

            try
            {
                // Act
                var value = lazy.Value;
                Assert.Fail("No exception");
            }
            catch (NullReferenceException ex)
            {
                // Assert
                Assert.IsInstanceOfType(ex, typeof(NullReferenceException));
            }
            finally
            {
                // Assert
                Assert.IsFalse(lazy.IsValueCreated);
            }
        }

        [TestMethod]
        public void Test_Reset_on_Exception()
        {
            // Arrange
            string result = null;
            var lazy = new SyncLazy<string>(() =>
            {
                if (result == null)
                    throw new NullReferenceException("Result cannot be null");
                return result;
            });
            
            try
            {
                // Act
                var value = lazy.Value;
                Assert.Fail("no exception");
            }
            catch (NullReferenceException)
            {
                result = "test";
                var value = lazy.Value;

                // Assert
                Assert.AreEqual(result, value);
            }
            finally
            {
                // Assert
                Assert.IsTrue(lazy.IsValueCreated);
            }
        }
    }
}
