using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SwitchBehaviors
{
    /// <summary>
    /// This class can calculate the controlling input of a switch.
    /// </summary>
    public abstract class Controller
    {
        /// <summary>
        /// Bind the controller to a simulation.
        /// </summary>
        /// <param name="context">The context.</param>
        public abstract void Bind(BindingContext context);

        /// <summary>
        /// Gets the value that is controlling the switch.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        public abstract double GetValue(BiasingSimulationState state);
    }
}
