using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.Subcircuits.Simple
{
    /// <summary>
    /// An <see cref="IFrequencyUpdateBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="IFrequencyUpdateBehavior" />
    public class FrequencyUpdate : SubcircuitBehavior<IFrequencyUpdateBehavior>, IFrequencyUpdateBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyUpdate"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="simulation">The simulation.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> or <paramref name="simulation"/> is <c>null</c>.</exception>
        public FrequencyUpdate(string name, SubcircuitSimulation simulation)
            : base(name, simulation)
        {
        }

        /// <inheritdoc/>
        void IFrequencyUpdateBehavior.Update()
        {
            foreach (var behavior in Behaviors)
                behavior.Update();
        }
    }
}
