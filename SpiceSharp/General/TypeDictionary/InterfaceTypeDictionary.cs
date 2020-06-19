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

        /// <inheritdoc/>
        public IEnumerable<Type> Keys => _interfaces.Keys;

        /// <inheritdoc/>
        public IEnumerable<T> Values => _dictionary.Values;

        /// <inheritdoc/>
        public int Count => _dictionary.Count;

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public void Add<V>(V value) where V : T
        {
            // Add a regular class entry
            var ctype = value.ThrowIfNull(nameof(value)).GetType();
            try
            {
                _dictionary.Add(value.GetType(), value);
            }
            catch (ArgumentException)
            {
                // This exception is thrown if the dictonary already contains the key
                // Just make it bit more verbose.
                throw new ArgumentException(Properties.Resources.TypeAlreadyExists.FormatString(value.GetType()));
            }

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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public TResult GetValue<TResult>() where TResult : T
        {
            if (_interfaces.TryGetValue(typeof(TResult), out var result))
            {
                if (result.IsAmbiguous)
                    throw new AmbiguousTypeException(typeof(TResult));
                return (TResult)result.Value;
            }
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
            if (_interfaces.TryGetValue(typeof(TResult), out var result))
                return result.Values.Cast<TResult>();
            return Enumerable.Empty<TResult>();
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public bool ContainsKey(Type key) => _interfaces.ContainsKey(key) || _dictionary.ContainsKey(key);

        /// <inheritdoc/>
        public bool ContainsValue(T value) => _dictionary.TryGetValue(value.GetType(), out var result) && result.Equals(value);

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
