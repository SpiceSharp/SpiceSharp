using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SwitchBehaviors
{
    /// <summary>
    /// Controller for making a switch current-controlled.
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.SwitchBehaviors.Controller" />
    public class CurrentControlled : Controller
    {
        /// <summary>
        /// The load behavior of the voltage source that measures the current
        /// </summary>
        private VoltageSourceBehaviors.BiasingBehavior _loadBehavior;

        /// <summary>
        /// Setup the behavior for the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The provider.</param>
        /// <exception cref="ArgumentNullException">provider</exception>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            provider.ThrowIfNull(nameof(provider));

            // Get behaviors
            _loadBehavior = provider.GetBehavior<VoltageSourceBehaviors.BiasingBehavior>("control");
        }

        /// <summary>
        /// Gets the value that is controlling the switch.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        public override double GetValue(BaseSimulationState state) => state.Solution[_loadBehavior.BranchEq];
    }
}
