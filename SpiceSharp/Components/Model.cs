using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class represents a (Spice) model.
    /// </summary>
    public abstract class Model : Entity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Model"/> class.
        /// </summary>
        /// <param name="name">The name of the model.</param>
        protected Model(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
