using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CommonBehaviors
{
    /// <summary>
    /// A binding context for controlled sources.
    /// </summary>
    /// <seealso cref="ComponentBindingContext" />
    public class ControlledBindingContext : ComponentBindingContext
    {
        /// <summary>
        /// Gets the name of the controlling source.
        /// </summary>
        /// <value>
        /// The control source.
        /// </value>
        protected string ControlSource { get; }

        /// <summary>
        /// Gets the controlling source behaviors.
        /// </summary>
        /// <value>
        /// The behaviors.
        /// </value>
        public IBehaviorContainer ControlBehaviors => Simulation.EntityBehaviors[ControlSource];

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlledBindingContext" /> class.
        /// </summary>
        /// <param name="component">The component that creates the behavior.</param>
        /// <param name="simulation">The simulation for which the behavior is created.</param>
        /// <param name="control">The controlling source identifier.</param>
        public ControlledBindingContext(Component component, ISimulation simulation, string control)
            : base(component, simulation)
        {
            ControlSource = control.ThrowIfNull(nameof(control));
        }
    }
}
