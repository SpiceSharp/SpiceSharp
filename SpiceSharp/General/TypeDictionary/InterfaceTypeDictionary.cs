using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.General
{
    /// <summary>
    /// An implementation of the <see cref="ITypeDictionary{K,V}"/> interface that
    /// allows retrieving information using interface types.
    /// </summary>
    /// <typeparam name="K">The base type for keys.</typeparam>
    /// <typeparam name="V">The base type for values.</typeparam>
    /// <seealso cref="ITypeDictionary{K,V}" />
    public class InterfaceTypeDictionary<K, V> : ITypeDictionary<K, V>
    {
        private readonly Dictionary<Type, V> _dictionary;
        private readonly Dictionary<Type, TypeValues<V>> _interfaces;

        /// <inheritdoc/>
        public IEnumerable<Type> Keys => _interfaces.Keys;

        /// <inheritdoc/>
        public IEnumerable<V> Values => _dictionary.Values;

        /// <inheritdoc/>
        public int Count => _dictionary.Count;

        /// <inheritdoc/>
        public V this[Type type]
        {
            get
            {
                if (_interfaces.TryGetValue(type, out var result))
                    return result.Value;
                return _dictionary[type];
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterfaceTypeDictionary{K,V}"/> class.
        /// </summary>
        public InterfaceTypeDictionary()
        {
            _dictionary = new Dictionary<Type, V>();
            _interfaces = new Dictionary<Type, TypeValues<V>>();
        }

        /// <inheritdoc/>
        public void Add<Key>(V value) where Key : K => Add(typeof(Key), value);

        /// <summary>
        /// Adds a value to the dictionary.
        /// </summary>
        /// <param name="key">The key type.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is <c>null</c>.</exception>
        protected void Add(Type key, V value)
        {
            key.ThrowIfNull(nameof(key));

            // Add a regular class entry
            try
            {
                _dictionary.Add(key, value);
            }
            catch (ArgumentException)
            {
                // This exception is thrown if the dictonary already contains the key
                // Just make it bit more verbose.
                throw new ArgumentException(Properties.Resources.TypeAlreadyExists.FormatString(key));
            }

            // Make references for the interfaces as well
            foreach (var ctype in InterfaceCache.Get(key))
            {
                if (!_interfaces.TryGetValue(ctype, out var values))
                {
                    values = new TypeValues<V>();
                    _interfaces.Add(ctype, values);
                }
                values.Add(value);
            }
        }

        /// <inheritdoc/>
        public bool Remove<Key>() where Key : K
        {
            if (_dictionary.TryGetValue(typeof(Key), out var value))
            {
                foreach (var type in InterfaceCache.Get(typeof(Key)))
                    _interfaces[type].Remove(value);
                _dictionary.Remove(typeof(Key));
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public V GetValue<Key>() where Key : K
        {
            if (_interfaces.TryGetValue(typeof(Key), out var result))
            {
                if (result.IsAmbiguous)
                    throw new AmbiguousTypeException(typeof(Key));
                return result.Value;
            }
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
            if (_interfaces.TryGetValue(typeof(Key), out var result))
                return result.Values.Cast<V>();
            return Enumerable.Empty<V>();
        }

        /// <inheritdoc/>
        public bool TryGetValue<Key>(out V value) where Key : K
        {
            if (_interfaces.TryGetValue(typeof(Key), out var result))
            {
                if (result.IsAmbiguous)
                    throw new AmbiguousTypeException(typeof(Key));
                value = result.Value;
                return true;
            }
            if (_dictionary.TryGetValue(typeof(Key), out var direct))
            {
                value = direct;
                return true;
            }
            value = default;
            return false;
        }

        /// <inheritdoc/>
        public bool TryGetValue(Type key, out V value)
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

        /// <inheritdoc/>
        public bool ContainsKey<Key>() where Key : K => _interfaces.ContainsKey(typeof(Key)) || _dictionary.ContainsKey(typeof(Key));

        /// <inheritdoc/>
        public bool ContainsKey(Type key) => _interfaces.ContainsKey(key) || _dictionary.ContainsKey(key);

        /// <inheritdoc/>
        public bool ContainsValue(V value) => _dictionary.ContainsValue(value);

        /// <inheritdoc/>
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
        public IEnumerator<KeyValuePair<Type, V>> GetEnumerator()
        {
            foreach (var pair in _interfaces)
                yield return new KeyValuePair<Type, V>(pair.Key, pair.Value.Value);
            foreach (var pair in _dictionary)
                yield return new KeyValuePair<Type, V>(pair.Key, pair.Value);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        ICloneable ICloneable.Clone()
        {
            var clone = new InterfaceTypeDictionary<K, V>();
            foreach (var pair in _dictionary)
            {
                var cloneValue = pair.Value;
                if (pair.Value is ICloneable cloneable)
                    cloneValue = (V)cloneable.Clone();
                clone.Add(pair.Key, cloneValue);
            }
            return clone;
        }

        /// <inheritdoc/>
        void ICloneable.CopyFrom(ICloneable source)
        {
            _dictionary.Clear();
            _interfaces.Clear();
            var src = (InterfaceTypeDictionary<K, V>)source.ThrowIfNull(nameof(source));
            foreach (var pair in src._dictionary)
                Add(pair.Key, pair.Value);
        }
    }
}
