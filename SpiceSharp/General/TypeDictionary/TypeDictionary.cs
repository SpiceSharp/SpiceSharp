using SpiceSharp.Diagnostics;
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

        /// <inheritdoc/>
        public int Count => _dictionary.Count;

        /// <inheritdoc/>
        public T this[Type key] => _dictionary[key];

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeDictionary{T}" /> class.
        /// </summary>
        public TypeDictionary()
        {
            _dictionary = new Dictionary<Type, T>();
        }

        /// <inheritdoc/>
        public void Add<V>(V value) where V : T
        {
            value.ThrowIfNull(nameof(value));
            _dictionary.Add(value.GetType(), value);
        }

        /// <inheritdoc/>
        public bool Remove(T value)
        {
            value.ThrowIfNull(nameof(value));
            return _dictionary.Remove(value.GetType());
        }

        /// <summary>
        /// Removes the value of the specified type from the dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if the value has been removed succesfully; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key" /> is <c>null</c>.</exception>
        public bool Remove(Type key)
            => _dictionary.Remove(key);

        /// <inheritdoc/>
        public TResult GetValue<TResult>() where TResult : T
        {
            try
            {
                return (TResult)_dictionary[typeof(TResult)];
            }
            catch (KeyNotFoundException ex)
            {
                throw new TypeNotFoundException(Properties.Resources.TypeDictionary_TypeNotFound.FormatString(typeof(TResult).FullName), ex);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<TResult> GetAllValues<TResult>() where TResult : T
        {
            if (_dictionary.TryGetValue(typeof(TResult), out var result))
                yield return (TResult)result;
            yield break;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public bool ContainsKey(Type key) => _dictionary.ContainsKey(key);

        /// <inheritdoc/>
        public bool ContainsValue(T value) => _dictionary.ContainsValue(value.ThrowIfNull(nameof(value)));

        /// <inheritdoc/>
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

        ICloneable ICloneable.Clone() => Clone();
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
    }
}
