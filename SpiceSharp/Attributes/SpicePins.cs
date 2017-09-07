using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Specifies the pins for a circuit component that extends <see cref="CircuitComponent{T}"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SpicePins : Attribute
    {
        /// <summary>
        /// Gets the nodes of the component
        /// </summary>
        public string[] Nodes { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nodes">The nodes (in order) of the circuit component</param>
        public SpicePins(params string[] nodes)
        {
            Nodes = nodes;
        }
    }
}
