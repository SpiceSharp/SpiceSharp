using SpiceSharp.Behaviors;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// A time behavior for a <see cref="ParallelLoader"/>
    /// </summary>
    /// <seealso cref="ParallelBehavior{T}" />
    /// <seealso cref="ITimeBehavior" />
    public class TimeBehavior : ParallelBehavior<ITimeBehavior>, ITimeBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public TimeBehavior(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Initialize the state values from the current DC solution.
        /// </summary>
        public void InitializeStates()
        {
            For(behavior => behavior.InitializeStates);
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector.
        /// </summary>
        public void Load()
        {
            For(behavior => behavior.Load);
        }
    }
}
