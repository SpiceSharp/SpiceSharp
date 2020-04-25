using System;
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
    /// <seealso cref="IReadOnlyDictionary{TKey, TValue}"/>
    /// <seealso cref="IVariable"/>
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
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="id"/> or <paramref name="variable"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if a variable with the same identifier already exists.</exception>
        void Add(string id, V variable);
    }
}
