using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An interface that describes a component that can be connected in a circuit.
    /// </summary>
    /// <seealso cref="IEntity" />
    public interface IComponent : IEntity
    {
        /// <summary>
        /// Gets or sets the model of the component.
        /// </summary>
        /// <value>
        /// The model of the component.
        /// </value>
        string Model { get; set; }

        /// <summary>
        /// Gets the number of nodes.
        /// </summary>
        /// <value>
        /// The number of nodes.
        /// </value>
        int PinCount { get; }

        /// <summary>
        /// Connects the component in the circuit.
        /// </summary>
        /// <param name="nodes">The node indices.</param>
        /// <returns>The instance calling the method for chaining.</returns>
        IComponent Connect(params string[] nodes);

        /// <summary>
        /// Gets the node name by pin index.
        /// </summary>
        /// <param name="index">The pin index.</param>
        /// <returns>The node index.</returns>
        string GetNode(int index);

        /// <summary>
        /// Gets the node indexes (in order).
        /// </summary>
        /// <param name="variables">The set of variables.</param>
        /// <returns>An enumerable for all nodes.</returns>
        IReadOnlyList<IVariable> MapNodes(IVariableSet variables);
    }
}
