using System;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp.General
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
        private readonly Dictionary<Type, T> _dictionary;

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
        public IEnumerable<T> Values => _dictionary.Values;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ITypeDictionary{T}" />.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count => _dictionary.Count;

        /// <summary>
        /// Gets or sets the value with the specified key.
        /// </summary>
        /// <param name="key">The type.</param>
        /// <returns>The object of the specified type.</returns>
        public T this[Type key] => _dictionary[key];

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeDictionary{T}" /> class.
        /// </summary>
        public TypeDictionary()
        {
            _dictionary = new Dictionary<Type, T>();
        }

        /// <summary>
        /// Adds the specified value to the dictionary.
        /// </summary>
        /// <typeparam name="V">The value type.</typeparam>
        /// <param name="value">The value.</param>
        public void Add<V>(V value) where V : T
        {
            _dictionary.Add(value.GetType(), value);
            if (!_dictionary.ContainsKey(typeof(V)))
                _dictionary[typeof(V)] = value;
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
        /// Gets all strongly typed values from the dictionary.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <returns>
        /// The results.
        /// </returns>
        public IEnumerable<TResult> GetAllValues<TResult>() where TResult : T
        {
            if (_dictionary.TryGetValue(typeof(TResult), out var result))
                yield return (TResult)result;
            yield break;
        }

        /// <summary>
        /// Tries to get a strongly typed value from the dictionary.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <c>true</c> if the specified key contains the type; otherwise <c>false</c>.
        /// </returns>
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
        public bool ContainsValue(T value) => _dictionary.ContainsValue(value);

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
            return _dictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Removes all items from the <see cref="TypeDictionary{T}"/>.
        /// </summary>
        public void Clear() => _dictionary.Clear();

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
        ICloneable ICloneable.Clone() => Clone();

        /// <summary>
        /// Copies the contents of one interface to this one.
        /// </summary>
        /// <param name="source">The source parameter.</param>
        void ICloneable.CopyFrom(ICloneable source) => CopyFrom(source);

        /// <summary>
        /// Clones the instance.
        /// </summary>
        /// <returns>
        /// The cloned instance.
        /// </returns>
        protected virtual ICloneable Clone()
        {
            var clone = new TypeDictionary<T>();
            foreach (var pair in _dictionary)
            {
                if (pair.Value is ICloneable cloneable)
                    clone._dictionary.Add(pair.Key, (T)cloneable.Clone());
                else
                    clone._dictionary.Add(pair.Key, pair.Value);
            }
            return clone;
        }

        /// <summary>
        /// Copies the contents of one interface to this one.
        /// </summary>
        /// <param name="source">The source parameter.</param>
        protected virtual void CopyFrom(ICloneable source)
        {
            _dictionary.Clear();
            foreach (var pair in _dictionary)
            {
                _dictionary.Add(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// Removes the value of the specified type from the dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// <c>true</c> if the value has been removed succesfully; otherwise <c>false</c>.
        /// </returns>
        public bool Remove(Type key)
            => _dictionary.Remove(key);
    }
}
