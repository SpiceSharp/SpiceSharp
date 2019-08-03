using System;
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
        /// Setup the behavior for the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The provider.</param>
        public abstract void Setup(Simulation simulation, SetupDataProvider provider);

        /// <summary>
        /// Connects the specified pins.
        /// </summary>
        /// <param name="pins">The pins.</param>
        public virtual void Connect(int[] pins)
        {
        }

        /// <summary>
        /// Gets the value that is controlling the switch.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        public abstract double GetValue(BaseSimulationState state);
    }
}
