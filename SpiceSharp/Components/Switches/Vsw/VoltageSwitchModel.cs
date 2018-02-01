using SpiceSharp.Components.VoltageSwitchBehaviors;

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
        public VoltageSwitchModel(Identifier name)
            : base(name)
        {
            // Add parameters
            ParameterSets.Add(new ModelBaseParameters());

            // Add factories
            Behaviors.Add(typeof(ModelLoadBehavior), () => new ModelLoadBehavior(Name));
        }
    }
}
