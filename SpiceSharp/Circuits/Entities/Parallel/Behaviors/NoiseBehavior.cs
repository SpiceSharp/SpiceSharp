using SpiceSharp.Behaviors;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// A noise behavior for a <see cref="ParallelLoader"/>.
    /// </summary>
    /// <seealso cref="ParallelBehavior{T}" />
    /// <seealso cref="INoiseBehavior" />
    public class NoiseBehavior : ParallelBehavior<INoiseBehavior>, INoiseBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoiseBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public NoiseBehavior(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Calculate the noise contributions.
        /// </summary>
        public void Noise()
        {
            For(behavior => behavior.Noise);
        }
    }
}
