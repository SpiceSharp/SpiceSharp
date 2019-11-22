using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace SpiceSharp
{
    /// <summary>
    /// An implementation of the <see cref="ITypeDictionary{T}"/> interface that
    /// also allows retrieving information using interface types.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    /// <seealso cref="ITypeDictionary{T}" />
    public class InterfaceTypeDictionary<T> : ITypeDictionary<T>
    {
        private readonly Dictionary<Type, T> _dictionary;
        private readonly HashSet<T> _values;

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <value>
        /// The keys.
        /// </value>
        public IEnumerable<Type> Keys => _dictionary.Keys;

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>
        /// The values.
        /// </value>
        public IEnumerable<T> Values => _values;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:SpiceSharp.ITypeDictionary`1" />.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count => _values.Count;

        /// <summary>
        /// Gets the value with the specified type.
        /// </summary>
        /// <value>
        /// The associated value.
        /// </value>
        /// <param name="type">The type.</param>
        /// <returns>The associated value.</returns>
        public T this[Type type] => _dictionary[type];

        /// <summary>
        /// Initializes a new instance of the <see cref="InterfaceTypeDictionary{T}"/> class.
        /// </summary>
        public InterfaceTypeDictionary()
        {
            _dictionary = new Dictionary<Type, T>();
            _values = new HashSet<T>();
        }

        /// <summary>
        /// Adds the specified value to the dictionary.
        /// </summary>
        /// <typeparam name="V">The value type.</typeparam>
        /// <param name="value">The value.</param>
        public void Add<V>(V value) where V : T
        {
            // Add a regular one
            _dictionary.Add(value.GetType(), value);
            if (!_dictionary.ContainsKey(typeof(V)))
                _dictionary.Add(typeof(V), value);

            // Make references for the interfaces as well
            var ifs = value.GetType().GetTypeInfo().GetInterfaces();
            foreach (var type in ifs)
                _dictionary[type] = value;
            _values.Add(value);
        }

        /// <summary>
        /// Gets the strongly typed value from the dictionary.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <returns>
        /// The result.
        /// </returns>
        public TResult GetValue<TResult>() where TResult : T
            => (TResult)_dictionary[typeof(TResult)];

        /// <summary>
        /// Tries to get a strongly typed value from the dictionary.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGetValue<TResult>(out TResult value) where TResult : T
        {
            if (_dictionary.TryGetValue(typeof(TResult), out var result))
            {
                value = (TResult)result;
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Tries to get a value from the dictionary.
        /// </summary>
        /// <param name="key">The key type.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGetValue(Type key, out T value) => _dictionary.TryGetValue(key, out value);

        /// <summary>
        /// Determines whether the dictionary contains a value of the specified type.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// <c>true</c> if the specified key contains key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(Type key) => _dictionary.ContainsKey(key);

        /// <summary>
        /// Determines whether the dictionary contains the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <c>true</c> if the dictionary contains the specified value; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsValue(T value) => _values.Contains(value);

        /// <summary>
        /// Clears all items in the dictionary.
        /// </summary>
        public void Clear()
        {
            _dictionary.Clear();
            _values.Clear();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<Type, T>> GetEnumerator() => _dictionary.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();

        /// <summary>
        /// Clones the instance.
        /// </summary>
        /// <returns>
        /// The cloned instance.
        /// </returns>
        ICloneable ICloneable.Clone()
        {
            var clone = new InterfaceTypeDictionary<T>();
            foreach (var pair in _dictionary)
            {
                var value = pair.Value;
                if (pair.Value is ICloneable cloneable)
                    value = (T)cloneable.Clone();
                clone._dictionary.Add(pair.Key, value);
                clone._values.Add(value);
            }
            return clone;
        }

        /// <summary>
        /// Copies the contents of one interface to this one.
        /// </summary>
        /// <param name="source">The source parameter.</param>
        void ICloneable.CopyFrom(ICloneable source)
        {
            _dictionary.Clear();
            _values.Clear();
            foreach (var pair in _dictionary)
            {
                _dictionary.Add(pair.Key, pair.Value);
                _values.Add(pair.Value);
            }
        }
    }
}
