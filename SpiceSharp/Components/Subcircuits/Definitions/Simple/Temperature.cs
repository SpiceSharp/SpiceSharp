using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.Subcircuits.Simple
{
    /// <summary>
    /// An <see cref="ITemperatureBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="ITemperatureBehavior" />
    public class Temperature : SubcircuitBehavior<ITemperatureBehavior>,
        ITemperatureBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Temperature"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="simulation">The simulation.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> or <paramref name="simulation"/> is <c>null</c>.</exception>
        public Temperature(string name, SubcircuitSimulation simulation)
            : base(name, simulation)
        {
        }

        /// <inheritdoc/>
        void ITemperatureBehavior.Temperature()
        {
            foreach (var behavior in Behaviors)
                behavior.Temperature();
        }
    }
}
