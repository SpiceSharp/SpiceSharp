using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A template for a set of strongly typed variables.
    /// </summary>
    /// <remarks>
    /// This can be used to map variables into a solver.
    /// </remarks>
    /// <seealso cref="IVariableSet"/>
    /// <seealso cref="IEnumerable{V}"/>
    public interface IVariableSet<V> : IVariableSet, IEnumerable<V> where V : IVariable
    {
        /// <summary>
        /// Gets the variable with the specified name.
        /// </summary>
        /// <value>
        /// The variable.
        /// </value>
        /// <param name="name">The name.</param>
        /// <returns>The variable.</returns>
        new V this[string name] { get; }

        /// <summary>
        /// Determines whether the set contains a variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>
        ///   <c>true</c> if the set contains the variable; otherwise, <c>false</c>.
        /// </returns>
        bool ContainsValue(V variable);

        /// <summary>
        /// Tries to get a variable by its name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="variable">The variable.</param>
        /// <returns>
        /// <c>true</c> if the variable was found; otherwise, <c>false</c>.
        /// </returns>
        bool TryGetValue(string name, out V variable);

        /// <summary>
        /// Adds the specified variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        void Add(V variable);

        /// <summary>
        /// Removes the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// <c>true</c> if the variable was removed; otherwise <c>false</c>.
        /// </returns>
        bool Remove(string name);
    }

    /// <summary>
    /// A template for a set of variables.
    /// </summary>
    /// <seealso cref="IVariableSet" />
    public interface IVariableSet
    {
        /// <summary>
        /// Gets the number of variables in the set.
        /// </summary>
        /// <value>
        /// The number of variables.
        /// </value>
        int Count { get; }

        /// <summary>
        /// Gets the comparer used for comparing variable names.
        /// </summary>
        /// <value>
        /// The comparer.
        /// </value>
        IEqualityComparer<string> Comparer { get; }

        /// <summary>
        /// Gets the variable with the specified name.
        /// </summary>
        /// <value>
        /// The variable.
        /// </value>
        /// <param name="name">The name.</param>
        /// <returns>The variable.</returns>
        IVariable this[string name] { get; }

        /// <summary>
        /// Determines whether set contains a variable with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        ///   <c>true</c> if the set contains a variable with the specified name; otherwise, <c>false</c>.
        /// </returns>
        bool ContainsKey(string name);
    }
}
