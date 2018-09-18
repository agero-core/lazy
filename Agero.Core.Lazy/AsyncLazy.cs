using System;
using System.Threading.Tasks;
using Agero.Core.Checker;
using Agero.Core.Lazy.Extensions;

namespace Agero.Core.Lazy
{
    /// <summary>Limited async implementation of Lazy{TValue} with task re-try functionality</summary>
    public class AsyncLazy<TValue>
    {
        private readonly Func<Task<TValue>> _valueFactory;

        private volatile Lazy<Task<TValue>> _lazy;

        private readonly object _sync = new object();

        /// <summary>Constructor</summary>
        /// <param name="valueFactory">Async value factory</param>
        public AsyncLazy(Func<Task<TValue>> valueFactory)
        {
            Check.ArgumentIsNull(valueFactory, "valueFactory");

            _valueFactory = valueFactory;
            _lazy = CreateLazy();
        }

        /// <summary>Returns value</summary>
        public async Task<TValue> GetValueAsync()
        {
            return await GetTask();
        }

        private Task<TValue> GetTask()
        {
            var lazy = _lazy;

            var task = lazy.Value;

            if (!task.IsCompletedWithError())
                return task;

            lock (_sync)
            {
                if (ReferenceEquals(lazy, _lazy))
                    _lazy = CreateLazy();
            }

            return _lazy.Value;
        }

        /// <summary>Checks whether value is created</summary>
        public bool IsValueCreated
        {
            get
            {
                var lazy = _lazy;

                if (!lazy.IsValueCreated)
                    return false;

                var task = lazy.Value;

                return !task.IsCompletedWithError();
            }
        }

        /// <summary>Clears value</summary>
        public void ClearValue()
        {
            lock (_sync)
            {
                _lazy = CreateLazy();
            }
        }

        private Lazy<Task<TValue>> CreateLazy()
        {
            return new Lazy<Task<TValue>>(async () => await _valueFactory());
        }
    }
}
