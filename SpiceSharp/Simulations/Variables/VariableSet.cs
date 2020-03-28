using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Contains and manages circuit nodes.
    /// </summary>
    public class VariableSet<V> : IVariableSet<V> where V : IVariable
    {
        private readonly Dictionary<string, V> _map = new Dictionary<string, V>();

        /// <summary>
        /// Gets the <see cref="IEqualityComparer{T}"/> that is used to determine equality of keys.
        /// </summary>
        public IEqualityComparer<string> Comparer => _map.Comparer;

        /// <summary>
        /// Gets the number of variables.
        /// </summary>
        public int Count => _map.Count;

        /// <summary>
        /// Gets an enumerable collection that contains the names of all variables in the set.
        /// </summary>
        public IEnumerable<string> Keys => _map.Keys;

        /// <summary>
        /// Gets an enumerable collection that contains the variables in the set.
        /// </summary>
        public IEnumerable<V> Values => _map.Values;

        /// <summary>
        /// Gets the <see cref="IVariable"/> with the specified identifier.
        /// </summary>
        /// <value>
        /// The <see cref="IVariable"/>.
        /// </value>
        /// <param name="name">The name.</param>
        /// <returns>The variable with the specified name.</returns>
        /// <exception cref="VariableNotFoundException">Thrown if the variable wasn't found.</exception>
        public V this[string name]
        {
            get
            {
                if (_map.TryGetValue(name, out var node))
                    return node;
                throw new VariableNotFoundException(name);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableSet{V}"/> class.
        /// </summary>
        public VariableSet()
            : this(EqualityComparer<string>.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableSet{V}"/> class.
        /// </summary>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing variable names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        public VariableSet(IEqualityComparer<string> comparer)
        {
            _map = new Dictionary<string, V>(comparer);
        }

        /// <summary>
        /// Adds the specified variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public void Add(V variable) => _map.Add(variable.Name, variable);

        /// <summary>
        /// Clear all variables.
        /// </summary>
        public void Clear() => _map.Clear();

        /// <summary>
        /// Determines whether the variable by the specified name exists..
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        ///   <c>true</c> if the variable exists; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(string name) => _map.ContainsKey(name);

        /// <summary>
        /// Determines whether the set contains a variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>
        /// <c>true</c> if the set contains the variable; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsValue(IVariable variable)
        {
            variable.ThrowIfNull(nameof(variable));
            if (!_map.TryGetValue(variable.Name, out var result))
                return false;
            return result.Equals(variable);
        }

        /// <summary>
        /// Tries to get the variable with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="variable">The variable.</param>
        /// <returns>
        /// <c>true</c> if the variable exists; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetValue(string name, out V variable) => _map.TryGetValue(name, out variable);

        bool IVariableSet<V>.TryGetValue(string name, out IVariable variable)
        {
            if (_map.TryGetValue(name, out var result))
            {
                variable = result;
                return true;
            }
            variable = default;
            return false;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<V> GetEnumerator() => _map.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
