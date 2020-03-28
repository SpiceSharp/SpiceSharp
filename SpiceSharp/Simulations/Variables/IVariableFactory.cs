using SpiceSharp.Simulations.Variables;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A template for a variable mapper that can create variables and map them to indices.
    /// </summary>
    /// <typeparam name="V"></typeparam>
    public interface IVariableFactory<out V> where V : IVariable
    {
        /// <summary>
        /// Maps a shared node in the simulation.
        /// </summary>
        /// <param name="name">The name of the shared node.</param>
        /// <returns>
        /// The shared node variable.
        /// </returns>
        V MapNode(string name);

        /// <summary>
        /// Maps a number of nodes.
        /// </summary>
        /// <param name="names">The nodes.</param>
        /// <returns>
        /// The shared node variables.
        /// </returns>
        IEnumerable<V> MapNodes(IEnumerable<string> names);

        /// <summary>
        /// Creates a local variable that should not be shared by the state with anyone else.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="unit">The unit of the variable.</param>
        /// <returns>
        /// The local variable.
        /// </returns>
        V Create(string name, IUnit unit);

        /// <summary>
        /// Determines whether the specified variable is a node without mapping it.
        /// </summary>
        /// <param name="name">The name of the node.</param>
        /// <returns>
        ///   <c>true</c> if the specified variable has node; otherwise, <c>false</c>.
        /// </returns>
        bool HasNode(string name);
    }
}
