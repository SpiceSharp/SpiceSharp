using SpiceSharp.Behaviors;
using SpiceSharp.Components.DiodeBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="Diode"/>
    /// </summary>
    public class DiodeModel : Model
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiodeModel"/> class.
        /// </summary>
        /// <param name="name">The name of the device</param>
        public DiodeModel(string name) : base(name)
        {
            Parameters.Add(new ModelBaseParameters());
            Parameters.Add(new ModelNoiseParameters());
        }

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name,
                LinkParameters ? Parameters : (IParameterSetDictionary)Parameters.Clone());
            behaviors.Parameters.CalculateDefaults();
            var context = new ModelBindingContext(simulation, behaviors);
            behaviors.AddIfNo<ITemperatureBehavior>(simulation, () => new ModelTemperatureBehavior(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
