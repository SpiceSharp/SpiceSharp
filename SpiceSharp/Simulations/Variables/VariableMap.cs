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
        private readonly Dictionary<IVariable, int> _map = [];

        /// <inheritdoc/>
        public int Count => _map.Count;

        /// <inheritdoc/>
        public int this[IVariable variable] => _map[variable];

        /// <inheritdoc/>
        public IVariable this[int index]
        {
            get
            {
                var result = _map.FirstOrDefault(p => p.Value == index);
                if (result.Equals(default(IVariable)))
                    throw new ArgumentException(Properties.Resources.VariableNotFound.FormatString(index));
                return result.Key;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableMap"/> class.
        /// </summary>
        /// <param name="ground">The ground variable that receives index 0.</param>
        public VariableMap(IVariable ground)
        {
            _map.Add(ground, 0);
        }

        /// <inheritdoc/>
        public bool Contains(IVariable variable) => _map.ContainsKey(variable);

        /// <summary>
        /// Adds the specified variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="index">The index.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is not strictly positive.</exception>
        /// <exception cref="ArgumentException">Thrown if a variable already exists with the same index.</exception>
        public void Add(IVariable variable, int index)
        {
            variable.ThrowIfNull(nameof(variable));
            index.GreaterThan(nameof(index), 0);
            try
            {
                _map.Add(variable, index);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException(Properties.Resources.VariableMap_KeyExists.FormatString(variable));
            }
        }

        /// <summary>
        /// Clears the map.
        /// </summary>
        public void Clear() => _map.Clear();

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
