using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.Subcircuits.Simple
{
    /// <summary>
    /// An <see cref="IBiasingUpdateBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="IBiasingUpdateBehavior" />
    public class BiasingUpdate : SubcircuitBehavior<IBiasingUpdateBehavior>, IBiasingUpdateBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingUpdate"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="simulation">The simulation.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> or <paramref name="simulation"/> is <c>null</c>.</exception>
        public BiasingUpdate(string name, SubcircuitSimulation simulation)
            : base(name, simulation)
        {
        }

        /// <inheritdoc/>
        void IBiasingUpdateBehavior.Update()
        {
            foreach (var behavior in Behaviors)
                behavior.Update();
        }
    }
}
