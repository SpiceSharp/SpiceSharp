using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.MutualInductances
{
    /// <summary>
    /// Binding context for a <see cref="MutualInductance"/>.
    /// </summary>
    /// <seealso cref="Entities.BindingContext" />
    [BindingContextFor(typeof(MutualInductance))]
    public class BindingContext : Entities.BindingContext
    {
        /// <summary>
        /// Gets the primary inductor behaviors.
        /// </summary>
        /// <value>
        /// The primary inductor behaviors.
        /// </value>
        public IBehaviorContainer Inductor1Behaviors { get; }

        /// <summary>
        /// Gets the secondary inductor behaviors.
        /// </summary>
        /// <value>
        /// The secondary inductor behaviors.
        /// </value>
        public IBehaviorContainer Inductor2Behaviors { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingContext" /> class.
        /// </summary>
        /// <param name="component">The component that creates the behavior.</param>
        /// <param name="simulation">The simulation for which the behavior is created.</param>
        /// <param name="behaviors">The created behaviors.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="component"/>, <paramref name="simulation"/> or <paramref name="behaviors"/> is <c>null</c>.</exception>
        public BindingContext(MutualInductance component, ISimulation simulation, IBehaviorContainer behaviors)
            : base(component, simulation, behaviors)
        {
            Inductor1Behaviors = simulation.EntityBehaviors[component.InductorName1.ThrowIfNull(nameof(component.InductorName1))];
            Inductor2Behaviors = simulation.EntityBehaviors[component.InductorName2.ThrowIfNull(nameof(component.InductorName2))];
        }
    }
}