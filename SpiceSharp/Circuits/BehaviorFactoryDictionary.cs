using SpiceSharp.Behaviors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// Factory for behaviors
    /// </summary>
    /// <seealso cref="TypeDictionary{BehaviorFactoryMethod}" />
    public class BehaviorFactoryDictionary : IEnumerable<KeyValuePair<Type, BehaviorFactoryMethod>>
    {
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private Dictionary<Type, BehaviorFactoryMethod> _dict = new Dictionary<Type, BehaviorFactoryMethod>();

        /// <summary>
        /// Gets or sets the <see cref="BehaviorFactoryMethod"/> with the specified type.
        /// </summary>
        /// <value>
        /// The <see cref="BehaviorFactoryMethod"/>.
        /// </value>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public BehaviorFactoryMethod this[Type type]
        {
            get => _dict[type];
            set => Add(type, value);
        }

        /// <summary>
        /// Adds the specified behavior factory.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="method">The method.</param>
        public void Add(Type key, BehaviorFactoryMethod method)
        {
            key.ThrowIfNull(nameof(method));
            _lock.EnterWriteLock();
            try
            {
                // Does another factory already give this value?
                var isChild = !_dict.ContainsKey(key);

                var currentType = key;
                while (currentType != null && currentType != typeof(object))
                {
                    if (!isChild)
                        _dict[currentType] = method;
                    else if (!_dict.ContainsKey(currentType))
                        _dict.Add(currentType, method);
                    else
                        break;
                    currentType = currentType.GetTypeInfo().BaseType;
                }

                // Also add all interfaces this instance implements.
                foreach (var itf in key.GetTypeInfo().GetInterfaces())
                {
                    if (!isChild)
                        _dict[itf] = method;
                    else if (!_dict.ContainsKey(itf))
                        _dict.Add(itf, method);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if the specified key contains key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(Type key) => _dict.ContainsKey(key);

        /// <summary>
        /// Tries to get the <see cref="BehaviorFactoryMethod"/> associated with the specified type.
        /// </summary>
        /// <param name="key">The key type.</param>
        /// <param name="method">The key method.</param>
        /// <returns></returns>
        public bool TryGetValue(Type key, out BehaviorFactoryMethod method) => _dict.TryGetValue(key, out method);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<Type, BehaviorFactoryMethod>> GetEnumerator() => _dict.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// Create a behavior for an entity
    /// </summary>
    /// <param name="entity">The entity creating the behavior.</param>
    /// <returns></returns>
    public delegate IBehavior BehaviorFactoryMethod(Entity entity);
}
