using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.General
{
    /// <summary>
    /// An <see cref="ITypeDictionary{K,V}"/> that tracks both inheritance and implemented interfaces.
    /// </summary>
    /// <typeparam name="K">The base key type.</typeparam>
    /// <typeparam name="V">The base value type.</typeparam>
    /// <seealso cref="ITypeDictionary{K,V}" />
    public class InheritedTypeDictionary<K, V> : ITypeDictionary<K, V>
    {
        private readonly Dictionary<Type, TypeValues<V>> _dictionary;
        private readonly HashSet<V> _values;

        /// <inheritdoc/>
        public V this[Type key]
        {
            get
            {
                var result = _dictionary[key];
                if (result.IsAmbiguous)
                    throw new AmbiguousTypeException(key);
                return result.Value;
            }
        }

        /// <inheritdoc/>
        public int Count => _values.Count;

        /// <inheritdoc/>
        public IEnumerable<Type> Keys => _dictionary.Keys;

        /// <inheritdoc/>
        public IEnumerable<V> Values => _values;

        /// <summary>
        /// Initializes a new instance of the <see cref="InheritedTypeDictionary{K,V}"/> class.
        /// </summary>
        public InheritedTypeDictionary()
        {
            _dictionary = new Dictionary<Type, TypeValues<V>>();
            _values = new HashSet<V>();
        }

        /// <inheritdoc/>
        public void Add<Key>(V value) where Key : K => Add(typeof(Key), value);

        /// <summary>
        /// Adds a value with the specified key.
        /// </summary>
        /// <param name="key">The key type.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is <c>null</c>.</exception>
        protected void Add(Type key, V value)
        {
            key.ThrowIfNull(nameof(key));

            // We should always be able to access the type by itself, so remove any ambiguous elements if necessary
            if (_dictionary.TryGetValue(key, out var values))
            {
                if (values.IsDirect)
                    throw new ArgumentException(Properties.Resources.TypeAlreadyExists.FormatString(key.FullName));
            }
            else
            {
                values = new TypeValues<V>();
                _dictionary.Add(key, values);
            }
            values.Add(value, true);
            _values.Add(value);

            foreach (var type in InheritanceCache.Get(key).Union(InterfaceCache.Get(key)))
            {
                if (!_dictionary.TryGetValue(type, out values))
                {
                    values = new TypeValues<V>();
                    _dictionary.Add(type, values);
                }
                values.Add(value);
            }
        }

        /// <inheritdoc/>
        public bool Remove<Key>() where Key : K
        {
            if (_dictionary.TryGetValue(typeof(Key), out var values))
            {
                foreach (var value in values.Values)
                {
                    _values.Remove(value);
                    foreach (var type in InheritanceCache.Get(typeof(Key)).Union(InterfaceCache.Get(typeof(Key))))
                        _dictionary[type].Remove(value);
                }
                _dictionary.Remove(typeof(Key));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes only the specified value with the specified key from the dictionary.
        /// </summary>
        /// <typeparam name="Key">The key type.</typeparam>
        /// <param name="value">The value that needs to be removed.</param>
        /// <returns>
        ///     <c>true</c> if the value was removed; otherwise <c>false</c>.
        /// </returns>
        public bool Remove<Key>(V value) where Key : K
        {
            if (_dictionary.TryGetValue(typeof(Key), out var values))
            {
                foreach (var type in InheritanceCache.Get(typeof(Key)).Union(InterfaceCache.Get(typeof(Key))))
                    _dictionary[type].Remove(value);
                return values.Remove(value);
            }
            return false;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _dictionary.Clear();
            _values.Clear();
        }

        /// <inheritdoc/>
        public bool ContainsKey<Key>() where Key : K => _dictionary.ContainsKey(typeof(Key));

        /// <inheritdoc/>
        public bool ContainsKey(Type key) => _dictionary.ContainsKey(key);

        /// <inheritdoc/>
        public bool ContainsValue(V value) => _values.Contains(value.ThrowIfNull(nameof(value)));

        /// <inheritdoc/>
        ICloneable ICloneable.Clone()
        {
            var clone = new InheritedTypeDictionary<K, V>();
            foreach (var pair in _dictionary)
            {
                // Only add direct elements
                if (pair.Value.IsDirect)
                {
                    var cloned = pair.Value.Value;
                    if (cloned is ICloneable cloneable)
                        cloned = (V)cloneable.Clone();
                    clone.Add(pair.Key, cloned);
                }
            }
            return clone;
        }

        /// <inheritdoc/>
        void ICloneable.CopyFrom(ICloneable source)
        {
            _dictionary.Clear();
            _values.Clear();
            var src = (InheritedTypeDictionary<K, V>)source.ThrowIfNull(nameof(source));
            foreach (var pair in src._dictionary)
            {
                if (pair.Value.IsDirect)
                    Add(pair.Key, pair.Value.Value);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<Type, V>> GetEnumerator()
        {
            foreach (var elt in _dictionary)
                yield return new KeyValuePair<Type, V>(elt.Key, elt.Value.Value);
        }

        /// <inheritdoc/>
        public V GetValue<Key>() where Key : K
        {
            if (_dictionary.TryGetValue(typeof(Key), out var result))
            {
                if (result.IsAmbiguous)
                    throw new AmbiguousTypeException(typeof(Key));
                return result.Value;
            }
            throw new TypeNotFoundException(Properties.Resources.TypeDictionary_TypeNotFound.FormatString(typeof(Key).FullName));
        }

        /// <inheritdoc/>
        public IEnumerable<V> GetAllValues<Key>() where Key : K
        {
            if (_dictionary.TryGetValue(typeof(Key), out var result))
                return result.Values.Cast<V>();
            return Enumerable.Empty<V>();
        }

        /// <inheritdoc/>
        public bool TryGetValue<Key>(out V value) where Key : K
        {
            if (_dictionary.TryGetValue(typeof(Key), out var result))
            {
                if (result.IsAmbiguous)
                    throw new AmbiguousTypeException(typeof(Key));
                value = result.Value;
                return true;
            }
            value = default;
            return false;
        }

        /// <inheritdoc/>
        public bool TryGetValue(Type key, out V value)
        {
            key.ThrowIfNull(nameof(key));
            if (_dictionary.TryGetValue(key, out var result))
            {
                if (result.IsAmbiguous)
                    throw new AmbiguousTypeException(key);
                value = result.Value;
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
