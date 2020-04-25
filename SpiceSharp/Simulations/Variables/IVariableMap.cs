using System;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A template for mapping a variable to indices.
    /// </summary>
    /// <seealso cref="IEnumerable{T}" />
    /// <seealso cref="IVariable"/>
    public interface IVariableMap : IEnumerable<KeyValuePair<IVariable, int>>
    {
        /// <summary>
        /// Gets the number of mapped variables.
        /// </summary>
        /// <value>
        /// The number of mapped variables.
        /// </value>
        int Count { get; }

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
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="variable"/> is <c>null</c>.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the variable was not found.</exception>
        int this[IVariable variable] { get; }

        /// <summary>
        /// Gets the variable associated to the specified index.
        /// </summary>
        /// <value>
        /// The variable.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The associated variable.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if no variable was found at the specified index.</exception>
        IVariable this[int index] { get; }

        /// <summary>
        /// Determines whether a variable is mapped.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>
        ///   <c>true</c> if the variable is mapped; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="variable"/> is <c>null</c>.</exception>
        bool Contains(IVariable variable);
    }
}
