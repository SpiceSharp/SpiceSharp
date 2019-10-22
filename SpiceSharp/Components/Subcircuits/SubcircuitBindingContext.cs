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
        /// <param name="nodes">The nodes the subcircuit is connected to.</param>
        /// <param name="eb">The entity behaviors.</param>
        /// <param name="simulations">The pool of behaviors in the subcircuit.</param>
        public SubcircuitBindingContext(ISimulation simulation, Variable[] nodes, BehaviorContainer eb, SubcircuitSimulation[] simulations)
            : base(simulation, eb, nodes, null)
        {
            Simulations = simulations.ThrowIfNull(nameof(simulation));
        }
    }
}
