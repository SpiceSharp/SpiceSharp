using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components
{
    public partial class MutualInductance
    {
        /// <summary>
        /// Binding context for a <see cref="MutualInductance"/>.
        /// </summary>
        /// <seealso cref="Entities.BindingContext" />
        protected class BindingContext : Entities.BindingContext
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
            /// <param name="linkParameters">Flag indicating that parameters should be linked. If false, only cloned parameters are returned by the context.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="component"/> or <paramref name="simulation"/> is <c>null</c>.</exception>
            public BindingContext(MutualInductance component, ISimulation simulation, bool linkParameters)
                : base(component, simulation, linkParameters)
            {
                Inductor1Behaviors = simulation.EntityBehaviors[component.InductorName1.ThrowIfNull(nameof(component.InductorName1))];
                Inductor2Behaviors = simulation.EntityBehaviors[component.InductorName2.ThrowIfNull(nameof(component.InductorName2))];
            }
        }
    }
}