using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Binding context for <see cref="Subcircuit"/> components.
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.ComponentBindingContext" />
    public class SubcircuitBindingContext : ComponentBindingContext
    {
        /// <summary>
        /// Gets the pool of behaviors.
        /// </summary>
        /// <value>
        /// The pool.
        /// </value>
        public BehaviorPool Pool { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitBindingContext"/> class.
        /// </summary>
        /// <param name="simulation">The simulation to bind to.</param>
        /// <param name="pool">The pool of behaviors in the subcircuit.</param>
        public SubcircuitBindingContext(Simulation simulation, BehaviorPool pool)
            : base(simulation)
        {
            Pool = pool;
        }
    }
}
