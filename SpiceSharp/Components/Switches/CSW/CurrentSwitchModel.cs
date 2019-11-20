using SpiceSharp.Behaviors;
using SpiceSharp.Components.SwitchBehaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="CurrentSwitch"/>
    /// </summary>
    public class CurrentSwitchModel : Model
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSwitchModel"/> class.
        /// </summary>
        /// <param name="name">The name of the model</param>
        public CurrentSwitchModel(string name) : base(name)
        {
            Parameters.Add<ModelBaseParameters>(new CurrentModelParameters());
        }

        /// <summary>
        /// Create one or more behaviors for the simulation.
        /// </summary>
        /// <param name="simulation">The simulation for which behaviors need to be created.</param>
        /// <param name="entities">The other entities.</param>
        /// <param name="behaviors">A container where all behaviors are to be stored.</param>
        protected override void CreateBehaviors(ISimulation simulation, IEntityCollection entities, IBehaviorContainer behaviors)
        {
        }
    }
}
