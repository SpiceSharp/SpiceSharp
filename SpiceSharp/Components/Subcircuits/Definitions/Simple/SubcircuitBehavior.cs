using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.Subcircuits.Simple
{
    /// <summary>
    /// A template for a subcircuit behavior.
    /// </summary>
    /// <typeparam name="B">The behavior type.</typeparam>
    /// <seealso cref="Behavior" />
    public abstract class SubcircuitBehavior<B> : Behavior
        where B : IBehavior
    {
        /// <summary>
        /// The subcircuit simulation.
        /// </summary>
        protected readonly SubcircuitSimulation Simulation;

        /// <summary>
        /// The behaviors of the subcircuit.
        /// </summary>
        protected readonly BehaviorList<B> Behaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitBehavior{B}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="simulation">The simulation.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> or <paramref name="simulation"/> is <c>null</c>.</exception>
        protected SubcircuitBehavior(string name, SubcircuitSimulation simulation)
            : base(name)
        {
            Simulation = simulation.ThrowIfNull(nameof(simulation));
            Behaviors = Simulation.EntityBehaviors.GetBehaviorList<B>();
        }
    }
}
