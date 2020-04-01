using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A template for a dictionary of strongly typed variables.
    /// </summary>
    /// <typeparam name="V">The variable type.</typeparam>
    /// <seealso cref="IReadOnlyDictionary{K, V}" />
    /// <remarks>
    /// This can be used to map variables into a solver.
    /// </remarks>
    public interface IVariableDictionary<V> : IReadOnlyDictionary<string, V> where V : IVariable
    {
        /// <summary>
        /// Gets the comparer used for comparing variable names.
        /// </summary>
        /// <value>
        /// The comparer.
        /// </value>
        IEqualityComparer<string> Comparer { get; }

        /// <summary>
        /// Adds a variable to the dictionary.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="variable">The variable.</param>
        void Add(string id, V variable);
    }
}
