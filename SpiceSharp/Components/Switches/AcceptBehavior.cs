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
        /// <param name="context">The context.</param>
        /// <param name="controller">The controller.</param>
        public AcceptBehavior(string name, ComponentBindingContext context, Controller controller) : base(name, context, controller)
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
