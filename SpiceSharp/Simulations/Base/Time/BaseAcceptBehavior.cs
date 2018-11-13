using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Template for accepting a timepoint.
    /// </summary>
    public abstract class BaseAcceptBehavior : Behavior, IAcceptBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseAcceptBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        protected BaseAcceptBehavior(string name) : base(name) { }

        /// <summary>
        /// Called when a new timepoint is being tested.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        public abstract void Probe(TimeSimulation simulation);

        /// <summary>
        /// Accepts the current timepoint.
        /// </summary>
        /// <param name="simulation">The time-based simulation</param>
        public abstract void Accept(TimeSimulation simulation);
    }
}
