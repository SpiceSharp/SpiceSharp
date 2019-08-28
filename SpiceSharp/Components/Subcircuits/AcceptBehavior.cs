using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Accept behavior for a <see cref="Subcircuit"/>.
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.SubcircuitBehaviors.SubcircuitBehavior{T}" />
    /// <seealso cref="SpiceSharp.Behaviors.IAcceptBehavior" />
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
            for (var i = 0; i < Behaviors.Count; i++)
                Behaviors[i].Accept();
        }

        /// <summary>
        /// Called when a new timepoint is being tested.
        /// </summary>
        public void Probe()
        {
            for (var i = 0; i < Behaviors.Count; i++)
                Behaviors[i].Probe();
        }
    }
}
