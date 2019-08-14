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
        /// Bind the behavior. for the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">The context.</param>
        public abstract void Bind(Simulation simulation, BindingContext context);

        /// <summary>
        /// Gets the value that is controlling the switch.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        public abstract double GetValue(BaseSimulationState state);
    }
}
