using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Accept behavior for a <see cref="Subcircuit"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="IAcceptBehavior" />
    public class AcceptBehavior : SubcircuitBehavior<IAcceptBehavior>, IAcceptBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AcceptBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public AcceptBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Accepts the current timepoint.
        /// </summary>
        public void Accept()
        {
            foreach (var bs in Behaviors)
            {
                for (var i = 0; i < bs.Count; i++)
                    bs[i].Accept();
            }
        }

        /// <summary>
        /// Called when a new timepoint is being tested.
        /// </summary>
        public void Probe()
        {
            foreach (var bs in Behaviors)
            {
                for (var i = 0; i < bs.Count; i++)
                    bs[i].Probe();
            }
        }
    }
}
