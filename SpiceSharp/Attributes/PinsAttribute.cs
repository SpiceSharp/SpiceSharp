using System;
using System.Collections.Generic;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// Specifies the pins for a circuit component that extends <see cref="Components.Component"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PinsAttribute : Attribute
    {
        /// <summary>
        /// Get a pin name
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns></returns>
        public string this[int index]
        {
            get => nodes[index];
        }

        /// <summary>
        /// Get node names
        /// </summary>
        public IEnumerable<string> Nodes
        {
            get => nodes;
        }

        /// <summary>
        /// Get number of pins
        /// </summary>
        public int Count { get => nodes.Length; }

        /// <summary>
        /// Nodes
        /// </summary>
        string[] nodes;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nodes">The nodes (in order) of the circuit component</param>
        public PinsAttribute(params string[] nodes)
        {
            this.nodes = nodes;
        }
    }
}
