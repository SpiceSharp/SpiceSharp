using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.General
{
    /// <summary>
    /// An implementation of the <see cref="ITypeDictionary{V}"/> interface that
    /// allows retrieving information using interface types.
    /// </summary>
    /// <typeparam name="V">The value type.</typeparam>
    /// <seealso cref="ITypeDictionary{V}" />
    public class InterfaceTypeDictionary<V> : ITypeDictionary<V>
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
        public V this[Type key]
        {
            get
            {
                key.ThrowIfNull(nameof(key));
                if (_interfaces.TryGetValue(key, out var result))
                    return result.Value;
                return _dictionary[key];
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterfaceTypeDictionary{V}"/> class.
        /// </summary>
        public InterfaceTypeDictionary()
        {
            _dictionary = [];
            _interfaces = [];
        }

        /// <summary>
        /// Adds a value to the dictionary.
        /// </summary>
        /// <param name="key">The key type.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is <c>null</c>.</exception>
        public void Add(Type key, V value)
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
        public bool Remove(Type key, V value)
        {
            key.ThrowIfNull(nameof(key));
            if (_dictionary.TryGetValue(key, out var existing))
            {
                if (!existing.Equals(value))
                    return false;
                foreach (var type in InterfaceCache.Get(key))
                    _interfaces[type].Remove(value);
                _dictionary.Remove(key);
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public IEnumerable<V> GetAllValues(Type key)
        {
            key.ThrowIfNull(nameof(key));
            if (_interfaces.TryGetValue(key, out var result))
                return result.Values;
            return Enumerable.Empty<V>();
        }

        /// <inheritdoc/>
        public int GetValueCount(Type key)
        {
            key.ThrowIfNull(nameof(key));
            if (_interfaces.TryGetValue(key, out var result))
                return result.Count;
            return 0;
        }

        /// <inheritdoc/>
        public bool TryGetValue(Type key, out V value)
        {
            key.ThrowIfNull(nameof(key));
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
        public bool ContainsKey(Type key) => _interfaces.ContainsKey(key.ThrowIfNull(nameof(key))) || _dictionary.ContainsKey(key);

        /// <inheritdoc/>
        public bool Contains(V value) => _dictionary.ContainsValue(value);

        /// <inheritdoc/>
        public void Clear()
        {
            _interfaces.Clear();
            _dictionary.Clear();
        }
    }
}
