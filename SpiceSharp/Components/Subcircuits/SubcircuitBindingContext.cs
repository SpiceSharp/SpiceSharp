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
        /// Initializes a new instance of the <see cref="SubcircuitBindingContext"/> class.
        /// </summary>
        /// <param name="simulation">The simulation to bind to.</param>
        public SubcircuitBindingContext(Simulation simulation)
            : base(simulation)
        {
        }
    }
}
