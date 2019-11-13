using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Components.BipolarBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="BipolarJunctionTransistor"/>
    /// </summary>
    public class BipolarJunctionTransistorModel : Model
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BipolarJunctionTransistorModel"/> class.
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BipolarJunctionTransistorModel(string name) : base(name)
        {
            // Add parameters
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
