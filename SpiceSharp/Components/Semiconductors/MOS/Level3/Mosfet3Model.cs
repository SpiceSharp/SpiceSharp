using SpiceSharp.Behaviors;
using SpiceSharp.Components.MosfetBehaviors.Level3;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="Mosfet3"/>
    /// </summary>
    public class Mosfet3Model : Model
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mosfet3Model"/> class.
        /// </summary>
        /// <param name="name">The name of the device</param>
        public Mosfet3Model(string name) : base(name)
        {
            Parameters.Add(new ModelBaseParameters());
            Parameters.Add(new MosfetBehaviors.Common.ModelNoiseParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mosfet3Model"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="nmos">True for NMOS transistors, false for PMOS transistors</param>
        public Mosfet3Model(string name, bool nmos) : base(name)
        {
            Parameters.Add(new ModelBaseParameters(nmos));
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
            if (simulation.UsesBehaviors<ITemperatureBehavior>())
                behaviors.Add(new ModelTemperatureBehavior(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
