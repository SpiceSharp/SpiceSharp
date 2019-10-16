using SpiceSharp.Entities;
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
        /// Bind the behavior. for the specified simulation.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void Bind(BindingContext context)
        {
            var c = (CommonBehaviors.ControlledBindingContext)context;
            var state = context.States.GetValue<IBiasingSimulationState>();
            var behavior = c.ControlBehaviors.GetValue<VoltageSourceBehaviors.BiasingBehavior>();
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
