using SpiceSharp.Attributes;
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
    [BindingContextFor(typeof(CurrentSwitch))]
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
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="component"/>, <paramref name="simulation"/> or <paramref name="behaviors"/> is <c>null</c>.</exception>
        public CurrentSwitchBindingContext(ICurrentControllingComponent component, ISimulation simulation, IBehaviorContainer behaviors)
            : base(component, simulation, behaviors)
        {
            var branch = ControlBehaviors.GetValue<IBranchedBehavior<double>>().Branch;
            ControlValue = () => branch.Value;
        }
    }
}
