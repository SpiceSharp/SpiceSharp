using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SwitchBehaviors
{
    /// <summary>
    /// Controller for making a switch voltage-controlled.
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.SwitchBehaviors.Controller" />
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
        /// Bind the behavior. for the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">The context.</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            if (context is ComponentBindingContext cc)
            {
                ContPosNode = cc.Pins[2];
                ContNegNode = cc.Pins[3];
            }
        }

        /// <summary>
        /// Gets the value that is controlling the switch.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        public override double GetValue(BaseSimulationState state) =>
            state.Solution[ContPosNode] - state.Solution[ContNegNode];
    }
}
