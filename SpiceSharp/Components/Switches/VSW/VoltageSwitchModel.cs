using SpiceSharp.Behaviors;
using SpiceSharp.Components.SwitchBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="VoltageSwitch"/>
    /// </summary>
    public class VoltageSwitchModel : Model,
        IParameterized<VoltageModelParameters>
    {
        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        public VoltageModelParameters Parameters { get; } = new VoltageModelParameters();

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageSwitchModel"/> class.
        /// </summary>
        /// <param name="name">The name of the model</param>
        public VoltageSwitchModel(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void CreateBehaviors(ISimulation simulation)
        {
            CalculateDefaults();
            var container = new BehaviorContainer(Name)
            {
                new ModelBehavior(Name, new ModelBindingContext(this, simulation))
            };
            simulation.EntityBehaviors.Add(container);
        }
    }
}
