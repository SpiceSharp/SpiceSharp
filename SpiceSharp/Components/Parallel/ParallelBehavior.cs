using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ParallelBehaviors
{
    /// <summary>
    /// A behavior for a <see cref="ParallelComponents"/>.
    /// </summary>
    /// <typeparam name="B">The behavior type.</typeparam>
    /// <seealso cref="Behavior" />
    public abstract class ParallelBehavior<B> : Behavior where B : IBehavior
    {
        /// <summary>
        /// Gets the behaviors.
        /// </summary>
        /// <value>
        /// The behaviors.
        /// </value>
        protected BehaviorList<B> Behaviors { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelBehavior{B}"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        /// <param name="simulation">The parallel simulation.</param>
        protected ParallelBehavior(string name, ParallelSimulation simulation)
            : base(name)
        {
            Behaviors = simulation.EntityBehaviors.GetBehaviorList<B>();
        }
    }
}
