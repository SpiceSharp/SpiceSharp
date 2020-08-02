using SpiceSharp.Components.Switches;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="CurrentSwitch"/>
    /// </summary>
    public class CurrentSwitchModel : Entity<BindingContext>,
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
    }
}
