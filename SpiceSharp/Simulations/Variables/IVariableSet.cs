using SpiceSharp.Simulations.Variables;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A template for a set of variables.
    /// </summary>
    public interface IVariableSet : IEnumerable<IVariable>
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
        IVariable Ground { get; }

        /// <summary>
        /// Gets the comparer used for variables.
        /// </summary>
        /// <value>
        /// The comparer.
        /// </value>
        IEqualityComparer<string> Comparer { get; }

        /// <summary>
        /// Gets the <see cref="IVariable"/> with the specified name.
        /// </summary>
        /// <value>
        /// The <see cref="IVariable"/>.
        /// </value>
        /// <param name="id">The name.</param>
        /// <returns>The variable.</returns>
        IVariable this[string id] { get; }

        /// <summary>
        /// This method maps a variable in the circuit. If a variable with the same name already exists, then that variable is returned.
        /// </summary>
        /// <remarks>
        /// If the variable already exists, the variable type is ignored.
        /// </remarks>
        /// <param name="id">The name of the variable.</param>
        /// <param name="units">The unit of the variable.</param>
        /// <returns>A new variable with the specified name and type, or a previously mapped variable if it already existed.</returns>
        IVariable MapNode(string id, IUnit units);

        /// <summary>
        /// Creates a new variable.
        /// </summary>
        /// <remarks>
        /// Variables created using this method cannot be found back using the <see cref="MapNode(string,IUnit)" /> method.
        /// </remarks>
        /// <param name="id">The name of the new variable.</param>
        /// <param name="units">The units of the variable.</param>
        /// <returns>A new variable.</returns>
        IVariable Create(string id, IUnit units);

        /// <summary>
        /// Determines whether the set contains a mapped variable by a specified name.
        /// </summary>
        /// <param name="id">The name.</param>
        /// <returns>
        ///   <c>true</c> if the specified set contains the variable; otherwise, <c>false</c>.
        /// </returns>
        bool ContainsNode(string id);

        /// <summary>
        /// Determines whether the set contains any variable by a specified name.
        /// </summary>
        /// <param name="id">The name.</param>
        /// <returns>
        ///   <c>true</c> if the set contains the variable; otherwise, <c>false</c>.
        /// </returns>
        bool Contains(string id);

        /// <summary>
        /// Make an alias for a variable.
        /// </summary>
        /// <remarks>
        /// This basically gives two names to the same variable. This can be used for example to make multiple names
        /// point to the ground node.
        /// </remarks>
        /// <param name="variable">The variable.</param>
        /// <param name="alias">The alias for the name.</param>
        void AliasNode(IVariable variable, string alias);

        /// <summary>
        /// Tries to get a variable.
        /// </summary>
        /// <param name="id">The name.</param>
        /// <param name="node">The found variable.</param>
        /// <returns>
        ///   <c>true</c> if the variable was found; otherwise <c>false</c>.
        /// </returns>
        bool TryGetNode(string id, out IVariable node);

        /// <summary>
        /// Clears the set from any variables.
        /// </summary>
        void Clear();
    }
}
