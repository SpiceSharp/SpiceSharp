using SpiceSharp.Behaviors;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// Initial condition behavior for a <see cref="ParallelLoader"/>.
    /// </summary>
    /// <seealso cref="ParallelBehavior{IInitialConditionBehavior}" />
    /// <seealso cref="IInitialConditionBehavior" />
    public class InitialConditionBehavior : ParallelBehavior<IInitialConditionBehavior>, IInitialConditionBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InitialConditionBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public InitialConditionBehavior(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Sets the initial conditions for the behavior.
        /// </summary>
        public void SetInitialCondition()
        {
            For(behavior => behavior.SetInitialCondition);
        }
    }
}
