using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SwitchBehaviors
{
    /// <summary>
    /// Controller for making a switch current-controlled.
    /// </summary>
    /// <seealso cref="Controller" />
    public class CurrentControlled : Controller
    {
        private readonly IVariable<double> _branch;

        /// <summary>
        /// Gets the value of the controlling value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public override double Value => _branch.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentControlled"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public CurrentControlled(CurrentControlledBindingContext context)
        {
            var state = context.GetState<IBiasingSimulationState>();
            var behavior = context.ControlBehaviors.GetValue<IBranchedBehavior<double>>();
            _branch = behavior.Branch;
        }
    }
}
