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
        /// <param name="entity">The entity creating the behavior.</param>
        /// <param name="simulation">The simulation for which a behavior is created.</param>
        /// <param name="linkParameters">Flag indicating that parameters should be linked. If false, only cloned parameters are returned by the context.</param>
        public ModelBindingContext(IEntity entity, ISimulation simulation, bool linkParameters)
            : base(entity, simulation, linkParameters)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBindingContext"/> class.
        /// </summary>
        /// <param name="entity">The entity creating the behavior.</param>
        /// <param name="simulation">The simulation for which a behavior is created.</param>
        public ModelBindingContext(Entity entity, ISimulation simulation)
            : base(entity, simulation)
        {
        }
    }
}
