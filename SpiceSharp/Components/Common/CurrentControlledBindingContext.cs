using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.CommonBehaviors
{
    /// <summary>
    /// A binding context for controlled sources.
    /// </summary>
    /// <seealso cref="ComponentBindingContext" />
    /// <seealso cref="ICurrentControlledBindingContext"/>
    [BindingContextFor(typeof(CurrentControlledCurrentSource))]
    [BindingContextFor(typeof(CurrentControlledVoltageSource))]
    public class CurrentControlledBindingContext : ComponentBindingContext,
        ICurrentControlledBindingContext
    {
        /// <inheritdoc/>
        public IBehaviorContainer ControlBehaviors { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentControlledBindingContext" /> class.
        /// </summary>
        /// <param name="component">The component that creates the behavior.</param>
        /// <param name="simulation">The simulation for which the behavior is created.</param>
        /// <param name="behaviors">The created behaviors.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="component"/> or <paramref name="simulation"/> is <c>null</c>.</exception>
        public CurrentControlledBindingContext(ICurrentControllingComponent component, ISimulation simulation, IBehaviorContainer behaviors)
            : base(component, simulation, behaviors)
        {
            // Gets the current controlling behaviors
            if (component.ControllingSource != null)
                ControlBehaviors = simulation.EntityBehaviors[component.ControllingSource];
        }
    }
}
