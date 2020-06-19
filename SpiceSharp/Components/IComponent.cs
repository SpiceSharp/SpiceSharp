using SpiceSharp.Entities;
using System;
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
        /// Gets or sets the name of the component model.
        /// </summary>
        /// <value>
        /// The name of the component model.
        /// </value>
        string Model { get; set; }

        /// <summary>
        /// Gets a list of all the nodes that the component is connected to.
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
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="nodes"/> or any of the nodes is <c>null</c>.</exception>
        /// <exception cref="NodeMismatchException">Thrown if the number of nodes does not match the pin count of the component.</exception>
        IComponent Connect(params string[] nodes);
    }
}
