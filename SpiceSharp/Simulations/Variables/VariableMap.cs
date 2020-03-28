using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A default implementation for a variable map.
    /// </summary>
    /// <remarks>
    /// Can be used to map to indices for a solver that uses matrix equations.
    /// </remarks>
    public class VariableMap : IVariableMap
    {
        private readonly IVariable _ground;
        private readonly Dictionary<IVariable, int> _map = new Dictionary<IVariable, int>();

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
        public int this[IVariable variable] => _map[variable];

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
        public IVariable this[int index] => _map.First(p => p.Value == index).Key;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableMap"/> class.
        /// </summary>
        /// <param name="ground">The ground variable.</param>
        public VariableMap(IVariable ground)
        {
            _ground = ground.ThrowIfNull(nameof(ground));
            _map.Add(_ground, 0);
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
        /// Adds the specified variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="index">The index.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown if the index is not strictly positive.</exception>
        public void Add(IVariable variable, int index)
        {
            if (index <= 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            _map.Add(variable, index);
        }

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
