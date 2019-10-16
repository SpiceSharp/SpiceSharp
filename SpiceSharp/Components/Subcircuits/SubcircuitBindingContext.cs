using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Binding context for <see cref="Subcircuit"/> components.
    /// </summary>
    /// <seealso cref="ComponentBindingContext" />
    public class SubcircuitBindingContext : ComponentBindingContext
    {
        /// <summary>
        /// Gets the simulations.
        /// </summary>
        /// <value>
        /// The simulations.
        /// </value>
        public SubcircuitSimulation[] Simulations { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitBindingContext"/> class.
        /// </summary>
        /// <param name="simulation">The simulation to bind to.</param>
        /// <param name="eb">The entity behaviors.</param>
        /// <param name="simulations">The pool of behaviors in the subcircuit.</param>
        public SubcircuitBindingContext(ISimulation simulation, BehaviorContainer eb, SubcircuitSimulation[] simulations)
            : base(simulation, eb, new Variable[] { }, null)
        {
            Simulations = simulations.ThrowIfNull(nameof(simulation));
        }
    }
}
