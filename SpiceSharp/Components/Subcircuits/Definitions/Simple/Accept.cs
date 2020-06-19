using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.Subcircuits.Simple
{
    /// <summary>
    /// An <see cref="IAcceptBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="IAcceptBehavior" />
    public class Accept : SubcircuitBehavior<IAcceptBehavior>,
        IAcceptBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Accept"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="simulation">The simulation.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> or <paramref name="simulation"/> is <c>null</c>.</exception>
        public Accept(string name, SubcircuitSimulation simulation)
            : base(name, simulation)
        {
        }

        /// <inheritdoc/>
        void IAcceptBehavior.Accept()
        {
            foreach (var behavior in Behaviors)
                behavior.Accept();
        }

        /// <inheritdoc/>
        void IAcceptBehavior.Probe()
        {
            foreach (var behavior in Behaviors)
                behavior.Probe();
        }
    }
}
