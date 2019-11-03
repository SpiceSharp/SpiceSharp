using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A template for a set of variables.
    /// </summary>
    public interface IVariableSet : IEnumerable<Variable>
    {
        /// <summary>
        /// Gets the number of variables.
        /// </summary>
        /// <value>
        /// The number of variables.
        /// </value>
        int Count { get; }

        /// <summary>
        /// Gets the ground variable.
        /// </summary>
        /// <value>
        /// The ground.
        /// </value>
        Variable Ground { get; }

        /// <summary>
        /// Gets the comparer used for variables.
        /// </summary>
        /// <value>
        /// The comparer.
        /// </value>
        IEqualityComparer<string> Comparer { get; }

        /// <summary>
        /// Gets the <see cref="Variable"/> with the specified identifier.
        /// </summary>
        /// <value>
        /// The <see cref="Variable"/>.
        /// </value>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        Variable this[string id] { get; }

        /// <summary>
        /// This method maps a variable in the circuit. If a variable with the same identifier already exists, then that variable is returned.
        /// </summary>
        /// <remarks>
        /// If the variable already exists, the variable type is ignored.
        /// </remarks>
        /// <param name="id">The identifier of the variable.</param>
        /// <param name="type">The type of the variable.</param>
        /// <returns>A new variable with the specified identifier and type, or a previously mapped variable if it already existed.</returns>
        Variable MapNode(string id, VariableType type);

        /// <summary>
        /// Creates a new variable.
        /// </summary>
        /// <remarks>
        /// Variables created using this method cannot be found back using the method <see cref="MapNode(string,VariableType)"/>.
        /// </remarks>
        /// <param name="id">The identifier of the new variable.</param>
        /// <param name="type">The type of the variable.</param>
        /// <returns>A new variable.</returns>
        Variable Create(string id, VariableType type);

        /// <summary>
        /// Determines whether the set contains a mapped variable by a specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        ///   <c>true</c> if the specified set contains the variable; otherwise, <c>false</c>.
        /// </returns>
        bool ContainsNode(string id);

        /// <summary>
        /// Determines whether the set contains any variable by a specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        ///   <c>true</c> if the set contains the variable; otherwise, <c>false</c>.
        /// </returns>
        bool Contains(string id);

        /// <summary>
        /// Make an alias for a variable identifier.
        /// </summary>
        /// <remarks>
        /// This basically gives two names to the same variable. This can be used for example to make multiple identifiers
        /// point to the ground node.
        /// </remarks>
        /// <param name="original">The original identifier.</param>
        /// <param name="alias">The alias for the identifier.</param>
        void AliasNode(string original, string alias);

        /// <summary>
        /// Tries to get a variable.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="node">The found variable.</param>
        /// <returns>
        ///   <c>true</c> if the variable was found; otherwise <c>false</c>.
        /// </returns>
        bool TryGetNode(string id, out Variable node);

        /// <summary>
        /// Clears the set from any variables.
        /// </summary>
        void Clear();
    }
}
