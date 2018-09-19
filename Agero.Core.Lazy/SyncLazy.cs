using System;
using System.Threading;

namespace Agero.Core.Lazy
{
    /// <summary>
    /// System.Lazy wrapper to handle initialization on exception
    /// </summary>
    /// <typeparam name="TValue">Value Type</typeparam>
    public class SyncLazy<TValue>
    {
        private Lazy<TValue> _lazy;

        private readonly object _sync = new object();

        private readonly Func<TValue> _valueFactory;
        private readonly bool? _isThreadSafe;
        private readonly LazyThreadSafetyMode? _mode;

        /// <summary>Initializes a new instance of the SyncLazy class.</summary>
        public SyncLazy()
        {
            _lazy = new Lazy<TValue>();
        }

        /// <summary>Initializes a new instance of the SyncLazy class.</summary>
        /// <param name="isThreadSafe">true to make this instance usable concurrently by multiple threads; false to make the instance usable by only one thread at a time.</param>
        public SyncLazy(bool isThreadSafe)
        {
            _isThreadSafe = isThreadSafe;

            _lazy = new Lazy<TValue>(isThreadSafe);
        }

        /// <summary>Initializes a new instance of the SyncLazy class.</summary>        
        /// <param name="valueFactory">The delegate that is invoked to produce the lazily initialized value when it is needed.</param>
        public SyncLazy(Func<TValue> valueFactory)
        {
            _valueFactory = valueFactory;

            _lazy = new Lazy<TValue>(valueFactory);
        }

        /// <summary>Initializes a new instance of the SyncLazy class.</summary>
        /// <param name="valueFactory">The delegate that is invoked to produce the lazily initialized value when it is needed.</param>
        /// <param name="isThreadSafe">true to make this instance usable concurrently by multiple threads; false to make this instance usable by only one thread at a time.</param>
        public SyncLazy(Func<TValue> valueFactory, bool isThreadSafe)
        {
            _valueFactory = valueFactory;
            _isThreadSafe = isThreadSafe;

            _lazy = new Lazy<TValue>(valueFactory, isThreadSafe);
        }

        /// <summary>Initializes a new instance of the SyncLazy class.</summary>
        /// <param name="valueFactory">The delegate that is invoked to produce the lazily initialized value when it is needed.</param>
        /// <param name="mode">One of the enumeration values that specifies the thread safety mode.</param>
        public SyncLazy(Func<TValue> valueFactory, LazyThreadSafetyMode mode)
        {
            _valueFactory = valueFactory;
            _mode = mode;

            _lazy = new Lazy<TValue>(valueFactory, mode);
        }

        /// <summary>Gets the lazily initialized value of the current instance.</summary>
        /// <returns>The lazily initialized value of the current instance.</returns>
        public TValue Value
        {
            get
            {
                try
                {
                    return _lazy.Value;
                }
                catch (Exception)
                {
                    var lazy = _lazy;
                    lock (_sync)
                    {
                        if (ReferenceEquals(lazy, _lazy))
                            _lazy = CreateLazy(_valueFactory, _isThreadSafe, _mode);
                    }

                    throw;
                }    
            
            }
        }

        /// <summary>Gets a value that indicates whether a value has been created for this instance.</summary>
        /// <returns>true if a value has been created for this instance; otherwise, false.</returns>
        public bool IsValueCreated => _lazy.IsValueCreated;

        /// <summary>Resets the lazily initialized value of the current instance</summary>
        public void ClearValue()
        {
            lock (_sync)
            {
                _lazy = CreateLazy(_valueFactory, _isThreadSafe, _mode);
            }
        }

        private static Lazy<TValue> CreateLazy(Func<TValue> valueFactory = null, bool? isThreadSafe = null, LazyThreadSafetyMode? mode = null)
        {
            if (valueFactory == null)
            {
                if (isThreadSafe.HasValue)
                    return new Lazy<TValue>(isThreadSafe.Value);

                return new Lazy<TValue>();
            }

            if (isThreadSafe.HasValue)
                return new Lazy<TValue>(valueFactory, isThreadSafe.Value);

            if (mode.HasValue)
                return new Lazy<TValue>(valueFactory, mode.Value);

            return new Lazy<TValue>(valueFactory);
        }
    }
}
