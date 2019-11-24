using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace SpiceSharp.General
{
    /// <summary>
    /// An <see cref="ITypeDictionary{T}"/> that tracks inheritance and interfaces.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    /// <seealso cref="ITypeDictionary{T}" />
    public class InheritedTypeDictionary<T> : ITypeDictionary<T>
    {
        private readonly Dictionary<Type, InheritanceNode<T>> _dictionary;
        private readonly HashSet<T> _values;

        /// <summary>
        /// Gets the value of the specified type.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public T this[Type key]
        {
            get
            {
                var result = _dictionary[key];
                if (result.NextSibling != null)
                    throw new AmbiguousTypeException(key);
                return result.Value;
            }
        }

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
            _dictionary = new Dictionary<Type, InheritanceNode<T>>();
            _values = new HashSet<T>();
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
            var node = new InheritanceNode<T>(value, true);

            // We should always be able to access the type by itself, so remove any ambiguous elements if necessary
            if (_dictionary.TryGetValue(ctype, out var existing))
            {
                if (existing.IsDirect)
                    throw new CircuitException("A value of type '{0}' already exists".FormatString(ctype.FullName));
                node.NextSibling = existing;
                _dictionary[ctype] = node;
            }
            else
                _dictionary.Add(ctype, node);
            _values.Add(node.Value);

            // Track the inheritance tree
            ctype = ctype.GetTypeInfo().BaseType;
            while (ctype != null && ctype != typeof(object) && ctype != typeof(T))
            {
                if (_dictionary.TryGetValue(ctype, out existing))
                {
                    // There is already a node that is associated to this type, let's add it as a sibling
                    var elt = existing;
                    while (elt.NextSibling != null)
                        elt = elt.NextSibling;
                    elt.NextSibling = new InheritanceNode<T>(value, false);
                }
                else
                    _dictionary.Add(ctype, new InheritanceNode<T>(value, false));
                ctype = ctype.GetTypeInfo().BaseType;
            }

            // Make references for the interfaces as well
            var ifs = value.GetType().GetTypeInfo().GetInterfaces();
            foreach (var type in ifs)
            {
                if (_dictionary.TryGetValue(type, out existing))
                {
                    // There is already a node that is associated to this interface, let's add it as a sibling
                    var elt = existing;
                    while (elt.NextSibling != null)
                        elt = elt.NextSibling;
                    elt.NextSibling = new InheritanceNode<T>(value, false);
                }
                else
                    _dictionary.Add(type, new InheritanceNode<T>(value, false));
            }
            _values.Add(value);
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
            if (!_values.Contains(value))
                return false;
            var ctype = value.GetType();
            RemoveValueFromList(value, ctype);
            _values.Remove(value);

            // Track the inheritance tree
            ctype = ctype.GetTypeInfo().BaseType;
            while (ctype != null && ctype != typeof(object) && ctype != typeof(T))
            {
                RemoveValueFromList(value, ctype);
                ctype = ctype.GetTypeInfo().BaseType;
            }

            // Make references for the interfaces as well
            var ifs = value.GetType().GetTypeInfo().GetInterfaces();
            foreach (var type in ifs)
                RemoveValueFromList(value, type);
            _values.Add(value);
            return true;
        }

        /// <summary>
        /// Removes the value from the list.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="key">The key.</param>
        private void RemoveValueFromList(T value, Type key)
        {
            var first = _dictionary[key];
            if (first.Value.Equals(value))
            {
                if (first.NextSibling != null)
                    _dictionary[key] = first.NextSibling;
                else
                    _dictionary.Remove(key);
            }
            else
            {
                var lastElt = first;
                var elt = first.NextSibling;
                while (!elt.Value.Equals(value))
                {
                    lastElt = elt;
                    elt = elt.NextSibling;
                }
                lastElt.NextSibling = elt.NextSibling;
            }
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
                var cloneValue = pair.Value.Clone();
                clone._dictionary.Add(pair.Key, cloneValue);
                var elt = cloneValue;
                while (elt != null)
                {
                    clone._values.Add(elt.Value);
                    elt = elt.NextSibling;
                }
            }
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
            var src = (InheritedTypeDictionary<T>)source;
            foreach (var pair in src._dictionary)
            {
                var srcNode = pair.Value;
                var newNode = new InheritanceNode<T>(srcNode.Value, srcNode.IsDirect);
                _dictionary.Add(pair.Key, newNode);
                while (srcNode.NextSibling != null)
                {
                    srcNode = srcNode.NextSibling;
                    newNode.NextSibling = new InheritanceNode<T>(srcNode.Value, srcNode.IsDirect);
                    newNode = newNode.NextSibling;
                }
            }
            foreach (var value in src._values)
                _values.Add(value);
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
            {
                if (result.NextSibling != null && !result.IsDirect)
                    throw new AmbiguousTypeException(typeof(TResult));
                return (TResult)result.Value;
            }
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
                if (result.NextSibling != null && !result.IsDirect)
                    throw new AmbiguousTypeException(typeof(TResult));
                value = (TResult)result.Value;
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
        /// <returns>
        /// <c>true</c> if the value was resolved; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetValue(Type key, out T value)
        {
            key.ThrowIfNull(nameof(key));
            if (_dictionary.TryGetValue(key, out var result))
            {
                if (result.NextSibling != null && !result.IsDirect)
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
