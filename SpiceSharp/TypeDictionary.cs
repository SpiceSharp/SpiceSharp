using System;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp
{
    /// <summary>
    /// Class for storing classes by their type
    /// It also trackes inheritance, so you can retrieve classes by their base class
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    public abstract class TypeDictionary<T> : IDictionary<Type, T> where T : class
    {
        /// <summary>
        /// Dictionary with our types
        /// </summary>
        protected Dictionary<Type, T> Dictionary { get; }

        /// <summary>
        /// True if the base classes are also considered
        /// </summary>
        protected Type BaseClass { get; }

        /// <summary>
        /// Gets the keys of the dictionary
        /// </summary>
        public ICollection<Type> Keys => Dictionary.Keys;

        /// <summary>
        /// Gets the values of the dictionary
        /// </summary>
        public ICollection<T> Values => Dictionary.Values;

        /// <summary>
        /// Count
        /// </summary>
        public int Count => Dictionary.Count;

        /// <summary>
        /// Is the collection read-only?
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets or sets an element
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns></returns>
        public T this[Type key]
        {
            get => Dictionary[key];
            set
            {
                Type currentType = key;
                while (currentType != BaseClass)
                {
                    Dictionary[currentType] = value;
                    if (currentType == typeof(object))
                        throw new ArgumentException("Type {0} is not derived from {1}".FormatString(key, BaseClass));
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseClass">The base class that does not need to be taken into account</param>
        protected TypeDictionary(Type baseClass)
        {
            BaseClass = baseClass;
            Dictionary = new Dictionary<Type, T>();
        }

        /// <summary>
        /// Add a value
        /// </summary>
        /// <param name="key">Key type</param>
        /// <param name="value">Value</param>
        public virtual void Add(Type key, T value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            Type currentType = key;
            while (currentType != null && currentType != BaseClass)
            {
                Dictionary.Add(currentType, value);
                currentType = currentType.BaseType;
                if (currentType == typeof(object))
                    throw new CircuitException("Type {0} is not derived from {1}".FormatString(key, BaseClass));
            }
        }

        /// <summary>
        /// Gets a strongly typed value from the dictionary
        /// </summary>
        /// <typeparam name="TResult">Return type</typeparam>
        /// <returns></returns>
        public TResult Get<TResult>() where TResult : T
        {
            if (Dictionary.TryGetValue(typeof(TResult), out var value))
                return (TResult)value;
            return default;
        }

        /// <summary>
        /// Contains key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns></returns>
        public bool ContainsKey(Type key) => Dictionary.ContainsKey(key);

        /// <summary>
        /// Remove a specific key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns></returns>
        public bool Remove(Type key) => Dictionary.Remove(key);

        /// <summary>
        /// Try getting a value by key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns></returns>
        public bool TryGetValue(Type key, out T value) => Dictionary.TryGetValue(key, out value);

        /// <summary>
        /// Add a key-value pair
        /// </summary>
        /// <param name="item"></param>
        public void Add(KeyValuePair<Type, T> item) => Add(item.Key, item.Value);

        /// <summary>
        /// Clear the dictionary
        /// </summary>
        public void Clear() => Dictionary.Clear();

        /// <summary>
        /// Gets enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<Type, T>> GetEnumerator() => Dictionary.GetEnumerator();

        /// <summary>
        /// Gets enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => Dictionary.GetEnumerator();

        /// <summary>
        /// Check if the dictionary contains a key-value pair
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool ICollection<KeyValuePair<Type, T>>.Contains(KeyValuePair<Type, T> item) => ((ICollection<KeyValuePair<Type, T>>)Dictionary).Contains(item);

        /// <summary>
        /// Copy the contents to an array
        /// </summary>
        /// <param name="array">Array</param>
        /// <param name="arrayIndex">Array index</param>
        void ICollection<KeyValuePair<Type, T>>.CopyTo(KeyValuePair<Type, T>[] array, int arrayIndex) => ((ICollection<KeyValuePair<Type, T>>)Dictionary).CopyTo(array, arrayIndex);

        /// <summary>
        /// Remove a key-value pair
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns></returns>
        bool ICollection<KeyValuePair<Type, T>>.Remove(KeyValuePair<Type, T> item) => ((ICollection<KeyValuePair<Type, T>>)Dictionary).Remove(item);
    }
}
