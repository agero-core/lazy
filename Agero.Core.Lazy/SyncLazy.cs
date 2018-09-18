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

        /// <summary>
        /// Constructor
        /// </summary>
        public SyncLazy()
        {
            _lazy = new Lazy<TValue>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isThreadSafe">Is Thread Safe</param>
        public SyncLazy(bool isThreadSafe)
        {
            _isThreadSafe = isThreadSafe;

            _lazy = new Lazy<TValue>(isThreadSafe);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="valueFactory">Value Factory</param>
        public SyncLazy(Func<TValue> valueFactory)
        {
            _valueFactory = valueFactory;

            _lazy = new Lazy<TValue>(valueFactory);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="valueFactory">Value Factory</param>
        /// <param name="isThreadSafe">is Thread Safe</param>
        public SyncLazy(Func<TValue> valueFactory, bool isThreadSafe)
        {
            _valueFactory = valueFactory;
            _isThreadSafe = isThreadSafe;

            _lazy = new Lazy<TValue>(valueFactory, isThreadSafe);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="valueFactory">Value Factory</param>
        /// <param name="mode">Lazy Thread Safety Mode</param>
        public SyncLazy(Func<TValue> valueFactory, LazyThreadSafetyMode mode)
        {
            _valueFactory = valueFactory;
            _mode = mode;

            _lazy = new Lazy<TValue>(valueFactory, mode);
        }

        /// <summary>
        /// Returns value
        /// </summary>
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

        /// <summary>
        /// Gets value created or not
        /// </summary>
        public bool IsValueCreated => _lazy.IsValueCreated;

        /// <summary>
        /// Unset value
        /// </summary>
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
