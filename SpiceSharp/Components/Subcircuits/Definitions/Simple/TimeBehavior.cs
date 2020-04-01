using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.SubcircuitBehaviors.Simple
{
    /// <summary>
    /// An <see cref="ITimeBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="ITimeBehavior" />
    public class TimeBehavior : BiasingBehavior, ITimeBehavior
    {
        private readonly BehaviorList<ITimeBehavior> _behaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="simulation">The simulation.</param>
        public TimeBehavior(string name, SubcircuitSimulation simulation)
            : base(name, simulation)
        {
            _behaviors = Simulation.EntityBehaviors.GetBehaviorList<ITimeBehavior>();
        }

        /// <summary>
        /// Initialize the state values from the current DC solution.
        /// </summary>
        public void InitializeStates()
        {
            foreach (var behavior in _behaviors)
                behavior.InitializeStates();
        }
    }
}
