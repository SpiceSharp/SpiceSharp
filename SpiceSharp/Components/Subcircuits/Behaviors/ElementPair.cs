using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// A pair of <see cref="Element{T}"/> instances.
    /// </summary>
    /// <seealso cref="ISolverSimulationState{T}" />
    public struct ElementPair<T> where T : IFormattable
    {
        /// <summary>
        /// Gets the parent element.
        /// </summary>
        /// <value>
        /// The parent element.
        /// </value>
        public Element<T> Parent { get; }

        /// <summary>
        /// Gets the local element.
        /// </summary>
        /// <value>
        /// The local element.
        /// </value>
        public Element<T> Local { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementPair{T}"/> struct.
        /// </summary>
        /// <param name="local">The local.</param>
        /// <param name="parent">The parent.</param>
        public ElementPair(Element<T> local, Element<T> parent)
        {
            Local = local.ThrowIfNull(nameof(local));
            Parent = parent.ThrowIfNull(nameof(parent));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "{0} -> {1}".FormatString(Local, Parent);
        }
    }
}
