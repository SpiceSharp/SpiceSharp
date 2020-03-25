using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
        private Dictionary<IVariable, int> _map = new Dictionary<IVariable, int>();

        /// <summary>
        /// Gets the ground node.
        /// </summary>
        /// <value>
        /// The ground.
        /// </value>
        public IVariable Ground { get; }

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
        /// <exception cref="ArgumentNullException">Thrown if the specified variable was null.</exception>
        public int this[IVariable variable]
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
        /// Gets the <see cref="IVariable"/> at assiciated to the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="IVariable"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The associated variable.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is out of range.</exception>
        public IVariable this[int index]
        {
            get
            {
                if (index < 0 || index > Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _map.FirstOrDefault(p => p.Value == index).Key;
            }
        }

        /// <summary>
        /// Gets all the variables in the map.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public IEnumerable<IVariable> Variables => _map.Keys;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableMap"/> class.
        /// </summary>
        /// <param name="ground">The ground variable.</param>
        public VariableMap(IVariable ground)
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
        public bool Contains(IVariable variable) => _map.ContainsKey(variable);

        /// <summary>
        /// Tries to get the associated index of the specified variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="index">The associated index.</param>
        /// <returns>
        ///   <c>true</c> if the variable has been found; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetIndex(IVariable variable, out int index)
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
        public IEnumerator<KeyValuePair<IVariable, int>> GetEnumerator() => _map.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
