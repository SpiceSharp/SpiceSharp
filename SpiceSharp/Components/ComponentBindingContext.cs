using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Context for binding an <see cref="IBehavior" /> created by an <see cref="IComponent" /> to an <see cref="ISimulation" />.
    /// </summary>
    /// <seealso cref="BindingContext" />
    /// <seealso cref="IComponentBindingContext" />
    [BindingContextFor(typeof(Component))]
    public class ComponentBindingContext : BindingContext,
        IComponentBindingContext
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
        public IReadOnlyList<string> Nodes { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingContext"/> class.
        /// </summary>
        /// <param name="component">The component creating the behavior.</param>
        /// <param name="simulation">The simulation for which a behavior is created.</param>
        /// <param name="behaviors">The behaviors created by the entity.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="component"/> or <paramref name="simulation"/> is <c>null</c>.</exception>
        public ComponentBindingContext(IComponent component, ISimulation simulation, IBehaviorContainer behaviors)
            : base(component, simulation, behaviors)
        {
            // Get the nodes of the component
            var nodes = component.Nodes;
            string[] myNodes;
            if (nodes != null && nodes.Count > 0)
            {
                myNodes = new string[nodes.Count];
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i] == null)
                    {
                        myNodes[i] = Constants.Ground;
                        SpiceSharpWarning.Warning(this, Properties.Resources.Nodes_NullToGround.FormatString(component.Name, i));
                    }
                    else
                        myNodes[i] = nodes[i];
                }
            }
            else
                myNodes = Array<string>.Empty();
            Nodes = myNodes;

            // Get the model of the component
            if (component.Model != null)
                ModelBehaviors = simulation.EntityBehaviors[component.Model];
            else
                ModelBehaviors = null;
        }
    }
}
