using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Context for binding an <see cref="IBehavior"/> created by an <see cref="IComponent"/> to an <see cref="ISimulation"/>.
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
        public IBehaviorContainer ModelBehaviors => Model != null ? Simulation.EntityBehaviors[Model] : null;

        /// <summary>
        /// Gets the nodes that the component is connected to.
        /// </summary>
        /// <value>
        /// The pins.
        /// </value>
        public Variable[] Nodes { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentBindingContext"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="behaviors">The behaviors.</param>
        /// <param name="nodes">The nodes that the component is connected to.</param>
        /// <param name="model">The model.</param>
        public ComponentBindingContext(ISimulation simulation, IBehaviorContainer behaviors, IEnumerable<Variable> nodes, string model)
            : base(simulation, behaviors)
        {
            Nodes = nodes.ThrowIfNull(nameof(nodes)).ToArray();
            Model = model;
        }
    }
}
