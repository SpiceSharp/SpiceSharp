using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CommonBehaviors
{
    /// <summary>
    /// A binding context for controlled sources.
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.ComponentBindingContext" />
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
        public EntityBehaviorDictionary ControlBehaviors => Simulation.EntityBehaviors[ControlSource];

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlledBindingContext"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="name">The name.</param>
        /// <param name="pins">The pins.</param>
        /// <param name="model">The model.</param>
        /// <param name="control">The control.</param>
        public ControlledBindingContext(ISimulation simulation, string name, int[] pins, string model, string control)
            : base(simulation, name, pins, model)
        {
            ControlSource = control.ThrowIfNull(nameof(control));
        }
    }
}
