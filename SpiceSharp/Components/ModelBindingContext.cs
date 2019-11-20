using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Context for binding an <see cref="IBehavior"/> created by a <see cref="Model"/> to an <see cref="ISimulation"/>.
    /// </summary>
    /// <seealso cref="BindingContext" />
    public class ModelBindingContext : BindingContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBindingContext"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="behaviors">The entity behaviors.</param>
        public ModelBindingContext(ISimulation simulation, IBehaviorContainer behaviors) : base(simulation, behaviors)
        {
        }
    }
}
