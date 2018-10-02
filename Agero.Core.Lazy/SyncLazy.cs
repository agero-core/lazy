using System;
using System.Threading;

namespace Agero.Core.Lazy
{
    /// <summary>see cref="Lazy{T}"/> wrapper which re-creates lazily initiated value when lazy initialization throws exception.</summary>
    /// <typeparam name="T">The type of object that is being lazily initialized.</typeparam>
    public class SyncLazy<T>
    {
        private Lazy<T> _lazy;

        private readonly Func<T> _valueFactory;
        private readonly bool? _isThreadSafe;
        private readonly LazyThreadSafetyMode? _mode;

        /// <summary>Initializes a new instance of the <see cref="SyncLazy{T}"></see> class. When lazy initialization occurs, the default constructor of the target type is used.</summary>
        public SyncLazy()
        {
            _lazy = new Lazy<T>();
        }

        /// <summary>Initializes a new instance of the <see cref="SyncLazy{T}"></see> class. When lazy initialization occurs, the default constructor of the target type and the specified initialization mode are used.</summary>
        /// <param name="isThreadSafe">true to make this instance usable concurrently by multiple threads; false to make the instance usable by only one thread at a time.</param>
        public SyncLazy(bool isThreadSafe)
        {
            _lazy = new Lazy<T>(isThreadSafe);
            
            _isThreadSafe = isThreadSafe;
        }

        /// <summary>Initializes a new instance of the <see cref="SyncLazy{T}"></see> class. When lazy initialization occurs, the specified initialization function is used.</summary>
        /// <param name="valueFactory">The delegate that is invoked to produce the lazily initialized value when it is needed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="valueFactory">valueFactory</paramref> is null.</exception>
        public SyncLazy(Func<T> valueFactory)
        {
            _lazy = new Lazy<T>(valueFactory);
            
            _valueFactory = valueFactory;
        }

        /// <summary>Initializes a new instance of the <see cref="SyncLazy{T}"></see> class. When lazy initialization occurs, the specified initialization function and initialization mode are used.</summary>
        /// <param name="valueFactory">The delegate that is invoked to produce the lazily initialized value when it is needed.</param>
        /// <param name="isThreadSafe">true to make this instance usable concurrently by multiple threads; false to make this instance usable by only one thread at a time.</param>
        /// <exception cref="ArgumentNullException"><paramref name="valueFactory">valueFactory</paramref> is null.</exception>
        public SyncLazy(Func<T> valueFactory, bool isThreadSafe)
        {
            _lazy = new Lazy<T>(valueFactory, isThreadSafe);           
            
            _valueFactory = valueFactory;
            _isThreadSafe = isThreadSafe;
        }

        /// <summary>Initializes a new instance of the <see cref="SyncLazy{T}"></see> class that uses the specified initialization function and thread-safety mode.</summary>
        /// <param name="valueFactory">The delegate that is invoked to produce the lazily initialized value when it is needed.</param>
        /// <param name="mode">One of the enumeration values that specifies the thread safety mode.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="mode">mode</paramref> contains an invalid value.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="valueFactory">valueFactory</paramref> is null.</exception>
        public SyncLazy(Func<T> valueFactory, LazyThreadSafetyMode mode)
        {
            _lazy = new Lazy<T>(valueFactory, mode);
            
            _valueFactory = valueFactory;
            _mode = mode;
        }

        /// <summary>Initializes a new instance of the <see cref="SyncLazy{T}"></see> class that uses the default constructor of <typeref name="T">T</typeref> and the specified thread-safety mode.</summary>
        /// <param name="mode">One of the enumeration values that specifies the thread safety mode.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="mode">mode</paramref> contains an invalid value.</exception>
        public SyncLazy(LazyThreadSafetyMode mode)
        {
            _lazy = new Lazy<T>(mode);
            
            _mode = mode;
        }

        /// <summary>Gets a value that indicates whether a value has been created for this <see cref="SyncLazy{T}"></see> instance.</summary>
        /// <returns>true if a value has been created for this <see cref="SyncLazy{T}"></see> instance; otherwise, false.</returns>
        public bool IsValueCreated => _lazy.IsValueCreated;
        
        private readonly object _sync = new object();
        
        /// <summary>Gets the lazily initialized value of the current <see cref="SyncLazy{T}"></see> instance.</summary>
        /// <returns>The lazily initialized value of the current <see cref="SyncLazy{T}"></see> instance.</returns>
        /// <exception cref="MemberAccessException">The <see cref="SyncLazy{T}"></see> instance is initialized to use the default constructor of the type that is being lazily initialized, and permissions to access the constructor are missing.</exception>
        /// <exception cref="MissingMemberException">The <see cref="SyncLazy{T}"></see> instance is initialized to use the default constructor of the type that is being lazily initialized, and that type does not have a public, parameterless constructor.</exception>
        /// <exception cref="InvalidOperationException">The initialization function tries to access <see cref="SyncLazy{T}.Value"></see> on this instance.</exception>
        public T Value
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

        /// <summary>Creates and returns a string representation of the <see cref="SyncLazy{T}.Value"></see> property for this instance.</summary>
        /// <returns>The result of calling the <see cref="Object.ToString"></see> method on the <see cref="SyncLazy{T}.Value"></see> property for this instance, if the value has been created (that is, if the <see cref="SyncLazy{T}.IsValueCreated"></see> property returns true). Otherwise, a string indicating that the value has not been created.</returns>
        /// <exception cref="NullReferenceException">The <see cref="SyncLazy{T}.Value"></see> property is null.</exception>
        public override string ToString()
        {
            return _lazy.ToString();
        }

        /// <summary>Resets the lazily initialized value of the current instance.</summary>
        public void ClearValue()
        {
            lock (_sync)
            {
                _lazy = CreateLazy(_valueFactory, _isThreadSafe, _mode);
            }
        }

        private static Lazy<T> CreateLazy(Func<T> valueFactory = null, bool? isThreadSafe = null, LazyThreadSafetyMode? mode = null)
        {
            if (valueFactory != null)
            {
                if (isThreadSafe.HasValue)
                    return new Lazy<T>(valueFactory, isThreadSafe.Value);
                
                if (mode.HasValue)
                    return new Lazy<T>(valueFactory, mode.Value);

                return new Lazy<T>(valueFactory);
            }
            
            if (isThreadSafe.HasValue)
                return new Lazy<T>(isThreadSafe.Value);

            if (mode.HasValue)
                return new Lazy<T>(mode.Value);

            return new Lazy<T>();
        }
    }
}
