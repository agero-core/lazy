using System;
using System.Threading.Tasks;
using Agero.Core.Checker;
using Agero.Core.Lazy.Extensions;

namespace Agero.Core.Lazy
{
    /// <summary>Provides support for lazy initialization.</summary>
    /// <remarks>Limited async implementation of Lazy{TValue} with task re-initialization.</remarks>
    /// <typeparam name="T">The type of object that is being lazily initialized.</typeparam>
    public class AsyncLazy<T>
    {
        private readonly Func<Task<T>> _valueFactory;

        private volatile Lazy<Task<T>> _lazy;

        /// <summary>Initializes a new instance of the <see cref="AsyncLazy{T}"></see> class. When lazy initialization occurs, the specified initialization function is used.</summary>
        /// <param name="valueFactory">The delegate that is invoked to produce the lazily initialized value when it is needed.</param>
        public AsyncLazy(Func<Task<T>> valueFactory)
        {
            Check.ArgumentIsNull(valueFactory, "valueFactory");

            _valueFactory = valueFactory;
            _lazy = CreateLazy();
        }

        private readonly object _sync = new object();
        
        /// <summary>Gets the lazily initialized <see cref="Task{TResult}"/> of value of the current <see cref="AsyncLazy{T}"></see> instance.</summary>
        /// <returns>The lazily initialized <see cref="Task{TResult}"/> of value of the current <see cref="AsyncLazy{T}"></see> instance.</returns>
        public async Task<T> GetValueAsync()
        {
            var lazy = _lazy;

            var task = lazy.Value;

            if (!task.IsCompletedWithError())
                return await task;

            lock (_sync)
            {
                if (ReferenceEquals(lazy, _lazy))
                    _lazy = CreateLazy();
            }

            return await _lazy.Value;
        }

        /// <summary>Gets a value that indicates whether a value has been created for this <see cref="AsyncLazy{T}"></see> instance.</summary>
        /// <returns>true if a value has been created for this <see cref="AsyncLazy{T}"></see> instance; otherwise, false.</returns>
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

        /// <summary>Resets the lazily initialized value of the current instance.</summary>
        public void ClearValue()
        {
            lock (_sync)
            {
                _lazy = CreateLazy();
            }
        }

        private Lazy<Task<T>> CreateLazy()
        {
            return new Lazy<Task<T>>(async () => await _valueFactory());
        }
    }
}
