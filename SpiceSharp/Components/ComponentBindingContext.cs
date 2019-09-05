using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Context for binding an <see cref="IBehavior"/> created by a <see cref="Component"/> to an <see cref="ISimulation"/>.
    /// </summary>
    /// <seealso cref="BindingContext" />
    public class ComponentBindingContext : BindingContext
    {
        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        protected string Model { get; }

        /// <summary>
        /// Gets the model behaviors.
        /// </summary>
        /// <value>
        /// The model behaviors.
        /// </value>
        public EntityBehaviors ModelBehaviors => Model != null ? Simulation.EntityBehaviors[Model] : null;

        /// <summary>
        /// Gets the pins that the component is connected to.
        /// </summary>
        /// <value>
        /// The pins.
        /// </value>
        public int[] Pins { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentBindingContext"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="behaviors">The behaviors.</param>
        /// <param name="pins">The pins.</param>
        /// <param name="model">The model.</param>
        public ComponentBindingContext(ISimulation simulation, EntityBehaviors behaviors, int[] pins, string model)
            : base(simulation, behaviors)
        {
            Pins = pins.ThrowIfNull(nameof(pins));
            Model = model;
        }
    }
}
