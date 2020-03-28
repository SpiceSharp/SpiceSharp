using SpiceSharp.Entities;
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
        /// Gets the nodes.
        /// </summary>
        /// <value>
        /// The nodes.
        /// </value>
        IReadOnlyList<string> Nodes { get; }

        /// <summary>
        /// Connects the component in the circuit.
        /// </summary>
        /// <param name="nodes">The node indices.</param>
        /// <returns>The instance calling the method for chaining.</returns>
        IComponent Connect(params string[] nodes);
    }
}
