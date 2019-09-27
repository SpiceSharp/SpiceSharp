using SpiceSharp.Behaviors;

namespace SpiceSharp.Circuits.ParallelBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="ParallelLoader"/>.
    /// </summary>
    /// <seealso cref="ParallelBehavior{T}" />
    /// <seealso cref="ITemperatureBehavior" />
    public class TemperatureBehavior : ParallelBehavior<ITemperatureBehavior>, ITemperatureBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public TemperatureBehavior(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        public void Temperature()
        {
            /*
             * Temperature behaviors do temperature-dependent calculations. These behaviors
             * do not change the state in any way, so we can just run these in parallel.
             */
            For(behavior => behavior.Temperature);
        }
    }
}
