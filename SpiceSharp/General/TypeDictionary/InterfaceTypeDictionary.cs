using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.General
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
        private readonly Dictionary<Type, TypeValues<T>> _interfaces;

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <value>
        /// The keys.
        /// </value>
        public IEnumerable<Type> Keys => _interfaces.Keys;

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>
        /// The values.
        /// </value>
        public IEnumerable<T> Values => _dictionary.Values;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ITypeDictionary{T}" />.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count => _dictionary.Count;

        /// <summary>
        /// Gets the value with the specified type.
        /// </summary>
        /// <value>
        /// The associated value.
        /// </value>
        /// <param name="type">The type.</param>
        /// <returns>The associated value.</returns>
        public T this[Type type]
        {
            get
            {
                if (_interfaces.TryGetValue(type, out var result))
                    return result.Value;
                return _dictionary[type];
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterfaceTypeDictionary{T}"/> class.
        /// </summary>
        public InterfaceTypeDictionary()
        {
            _dictionary = new Dictionary<Type, T>();
            _interfaces = new Dictionary<Type, TypeValues<T>>();
        }

        /// <summary>
        /// Adds the specified value to the dictionary.
        /// </summary>
        /// <typeparam name="V">The value type.</typeparam>
        /// <param name="value">The value.</param>
        public void Add<V>(V value) where V : T
        {
            // Add a regular class entry
            var ctype = value.GetType();
            _dictionary.Add(value.GetType(), value);

            // Make references for the interfaces as well
            foreach (var type in InterfaceCache.Get(ctype))
            {
                if (!_interfaces.TryGetValue(type, out var values))
                {
                    values = new TypeValues<T>();
                    _interfaces.Add(type, values);
                }
                values.Add(value);
            }
        }

        /// <summary>
        /// Removes the specified value from the dictionary.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <c>true</c> if the value was removed; otherwise <c>false</c>.
        /// </returns>
        public bool Remove(T value)
        {
            var ctype = value.GetType();
            if (ContainsValue(value))
            {
                _dictionary.Remove(ctype);
                foreach (var type in InterfaceCache.Get(ctype))
                    _interfaces[type].Remove(value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the strongly typed value from the dictionary.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <returns>
        /// The result.
        /// </returns>
        public TResult GetValue<TResult>() where TResult : T
        {
            if (_interfaces.TryGetValue(typeof(TResult), out var result))
            {
                if (result.IsAmbiguous)
                    throw new AmbiguousTypeException(typeof(TResult));
                return (TResult)result.Value;
            }
            return (TResult)_dictionary[typeof(TResult)];
        }

        /// <summary>
        /// Gets all strongly typed values from the dictionary.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <returns>
        /// The results.
        /// </returns>
        public IEnumerable<TResult> GetAllValues<TResult>() where TResult : T
        {
            if (_interfaces.TryGetValue(typeof(TResult), out var result))
                return result.Values.Cast<TResult>();
            return Enumerable.Empty<TResult>();
        }

        /// <summary>
        /// Tries to get a strongly typed value from the dictionary.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGetValue<TResult>(out TResult value) where TResult : T
        {
            if (_interfaces.TryGetValue(typeof(TResult), out var result))
            {
                if (result.IsAmbiguous)
                    throw new AmbiguousTypeException(typeof(TResult));
                value = (TResult)result.Value;
                return true;
            }
            if (_dictionary.TryGetValue(typeof(TResult), out var direct))
            {
                value = (TResult)direct;
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
        public bool TryGetValue(Type key, out T value)
        {
            if (_interfaces.TryGetValue(key, out var result))
            {
                if (result.IsAmbiguous)
                    throw new AmbiguousTypeException(key);
                value = result.Value;
                return true;
            }
            if (_dictionary.TryGetValue(key, out var direct))
            {
                value = direct;
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Determines whether the dictionary contains a value of the specified type.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// <c>true</c> if the specified key contains key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(Type key) => _interfaces.ContainsKey(key) || _dictionary.ContainsKey(key);

        /// <summary>
        /// Determines whether the dictionary contains the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <c>true</c> if the dictionary contains the specified value; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsValue(T value) => _dictionary.TryGetValue(value.GetType(), out var result) && result.Equals(value);

        /// <summary>
        /// Clears all items in the dictionary.
        /// </summary>
        public void Clear()
        {
            _interfaces.Clear();
            _dictionary.Clear();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<Type, T>> GetEnumerator()
        {
            foreach (var pair in _interfaces)
                yield return new KeyValuePair<Type, T>(pair.Key, pair.Value.Value);
            foreach (var pair in _dictionary)
                yield return new KeyValuePair<Type, T>(pair.Key, pair.Value);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Clones the instance.
        /// </summary>
        /// <returns>
        /// The cloned instance.
        /// </returns>
        ICloneable ICloneable.Clone()
        {
            var clone = new InheritedTypeDictionary<T>();
            foreach (var v in Values)
            {
                var cloneValue = v;
                if (v is ICloneable cloneable)
                    cloneValue = (T)cloneable.Clone();
                clone.Add(cloneValue);
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
            _interfaces.Clear();
            var src = (InterfaceTypeDictionary<T>)source;
            foreach (var value in src.Values)
                Add(value);
        }
    }
}
