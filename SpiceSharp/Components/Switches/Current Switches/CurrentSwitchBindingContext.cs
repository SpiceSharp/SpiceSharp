using SpiceSharp.Behaviors;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Switches
{
    /// <summary>
    /// A binding context for a <see cref="CurrentSwitch"/>.
    /// </summary>
    /// <seealso cref="CurrentControlledBindingContext" />
    public class CurrentSwitchBindingContext : CurrentControlledBindingContext,
        ISwitchBindingContext
    {
        /// <inheritdoc/>
        public Func<double> ControlValue { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSwitchBindingContext"/> class.
        /// </summary>
        /// <param name="component">The component that creates the behavior.</param>
        /// <param name="simulation">The simulation for which the behavior is created.</param>
        /// <param name="behaviors">The created behaviors.</param>
        /// <param name="linkParameters">Flag indicating that parameters should be linked.If false, only cloned parameters are returned by the context.</param>
        public CurrentSwitchBindingContext(ICurrentControllingComponent component, ISimulation simulation, IBehaviorContainer behaviors, bool linkParameters)
            : base(component, simulation, behaviors, linkParameters)
        {
            var branch = ControlBehaviors.GetValue<IBranchedBehavior<double>>().Branch;
            ControlValue = () => branch.Value;
        }
    }
}
