using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.Subcircuits.Simple
{
    /// <summary>
    /// An <see cref="ITimeBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="ITimeBehavior" />
    public class Time : Biasing,
        ITimeBehavior
    {
        private readonly BehaviorList<ITimeBehavior> _behaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="Time"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="simulation">The simulation.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> or <paramref name="simulation"/> is <c>null</c>.</exception>
        public Time(string name, SubcircuitSimulation simulation)
            : base(name, simulation)
        {
            _behaviors = Simulation.EntityBehaviors.GetBehaviorList<ITimeBehavior>();
        }

        /// <inheritdoc/>
        void ITimeBehavior.InitializeStates()
        {
            foreach (var behavior in _behaviors)
                behavior.InitializeStates();
        }
    }
}
