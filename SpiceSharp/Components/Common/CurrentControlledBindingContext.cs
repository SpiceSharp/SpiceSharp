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
    public class CurrentControlledBindingContext : ComponentBindingContext, ICurrentControlledBindingContext
    {
        /// <inheritdoc/>
        public IBehaviorContainer ControlBehaviors { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentControlledBindingContext" /> class.
        /// </summary>
        /// <param name="component">The component that creates the behavior.</param>
        /// <param name="simulation">The simulation for which the behavior is created.</param>
        /// <param name="behaviors">The created behaviors.</param>
        /// <param name = "linkParameters" > Flag indicating that parameters should be linked.If false, only cloned parameters are returned by the context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="component"/> or <paramref name="simulation"/> is <c>null</c>.</exception>
        public CurrentControlledBindingContext(ICurrentControllingComponent component, ISimulation simulation, IBehaviorContainer behaviors, bool linkParameters)
            : base(component, simulation, behaviors, linkParameters)
        {
            // Gets the current controlling behaviors
            if (component.ControllingSource != null)
                ControlBehaviors = simulation.EntityBehaviors[component.ControllingSource];
        }
    }
}
