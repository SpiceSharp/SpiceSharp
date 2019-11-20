using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MutualInductanceBehaviors
{
    /// <summary>
    /// Binding context for a <see cref="MutualInductance"/>.
    /// </summary>
    /// <seealso cref="BindingContext" />
    public class MutualInductanceBindingContext : BindingContext
    {
        /// <summary>
        /// Gets the name of the primary inductor.
        /// </summary>
        /// <value>
        /// The name of the primary inductor.
        /// </value>
        protected string InductorName1 { get; }

        /// <summary>
        /// Gets the name of the secondary inductor.
        /// </summary>
        /// <value>
        /// The name of the secondary inductor.
        /// </value>
        protected string InductorName2 { get; }

        /// <summary>
        /// Gets the primary inductor behaviors.
        /// </summary>
        /// <value>
        /// The primary inductor behaviors.
        /// </value>
        public IBehaviorContainer Inductor1Behaviors => Simulation.EntityBehaviors[InductorName1];

        /// <summary>
        /// Gets the secondary inductor behaviors.
        /// </summary>
        /// <value>
        /// The secondary inductor behaviors.
        /// </value>
        public IBehaviorContainer Inductor2Behaviors => Simulation.EntityBehaviors[InductorName2];

        /// <summary>
        /// Initializes a new instance of the <see cref="MutualInductanceBindingContext"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="eb">The eb.</param>
        /// <param name="inductor1">The name of the primary inductor.</param>
        /// <param name="inductor2">The name of the secondary inductor.</param>
        public MutualInductanceBindingContext(ISimulation simulation, IBehaviorContainer eb, string inductor1, string inductor2)
            : base(simulation, eb)
        {
            InductorName1 = inductor1.ThrowIfNull(nameof(inductor1));
            InductorName2 = inductor2.ThrowIfNull(nameof(inductor2));
        }
    }
}
