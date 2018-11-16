using SpiceSharp.Components.SwitchBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="VoltageSwitch"/>
    /// </summary>
    public class VoltageSwitchModel : Model
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the model</param>
        public VoltageSwitchModel(string name)
            : base(name)
        {
            // Add parameters
            ParameterSets.Add(new VoltageModelParameters());
        }
    }
}
