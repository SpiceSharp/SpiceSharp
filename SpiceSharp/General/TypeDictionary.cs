using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace SpiceSharp
{
    /// <summary>
    /// An implementation of the <see cref="ITypeDictionary{T}"/> interface.
    /// This implementation supports multithreaded access.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    /// <seealso cref="ITypeDictionary{T}" />
    public class TypeDictionary<T> : ITypeDictionary<T>
    {
        /// <summary>
        /// Gets the dictionary to look up using types.
        /// </summary>
        protected Dictionary<Type, T> Dictionary { get; }

        /// <summary>
        /// Gets the lock used for multithreaded usage.
        /// </summary>
        protected ReaderWriterLockSlim Lock { get; }

        /// <summary>
        /// Gets an <see cref="ICollection{T}" /> containing the keys of the <see cref="TypeDictionary{T}" />.
        /// </summary>
        public ICollection<Type> Keys
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return Dictionary.Keys;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Gets an <see cref="ICollection{T}" /> containing the values in the <see cref="TypeDictionary{T}" />.
        /// </summary>
        public ICollection<T> Values
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    var values = new HashSet<T>();
                    foreach (var v in Dictionary.Values)
                        values.Add(v);
                    return values;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="TypeDictionary{T}" />.
        /// </summary>
        public int Count
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return Dictionary.Count;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Gets or sets the value with the specified key.
        /// </summary>
        /// <param name="key">The type.</param>
        /// <returns>The object of the specified type.</returns>
        public T this[Type key]
        {
            get
            {
                key.ThrowIfNull(nameof(key));

                Lock.EnterReadLock();
                try
                {
                    return Dictionary[key];
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }
            set
            {
                key.ThrowIfNull(nameof(key));

                Lock.EnterWriteLock();
                try
                {
                    // This may seem a bit tricky:
                    // - If 'isChild' is true, then that means the new added class has not been implemented yet
                    //   by any other class. It will not overwrite any base classes that already have been
                    //   implemented as it may remove references to "simpler" base classes".
                    // - if 'isChild' is false, then a class has been added that already implements the type.
                    //   The new value is considered to be a "simpler" type and it will overwrite the existing
                    //   types.
                    var isChild = !Dictionary.ContainsKey(key);

                    // Add the regular class hierarchy that this instance implements.
                    var currentType = key;
                    while (currentType != null && currentType != typeof(object))
                    {
                        if (!isChild)
                            Dictionary[currentType] = value;
                        else if (!Dictionary.ContainsKey(currentType))
                            Dictionary.Add(currentType, value);
                        else
                            break;
                        currentType = currentType.GetTypeInfo().BaseType;
                    }

                    // Also add all interfaces this instance implements.
                    foreach (var itf in key.GetTypeInfo().GetInterfaces())
                    {
                        if (!isChild)
                            Dictionary[itf] = value;
                        else if (!Dictionary.ContainsKey(itf))
                            Dictionary.Add(itf, value);
                    }
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeDictionary{T}" /> class.
        /// </summary>
        public TypeDictionary()
        {
            Lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            Dictionary = new Dictionary<Type, T>();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="TypeDictionary{T}"/> class.
        /// </summary>
        ~TypeDictionary()
        {
            Lock?.Dispose();
        }

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="TypeDictionary{T}"/>.
        /// </summary>
        /// <param name="value">The added value.</param>
        public virtual void Add(T value)
        {
            var key = value.GetType();
            Lock.EnterWriteLock();
            try
            {
                // This may seem a bit tricky:
                // - If 'isChild' is true, then that means the new added class has not been implemented yet
                //   by any other class. It will not overwrite any base classes that already have been
                //   implemented as it may remove references to "simpler" base classes".
                // - if 'isChild' is false, then a class has been added that already implements the type.
                //   The new value is considered to be a "simpler" type and it will overwrite the existing
                //   types.
                var isChild = !Dictionary.ContainsKey(key);

                // Add the regular class hierarchy that this instance implements.
                var currentType = key;
                while (currentType != null && currentType != typeof(T))
                {
                    if (!isChild)
                        Dictionary[currentType] = value;
                    else if (!Dictionary.ContainsKey(currentType))
                        Dictionary.Add(currentType, value);
                    else
                        break;
                    currentType = currentType.GetTypeInfo().BaseType;
                }

                // Also add all interfaces this instance implements.
                foreach (var itf in key.GetTypeInfo().GetInterfaces())
                {
                    if (!isChild)
                        Dictionary[itf] = value;
                    else if (!Dictionary.ContainsKey(itf))
                        Dictionary.Add(itf, value);
                }
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets a strongly typed object from the dictionary.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <returns>The requested object.</returns>
        public TResult GetValue<TResult>() where TResult : T
        {
            Lock.EnterReadLock();
            try
            {
                return (TResult) Dictionary[typeof(TResult)];
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Tries to get a strongly typed object from the dictionary.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="value">The requested object.</param>
        /// <returns>
        ///   <c>true</c> if the object was found; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetValue<TResult>(out TResult value) where TResult : T
        {
            Lock.EnterReadLock();
            try
            {
                if (Dictionary.TryGetValue(typeof(TResult), out var result))
                {
                    value = (TResult) result;
                    return true;
                }

                value = default;
                return false;
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Determines whether the <see cref="TypeDictionary{T}" /> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="TypeDictionary{T}" />.</param>
        /// <returns>
        ///   <c>true</c> if the <see cref="TypeDictionary{T}" /> contains an element with the key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(Type key)
        {
            key.ThrowIfNull(nameof(key));
            Lock.EnterReadLock();
            try
            {
                return Dictionary.ContainsKey(key);
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key,
        /// if the key is found; otherwise, the default value for the type of the
        /// <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns>
        ///   <c>true</c> if the object that implements <see cref="TypeDictionary{T}" /> contains an element with the specified key; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetValue(Type key, out T value)
        {
            key.ThrowIfNull(nameof(key));

            Lock.EnterReadLock();
            try
            {
                return Dictionary.TryGetValue(key, out value);
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Removes all items from the <see cref="TypeDictionary{T}"/>.
        /// </summary>
        public void Clear()
        {
            Lock.EnterWriteLock();
            try
            {
                Dictionary.Clear();
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<Type, T>> GetEnumerator() => Dictionary.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => Dictionary.GetEnumerator();
    }
}
