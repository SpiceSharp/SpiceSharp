using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.SwitchBehaviors
{
    /// <summary>
    /// Accepting behavior for switches.
    /// </summary>
    public class AcceptBehavior : BiasingBehavior, IAcceptBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AcceptBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="method">The method.</param>
        public AcceptBehavior(string name, Controller method) : base(name, method)
        {
        }

        /// <summary>
        /// Called when a new timepoint is being tested.
        /// </summary>
        void IAcceptBehavior.Probe()
        {
        }

        /// <summary>
        /// Accepts the current timepoint.
        /// </summary>
        void IAcceptBehavior.Accept()
        {
            UseOldState = true;
            PreviousState = CurrentState;
        }
    }
}
