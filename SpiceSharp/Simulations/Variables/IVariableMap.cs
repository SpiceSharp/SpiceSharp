using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A template for mapping a variable to indices.
    /// </summary>
    /// <seealso cref="IEnumerable{T}" />
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
        IVariable this[int index] { get; }

        /// <summary>
        /// Determines whether a variable is mapped.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>
        ///   <c>true</c> if the variable is mapped; otherwise, <c>false</c>.
        /// </returns>
        bool Contains(IVariable variable);
    }
}
