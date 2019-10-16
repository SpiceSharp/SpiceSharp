using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A template for mapping a variable to indices.
    /// </summary>
    /// <seealso cref="IEnumerable{T}" />
    public interface IVariableMap : IEnumerable<KeyValuePair<Variable, int>>
    {
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
        int this[Variable variable] { get; }

        /// <summary>
        /// Gets all the variables in the map.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        IEnumerable<Variable> Variables { get; }

        /// <summary>
        /// Determines whether a variable is mapped.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>
        ///   <c>true</c> if the variable is mapped; otherwise, <c>false</c>.
        /// </returns>
        bool Contains(Variable variable);

        /// <summary>
        /// Clears the map.
        /// </summary>
        void Clear();
    }
}
