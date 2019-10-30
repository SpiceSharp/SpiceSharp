using System;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A class that maps variables to indices.
    /// </summary>
    /// <remarks>
    /// Can be used to map to indices for a solver that uses matrix equations.
    /// </remarks>
    public class VariableMap : IVariableMap
    {
        /// <summary>
        /// The map of variables.
        /// </summary>
        /// <remarks>
        /// We just want to search by reference!
        /// </remarks>
        private Dictionary<Variable, int> _map = new Dictionary<Variable, int>();

        /// <summary>
        /// Gets the ground node.
        /// </summary>
        /// <value>
        /// The ground.
        /// </value>
        public Variable Ground { get; }

        /// <summary>
        /// Gets the number of mapped variables.
        /// </summary>
        /// <value>
        /// The number of mapped variables.
        /// </value>
        public int Count => _map.Count;

        /// <summary>
        /// Gets the index associated with the specified variable.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        /// <param name="variable">The variable.</param>
        /// <returns>
        /// The variable index.
        /// </returns>
        public int this[Variable variable]
        {
            get
            {
                if (variable == null)
                    throw new ArgumentNullException(nameof(variable));
                if (!_map.TryGetValue(variable, out var index))
                {
                    index = _map.Count;
                    _map.Add(variable, index);
                }
                return index;
            }
        }

        /// <summary>
        /// Gets all the variables in the map.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public IEnumerable<Variable> Variables => _map.Keys;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableMap"/> class.
        /// </summary>
        /// <param name="ground">The ground variable.</param>
        public VariableMap(Variable ground)
        {
            Ground = ground.ThrowIfNull(nameof(ground));
            _map.Add(Ground, 0);
        }

        /// <summary>
        /// Determines whether a variable is mapped.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>
        ///   <c>true</c> if the variable is mapped; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(Variable variable) => _map.ContainsKey(variable);

        /// <summary>
        /// Tries to get the associated index of the specified variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="index">The associated index.</param>
        /// <returns>
        ///   <c>true</c> if the variable has been found; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetIndex(Variable variable, out int index)
            => _map.TryGetValue(variable, out index);

        /// <summary>
        /// Clears the map.
        /// </summary>
        public void Clear()
        {
            _map.Clear();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<Variable, int>> GetEnumerator() => _map.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
