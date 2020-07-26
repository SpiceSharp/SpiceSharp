using System;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp.General
{
    /// <summary>
    /// An implementation of the <see cref="ITypeDictionary{K,V}"/> interface.
    /// This implementation supports multithreaded access.
    /// </summary>
    /// <typeparam name="K">The base key type.</typeparam>
    /// <typeparam name="V">The base value type.</typeparam>
    /// <seealso cref="ITypeDictionary{K,V}" />
    public class TypeDictionary<K, V> : ITypeDictionary<K, V>
    {
        private readonly Dictionary<Type, V> _dictionary;

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
        public IEnumerable<V> Values => _dictionary.Values;

        /// <inheritdoc/>
        public int Count => _dictionary.Count;

        /// <inheritdoc/>
        public V this[Type key] => _dictionary[key];

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeDictionary{K,V}" /> class.
        /// </summary>
        public TypeDictionary()
        {
            _dictionary = new Dictionary<Type, V>();
        }

        /// <inheritdoc/>
        public void Add<Key>(V value) where Key : K
        {
            value.ThrowIfNull(nameof(value));
            _dictionary.Add(value.GetType(), value);
        }

        /// <inheritdoc/>
        public bool Remove<Key>() where Key : K => _dictionary.Remove(typeof(Key));

        /// <summary>
        /// Removes the value of the specified type from the dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if the value has been removed succesfully; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key" /> is <c>null</c>.</exception>
        public bool Remove(Type key) => _dictionary.Remove(key);

        /// <inheritdoc/>
        public V GetValue<Key>() where Key : K
        {
            try
            {
                return _dictionary[typeof(Key)];
            }
            catch (KeyNotFoundException ex)
            {
                throw new TypeNotFoundException(Properties.Resources.TypeDictionary_TypeNotFound.FormatString(typeof(Key).FullName), ex);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<V> GetAllValues<Key>() where Key : K
        {
            if (_dictionary.TryGetValue(typeof(Key), out var result))
                yield return result;
            yield break;
        }

        /// <inheritdoc/>
        public bool TryGetValue<Key>(out V value) where Key : K
        {
            if (_dictionary.TryGetValue(typeof(Key), out var result))
            {
                value = result;
                return true;
            }
            value = default;
            return false;
        }

        /// <inheritdoc/>
        public bool ContainsKey<Key>() where Key : K => _dictionary.ContainsKey(typeof(Key));

        /// <inheritdoc/>
        public bool ContainsKey(Type key) => _dictionary.ContainsKey(key.ThrowIfNull(nameof(key)));

        /// <inheritdoc/>
        public bool ContainsValue(V value) => _dictionary.ContainsValue(value.ThrowIfNull(nameof(value)));

        /// <inheritdoc/>
        public bool TryGetValue(Type key, out V value)
        {
            key.ThrowIfNull(nameof(key));
            return _dictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Removes all items from the <see cref="TypeDictionary{K,V}"/>.
        /// </summary>
        public void Clear() => _dictionary.Clear();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<Type, V>> GetEnumerator() => _dictionary.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();

        /// <inheritdoc/>
        ICloneable ICloneable.Clone() => Clone();

        /// <inheritdoc/>
        void ICloneable.CopyFrom(ICloneable source) => CopyFrom(source);

        /// <summary>
        /// Clones the instance.
        /// </summary>
        /// <returns>
        /// The cloned instance.
        /// </returns>
        protected virtual ICloneable Clone()
        {
            var clone = new TypeDictionary<K, V>();
            foreach (var pair in _dictionary)
            {
                if (pair.Value is ICloneable cloneable)
                    clone._dictionary.Add(pair.Key, (V)cloneable.Clone());
                else
                    clone._dictionary.Add(pair.Key, pair.Value);
            }
            return clone;
        }

        /// <summary>
        /// Copies the contents of one interface to this one.
        /// </summary>
        /// <param name="source">The source parameter.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <c>null</c>.</exception>
        protected virtual void CopyFrom(ICloneable source)
        {
            var src = (TypeDictionary<K, V>)source.ThrowIfNull(nameof(source));
            _dictionary.Clear();
            foreach (var pair in src._dictionary)
                _dictionary.Add(pair.Key, pair.Value);
        }
    }
}
