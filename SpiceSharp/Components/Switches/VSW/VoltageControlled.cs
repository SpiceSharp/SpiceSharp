using System;
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
        /// <value>
        /// The controlling positive node index.
        /// </value>
        protected int ContPosNode { get; private set; }

        /// <summary>
        /// Gets the controlling negative node index.
        /// </summary>
        /// <value>
        /// The controlling negative node index.
        /// </value>
        protected int ContNegNode { get; private set; }
        
        /// <summary>
        /// Setup the behavior for the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The provider.</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
        }

        /// <summary>
        /// Connects the specified pins.
        /// </summary>
        /// <param name="pins">The pins.</param>
        /// <exception cref="ArgumentNullException">pins</exception>
        /// <exception cref="ArgumentException">Pin count mismatch</exception>
        public override void Connect(int[] pins)
        {
            pins.ThrowIfNot(nameof(pins), 4);
            ContPosNode = pins[2];
            ContNegNode = pins[3];
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
