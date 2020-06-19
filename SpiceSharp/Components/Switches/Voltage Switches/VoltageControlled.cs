using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.Switches
{
    /// <summary>
    /// Controller for making a switch voltage-controlled.
    /// </summary>
    /// <seealso cref="Controller" />
    public class VoltageControlled : Controller
    {
        private readonly OnePort<double> _variables;

        /// <summary>
        /// Gets the value of the controlling value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public override double Value => _variables.Positive.Value - _variables.Negative.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageControlled"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public VoltageControlled(ComponentBindingContext context)
        {
            context.Nodes.CheckNodes(4);
            var state = context.GetState<IBiasingSimulationState>();
            _variables = new OnePort<double>(state.GetSharedVariable(context.Nodes[2]), state.GetSharedVariable(context.Nodes[3]));
        }
    }
}
