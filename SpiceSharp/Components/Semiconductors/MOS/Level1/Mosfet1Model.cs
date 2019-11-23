using SpiceSharp.Behaviors;
using SpiceSharp.Components.MosfetBehaviors.Level1;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="Mosfet1"/>
    /// </summary>
    public class Mosfet1Model : Model
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mosfet1Model"/> class.
        /// </summary>
        /// <param name="name">The name of the device</param>
        public Mosfet1Model(string name) : base(name)
        {
            Parameters.Add(new ModelBaseParameters());
            Parameters.Add(new MosfetBehaviors.Common.ModelNoiseParameters());
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
