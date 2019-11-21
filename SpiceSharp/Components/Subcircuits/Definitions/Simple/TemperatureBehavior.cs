using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.SubcircuitBehaviors.Simple
{
    /// <summary>
    /// An <see cref="ITemperatureBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="ITemperatureBehavior" />
    public class TemperatureBehavior : SubcircuitBehavior<ITemperatureBehavior>, ITemperatureBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="simulation">The simulation.</param>
        public TemperatureBehavior(string name, SubcircuitSimulation simulation)
            : base(name, simulation)
        {
        }

        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        public void Temperature()
        {
            foreach (var behavior in Behaviors)
                behavior.Temperature();
        }
    }
}
