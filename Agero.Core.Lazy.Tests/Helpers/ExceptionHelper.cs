using Agero.Core.Checker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Agero.Core.Lazy.Tests.Helpers
{
    internal static class ExceptionHelper
    {
        public static async Task<TValue> RunUntilSuccessfullAsync<TValue, TException>(Func<Task<TValue>> action,
            int maxRetryCount)
            where TException : Exception
        {
            Check.ArgumentIsNull(action, "action");
            Check.Argument(maxRetryCount > 0, "maxRetryCount");

            var retryCount = 0;

            while (retryCount < maxRetryCount)
            {
                try
                {
                    return await action();
                }
                catch (TException)
                {
                    retryCount++;
                }
            }

            throw new AssertFailedException($"Max number of retries '{maxRetryCount}' reached.");
        }

        public static async Task SuppressAsync<TException>(Func<Task> action)
            where TException : Exception
        {
            try
            {
                await action();
            }
            catch (TException)
            {
            }
        }
    }
}
