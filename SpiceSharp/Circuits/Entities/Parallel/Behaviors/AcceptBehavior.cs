using SpiceSharp.Behaviors;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// Accept behavior for a <see cref="ParallelLoader"/>
    /// </summary>
    /// <seealso cref="ParallelBehavior{T}" />
    /// <seealso cref="IAcceptBehavior" />
    public class AcceptBehavior : ParallelBehavior<IAcceptBehavior>, IAcceptBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AcceptBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public AcceptBehavior(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Accepts the current timepoint.
        /// </summary>
        public void Accept()
        {
            For(behavior => behavior.Accept);
        }

        /// <summary>
        /// Called when a new timepoint is being tested.
        /// </summary>
        public void Probe()
        {
            For(behavior => behavior.Probe);
        }
    }
}
