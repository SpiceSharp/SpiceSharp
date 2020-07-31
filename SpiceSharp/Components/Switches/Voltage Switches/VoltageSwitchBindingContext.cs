using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Switches
{
    /// <summary>
    /// A binding context for a <see cref="VoltageSwitch"/>.
    /// </summary>
    /// <seealso cref="ComponentBindingContext" />
    /// <seealso cref="ISwitchBindingContext" />
    public class VoltageSwitchBindingContext : ComponentBindingContext,
        ISwitchBindingContext
    {
        /// <inheritdoc/>
        public Func<double> ControlValue { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageSwitchBindingContext"/> class.
        /// </summary>
        /// <param name="component">The component creating the behavior.</param>
        /// <param name="simulation">The simulation for which a behavior is created.</param>
        /// <param name="behaviors">The behaviors created by the entity.</param>
        /// <param name="linkParameters">Flag indicating that parameters should be linked. If false, only cloned parameters are returned by the context.</param>
        public VoltageSwitchBindingContext(IComponent component, ISimulation simulation, IBehaviorContainer behaviors, bool linkParameters) : base(component, simulation, behaviors, linkParameters)
        {
            var state = GetState<IBiasingSimulationState>();
            var a = state.GetSharedVariable(Nodes[2]);
            var b = state.GetSharedVariable(Nodes[3]);
            ControlValue = () => a.Value - b.Value;
        }
    }
}
