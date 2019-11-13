using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Components.JFETBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Model for a <see cref="JFET" />.
    /// </summary>
    /// <seealso cref="Model" />
    public class JFETModel : Model
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JFETModel"/> class.
        /// </summary>
        /// <param name="name">The name of the model.</param>
        public JFETModel(string name)
            : base(name)
        {
            Parameters.Add(new ModelBaseParameters());
            Parameters.Add(new ModelNoiseParameters());
        }

        /// <summary>
        /// Create one or more behaviors for the simulation.
        /// </summary>
        /// <param name="simulation">The simulation for which behaviors need to be created.</param>
        /// <param name="entities">The other entities.</param>
        /// <param name="behaviors">A container where all behaviors are to be stored.</param>
        protected override void CreateBehaviors(ISimulation simulation, IEntityCollection entities, BehaviorContainer behaviors)
        {
            var context = new ModelBindingContext(simulation, behaviors);
            if (simulation.EntityBehaviors.Tracks<ITemperatureBehavior>())
                behaviors.Add(new ModelTemperatureBehavior(Name, context));
        }
    }
}
