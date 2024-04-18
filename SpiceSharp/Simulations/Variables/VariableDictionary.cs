using System;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// An implementation of the <see cref="IVariableDictionary{V}"/>.
    /// </summary>
    /// <typeparam name="V">The variable type.</typeparam>
    /// <seealso cref="IVariableDictionary{V}" />
    /// <seealso cref="IVariable"/>
    public class VariableDictionary<V> : IVariableDictionary<V> where V : IVariable
    {
        private readonly Dictionary<string, V> _map = [];

        /// <inheritdoc/>
        public IEqualityComparer<string> Comparer => _map.Comparer;

        /// <summary>
        /// Gets the number of variables in the dictionary.
        /// </summary>
        /// <value>
        /// The number of variables in the dictionary.
        /// </value>
        public int Count => _map.Count;

        /// <summary>
        /// Gets an enumerable collection that contains the identifiers of all variables in the set.
        /// </summary>
        /// <value>
        /// An enumerable of all the identifiers in the dictionary.
        /// </value>
        public IEnumerable<string> Keys => _map.Keys;

        /// <summary>
        /// Gets an enumerable collection that contains the variables in the set.
        /// </summary>
        /// <value>
        /// An enumerable of all the variables in the dictionary.
        /// </value>
        public IEnumerable<V> Values => _map.Values;

        /// <summary>
        /// Gets the <see cref="IVariable"/> with the specified identifier.
        /// </summary>
        /// <value>
        /// The <see cref="IVariable"/>.
        /// </value>
        /// <param name="name">The name.</param>
        /// <returns>The variable with the specified name.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the variable couldn't be found.</exception>
        public V this[string name]
        {
            get
            {
                name.ThrowIfNull(nameof(name));
                if (_map.TryGetValue(name, out var node))
                    return node;
                throw new KeyNotFoundException(Properties.Resources.VariableNotFound.FormatString(name));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableDictionary{V}"/> class.
        /// </summary>
        public VariableDictionary()
            : this(Constants.DefaultComparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableDictionary{V}"/> class.
        /// </summary>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing variable names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        public VariableDictionary(IEqualityComparer<string> comparer)
        {
            _map = new Dictionary<string, V>(comparer ?? Constants.DefaultComparer);
        }

        /// <inheritdoc/>
        public void Add(string id, V variable)
        {
            id.ThrowIfNull(nameof(id));
            try
            {
                _map.Add(id, variable);
            }
            catch (ArgumentException ex)
            {
                // supply our own
                throw new ArgumentException(Properties.Resources.VariableDictionary_KeyExists.FormatString(id), ex);
            }
        }

        /// <summary>
        /// Determines whether the dictionary contains an element that has the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>
        /// <c>true</c> if the read-only dictionary contains an element that has the specified key; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is <c>null</c>.</exception>
        public bool ContainsKey(string key) => _map.ContainsKey(key);

        /// <summary>
        /// Gets the value that is associated with the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns>
        /// <c>true</c> if the dictionary contains an element that has the specified key; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is <c>null</c>.</exception>
        public bool TryGetValue(string key, out V value) => _map.TryGetValue(key, out value);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<string, V>> GetEnumerator() => _map.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
