using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.SubcircuitBehaviors.Simple
{
    /// <summary>
    /// An <see cref="IFrequencyUpdateBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="IFrequencyUpdateBehavior" />
    public class FrequencyUpdateBehavior : SubcircuitBehavior<IFrequencyUpdateBehavior>, IFrequencyUpdateBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyUpdateBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="simulation">The simulation.</param>
        public FrequencyUpdateBehavior(string name, SubcircuitSimulation simulation)
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
