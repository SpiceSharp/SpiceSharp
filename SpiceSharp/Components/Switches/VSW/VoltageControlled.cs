using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SwitchBehaviors
{
    /// <summary>
    /// Controller for making a switch voltage-controlled.
    /// </summary>
    /// <seealso cref="Controller" />
    public class VoltageControlled : Controller
    {
        /// <summary>
        /// Gets the controlling positive node index.
        /// </summary>
        protected int ContPosNode { get; private set; }

        /// <summary>
        /// Gets the controlling negative node index.
        /// </summary>
        protected int ContNegNode { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageControlled"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public VoltageControlled(ComponentBindingContext context)
        {
            context.Nodes.CheckNodes(4);
            var state = context.GetState<IBiasingSimulationState>();
            ContPosNode = state.Map[context.Nodes[2]];
            ContNegNode = state.Map[context.Nodes[3]];
        }

        /// <summary>
        /// Gets the value that is controlling the switch.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        public override double GetValue(IBiasingSimulationState state) =>
            state.Solution[ContPosNode] - state.Solution[ContNegNode];
    }
}
