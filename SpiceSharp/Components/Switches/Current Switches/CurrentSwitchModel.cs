using SpiceSharp.Behaviors;
using SpiceSharp.Components.Common;
using SpiceSharp.Components.Switches;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="CurrentSwitch"/>
    /// </summary>
    public class CurrentSwitchModel : Model,
        IParameterized<CurrentModelParameters>
    {
        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        public CurrentModelParameters Parameters { get; } = new CurrentModelParameters();

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSwitchModel"/> class.
        /// </summary>
        /// <param name="name">The name of the model.</param>
        public CurrentSwitchModel(string name) 
            : base(name)
        {
        }

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void CreateBehaviors(ISimulation simulation)
        {
            Parameters.CalculateDefaults();
            var container = new BehaviorContainer(Name)
            {
                new ParameterBehavior<ModelParameters>(Name, new BindingContext(this, simulation, LinkParameters))
            };
            simulation.EntityBehaviors.Add(container);
        }
    }
}
