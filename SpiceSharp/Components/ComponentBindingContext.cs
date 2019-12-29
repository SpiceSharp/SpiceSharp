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
        /// Gets the model behaviors.
        /// </summary>
        /// <value>
        /// The model behaviors.
        /// </value>
        public IBehaviorContainer ModelBehaviors { get; }

        /// <summary>
        /// Gets the nodes that the component is connected to.
        /// </summary>
        /// <value>
        /// The pins.
        /// </value>
        public IReadOnlyList<Variable> Nodes { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingContext"/> class.
        /// </summary>
        /// <param name="component">The component creating the behavior.</param>
        /// <param name="simulation">The simulation for which a behavior is created.</param>
        /// <param name="linkParameters">Flag indicating that parameters should be linked. If false, only cloned parameters are returned by the context.</param>
        public ComponentBindingContext(IComponent component, ISimulation simulation, bool linkParameters)
            : base(component, simulation, linkParameters)
        {
            Nodes = component.MapNodes(simulation.Variables);
            if (component.Model != null)
                ModelBehaviors = simulation.EntityBehaviors[component.Model];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentBindingContext"/> class.
        /// </summary>
        /// <param name="component">The component creating the behavior.</param>
        /// <param name="simulation">The simulation for which a behavior is created.</param>
        public ComponentBindingContext(Component component, ISimulation simulation)
            : base(component, simulation)
        {
            Nodes = component.MapNodes(simulation.Variables);
            if (component.Model != null)
                ModelBehaviors = simulation.EntityBehaviors[component.Model];
        }
    }
}
