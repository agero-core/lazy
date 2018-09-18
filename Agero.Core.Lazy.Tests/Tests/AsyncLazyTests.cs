using Agero.Core.Lazy.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Agero.Core.Lazy.Tests.Tests
{
    [TestClass]
    public class AsyncLazyTests
    {
        private static void ThrowException()
        {
            throw new InvalidOperationException("Failed");
        }

        [TestMethod]
        public async Task GetValueAsync_Should_Execute_ValueFactory_Only_Once()
        {
            // Arrange
            var counter = 0;
            async Task<int> ValueFactory() => await Task.Run(() => ++counter);

            var lazy = new AsyncLazy<int>(ValueFactory);

            // Act
            var value1 = await lazy.GetValueAsync();
            var value2 = await lazy.GetValueAsync();

            // Assert
            Assert.AreEqual(1, value1);
            Assert.AreEqual(1, value2);

            Assert.AreEqual(1, counter);
        }

        [TestMethod]
        public async Task GetValueAsync_Should_Retry_When_First_Execution_Of_ValueFactory_Failed()
        {
            // Arrange
            const int RETRY_COUNT = 3;

            var counter = 0;

            async Task<int> ValueFactory() => await Task.Run(() =>
            {
                counter++;

                if (counter < RETRY_COUNT) ThrowException();

                return counter;
            });

            var lazy = new AsyncLazy<int>(ValueFactory);

            // Act
            var value = await ExceptionHelper.RunUntilSuccessfullAsync<int, InvalidOperationException>(async () => await lazy.GetValueAsync(), RETRY_COUNT);
            
            // Assert
            Assert.AreEqual(RETRY_COUNT, counter);
            Assert.AreEqual(RETRY_COUNT, value);
        }

        [TestMethod]
        public void IsValueCreated_Should_Return_False_When_Value_Is_Not_Created ()
        {
            // Arrange
            async Task<bool> ValueFactory() => await Task.Run(() => true);

            var lazy = new AsyncLazy<bool>(ValueFactory);

            // Act
            var result = lazy.IsValueCreated;

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task IsValueCreated_Should_Return_True_When_Value_Is_Created()
        {
            // Arrange
            async Task<bool> ValueFactory() => await Task.Run(() => true);

            var lazy = new AsyncLazy<bool>(ValueFactory);

            await lazy.GetValueAsync();

            // Act
            var result = lazy.IsValueCreated;

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task IsValueCreated_Should_Return_False_When_Value_Is_Created_And_ValueFactory_Is_Failed()
        {
            // Arrange
            async Task<bool> ValueFactory() => await Task.Run(() =>
            {
                ThrowException();

                return true;
            });

            var lazy = new AsyncLazy<bool>(ValueFactory);

            await ExceptionHelper.SuppressAsync<InvalidOperationException>(async () => await lazy.GetValueAsync());

            // Act
            var result = lazy.IsValueCreated;

            // Assert
            Assert.IsFalse(result);
        }
    }
}
