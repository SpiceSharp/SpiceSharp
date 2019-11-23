using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace SpiceSharp
{
    /// <summary>
    /// An <see cref="ITypeDictionary{T}"/> that tracks inheritance and interfaces.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    /// <seealso cref="ITypeDictionary{T}" />
    public class InheritedTypeDictionary<T> : ITypeDictionary<T>
    {
        private readonly Dictionary<Type, T> _dictionary;
        private readonly HashSet<T> _values;
        private readonly HashSet<Type> _ambiguousTypes;

        /// <summary>
        /// Gets the value of the specified type.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public T this[Type key] => _dictionary[key];

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ITypeDictionary{T}" />.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count => _values.Count;

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
        /// Initializes a new instance of the <see cref="InheritedTypeDictionary{T}"/> class.
        /// </summary>
        public InheritedTypeDictionary()
        {
            _dictionary = new Dictionary<Type, T>();
            _values = new HashSet<T>();
            _ambiguousTypes = new HashSet<Type>();
            _ambiguousTypes.Add(typeof(object));
        }

        /// <summary>
        /// Adds the specified value to the dictionary.
        /// </summary>
        /// <remarks>
        /// If two types share a common base class, then the common base classes become impossible
        /// to be used as a reference due to ambiguity.
        /// </remarks>
        /// <typeparam name="V">The value type.</typeparam>
        /// <param name="value">The value.</param>
        public void Add<V>(V value) where V : T
        {
            var ctype = value.GetType();
            _dictionary.Add(ctype, value);
            _values.Add(value);
            _ambiguousTypes.Remove(ctype); // We have a direct reference now

            // Track inheritance
            ctype = ctype.GetTypeInfo().BaseType;
            while (!_ambiguousTypes.Contains(ctype))
            {
                // Do we already have an entry for the base type?
                if (_dictionary.ContainsKey(ctype))
                {
                    // There is already an entry for the base class:
                    // - Any abstract classes down the line will become unavailable due to ambiguity
                    // - Any classes that do not directly reference themselves become unavailable due to ambiguity
                    while (!_ambiguousTypes.Contains(ctype))
                    {
                        var info = ctype.GetTypeInfo();
                        if (info.IsAbstract)
                            _ambiguousTypes.Add(ctype);
                        else if (info.IsClass && _dictionary.TryGetValue(ctype, out var result) && result.GetType() != ctype)
                            _ambiguousTypes.Add(ctype);
                        ctype = ctype.GetTypeInfo().BaseType;
                    }
                    break;
                }
                else
                    _dictionary.Add(ctype, value);
                ctype = ctype.GetTypeInfo().BaseType;
            }

            // Make references for the interfaces as well
            var ifs = value.GetType().GetTypeInfo().GetInterfaces();
            foreach (var type in ifs)
            {
                if (_ambiguousTypes.Contains(type))
                    continue;
                if (_dictionary.ContainsKey(type))
                {
                    // The interface can't resolve to a single item anymore
                    _dictionary.Remove(type);
                    _ambiguousTypes.Add(type);
                }
                else
                    _dictionary.Add(type, value);
            }
            _values.Add(value);
        }

        /// <summary>
        /// Clears all items in the dictionary.
        /// </summary>
        public void Clear()
        {
            _dictionary.Clear();
            _values.Clear();
        }

        /// <summary>
        /// Clones the instance.
        /// </summary>
        /// <returns>
        /// The cloned instance.
        /// </returns>
        ICloneable ICloneable.Clone()
        {
            var clone = new InheritedTypeDictionary<T>();
            foreach (var pair in _dictionary)
            {
                var value = pair.Value;
                if (pair.Value is ICloneable cloneable)
                    value = (T)cloneable.Clone();
                clone._dictionary.Add(pair.Key, value);
                clone._values.Add(value);
            }
            foreach (var type in _ambiguousTypes)
                clone._ambiguousTypes.Add(type);
            return clone;
        }

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

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<Type, T>> GetEnumerator() => _dictionary.GetEnumerator();

        /// <summary>
        /// Gets the strongly typed value from the dictionary.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <returns>
        /// The result.
        /// </returns>
        public TResult GetValue<TResult>() where TResult : T
        {
            if (_dictionary.TryGetValue(typeof(TResult), out var result))
                return (TResult)result;
            if (_ambiguousTypes.Contains(typeof(TResult)))
                throw new CircuitException("Ambiguity detected for type '{0}'".FormatString(typeof(TResult).FullName));
            throw new CircuitException("No value for '{0}'".FormatString(typeof(TResult).FullName));
        }

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
            if (_ambiguousTypes.Contains(typeof(TResult)))
                throw new CircuitException("Ambiguity detected for type '{0}'".FormatString(typeof(TResult).FullName));
            value = default;
            return false;
        }

        /// <summary>
        /// Tries to get a value from the dictionary.
        /// </summary>
        /// <param name="key">The key type.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGetValue(Type key, out T value)
        {
            key.ThrowIfNull(nameof(key));
            if (_dictionary.TryGetValue(key, out value))
                return true;
            if (_ambiguousTypes.Contains(key))
                throw new CircuitException("Ambiguity detected for type '{0}'".FormatString(key.FullName));
            value = default;
            return false;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
            => ((IEnumerable)_dictionary).GetEnumerator();
    }
}
