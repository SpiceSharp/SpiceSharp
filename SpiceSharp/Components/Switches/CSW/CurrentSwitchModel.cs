using SpiceSharp.Components.SwitchBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="CurrentSwitch"/>
    /// </summary>
    public class CurrentSwitchModel : Model
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CurrentSwitchModel"/> class.
        /// </summary>
        /// <param name="name">The name of the model</param>
        public CurrentSwitchModel(string name) : base(name)
        {
            // Add parameters
            ParameterSets.Add(new CurrentModelParameters());
        }
    }
}
