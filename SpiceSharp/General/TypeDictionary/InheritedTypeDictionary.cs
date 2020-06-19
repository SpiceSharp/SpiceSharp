using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.General
{
    /// <summary>
    /// An <see cref="ITypeDictionary{T}"/> that tracks inheritance and interfaces.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    /// <seealso cref="ITypeDictionary{T}" />
    public class InheritedTypeDictionary<T> : ITypeDictionary<T>
    {
        private readonly Dictionary<Type, TypeValues<T>> _dictionary;
        private readonly HashSet<T> _values;

        /// <inheritdoc/>
        public T this[Type key]
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
        public IEnumerable<T> Values => _values;

        /// <summary>
        /// Initializes a new instance of the <see cref="InheritedTypeDictionary{T}"/> class.
        /// </summary>
        public InheritedTypeDictionary()
        {
            _dictionary = new Dictionary<Type, TypeValues<T>>();
            _values = new HashSet<T>();
        }

        /// <inheritdoc/>
        public void Add<V>(V value) where V : T
        {
            var ctype = value.ThrowIfNull(nameof(value)).GetType();

            // We should always be able to access the type by itself, so remove any ambiguous elements if necessary
            if (_dictionary.TryGetValue(ctype, out var values))
            {
                if (values.IsDirect)
                    throw new ArgumentException(Properties.Resources.TypeAlreadyExists.FormatString(ctype.FullName));
            }
            else
            {
                values = new TypeValues<T>();
                _dictionary.Add(ctype, values);
            }
            values.Add(value, true);
            _values.Add(value);

            foreach (var type in InheritanceCache.Get(ctype).Union(InterfaceCache.Get(ctype)))
            {
                if (!_dictionary.TryGetValue(type, out values))
                {
                    values = new TypeValues<T>();
                    _dictionary.Add(type, values);
                }
                values.Add(value);
            }
        }

        /// <inheritdoc/>
        public bool Remove(T value)
        {
            if (!_values.Contains(value))
                return false;
            var ctype = value.GetType();
            _dictionary[ctype].Remove(value);
            _values.Remove(value);
            foreach (var type in InheritanceCache.Get(ctype).Union(InterfaceCache.Get(ctype)))
                _dictionary[type].Remove(value);
            return true;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _dictionary.Clear();
            _values.Clear();
        }

        /// <inheritdoc/>
        public bool ContainsKey(Type key) => _dictionary.ContainsKey(key);

        /// <inheritdoc/>
        public bool ContainsValue(T value) => _values.Contains(value.ThrowIfNull(nameof(value)));

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
            _values.Clear();
            var src = (InheritedTypeDictionary<T>)source;
            foreach (var value in src.Values)
                Add(value);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<Type, T>> GetEnumerator()
        {
            foreach (var elt in _dictionary)
                yield return new KeyValuePair<Type, T>(elt.Key, elt.Value.Value);
        }

        /// <inheritdoc/>
        public TResult GetValue<TResult>() where TResult : T
        {
            if (_dictionary.TryGetValue(typeof(TResult), out var result))
            {
                if (result.IsAmbiguous)
                    throw new AmbiguousTypeException(typeof(TResult));
                return (TResult)result.Value;
            }
            throw new TypeNotFoundException(Properties.Resources.TypeDictionary_TypeNotFound.FormatString(typeof(TResult).FullName));
        }

        /// <inheritdoc/>
        public IEnumerable<TResult> GetAllValues<TResult>() where TResult : T
        {
            if (_dictionary.TryGetValue(typeof(TResult), out var result))
                return result.Values.Cast<TResult>();
            return Enumerable.Empty<TResult>();
        }

        /// <inheritdoc/>
        public bool TryGetValue<TResult>(out TResult value) where TResult : T
        {
            if (_dictionary.TryGetValue(typeof(TResult), out var result))
            {
                if (result.IsAmbiguous)
                    throw new AmbiguousTypeException(typeof(TResult));
                value = (TResult)result.Value;
                return true;
            }
            value = default;
            return false;
        }

        /// <inheritdoc/>
        public bool TryGetValue(Type key, out T value)
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
