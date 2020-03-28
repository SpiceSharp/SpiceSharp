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
        private int _brNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentControlled"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public CurrentControlled(CurrentControlledBindingContext context)
        {
            var state = context.GetState<IBiasingSimulationState>();
            var behavior = context.ControlBehaviors.GetValue<IBranchedBehavior<double>>();
            _brNode = state.Map[behavior.Branch];
        }

        /// <summary>
        /// Gets the value that is controlling the switch.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>
        /// The controlling value.
        /// </returns>
        public override double GetValue(IBiasingSimulationState state) => state.Solution[_brNode];
    }
}
