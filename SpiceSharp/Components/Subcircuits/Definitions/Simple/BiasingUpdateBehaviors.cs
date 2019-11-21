using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.SubcircuitBehaviors.Simple
{
    /// <summary>
    /// An <see cref="IBiasingUpdateBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="IBiasingUpdateBehavior" />
    public class BiasingUpdateBehavior : SubcircuitBehavior<IBiasingUpdateBehavior>, IBiasingUpdateBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingUpdateBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="simulation">The simulation.</param>
        public BiasingUpdateBehavior(string name, SubcircuitSimulation simulation)
            : base(name, simulation)
        {
        }

        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        public void Update()
        {
            foreach (var behavior in Behaviors)
                behavior.Update();
        }
    }
}
