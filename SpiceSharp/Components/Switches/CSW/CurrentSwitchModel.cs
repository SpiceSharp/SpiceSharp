using SpiceSharp.Components.CurrentSwitchBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="CurrentSwitch"/>
    /// </summary>
    public class CurrentSwitchModel : Model
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the model</param>
        public CurrentSwitchModel(Identifier name) : base(name)
        {
            // Add parameters
            ParameterSets.Add(new ModelBaseParameters());

            // Add factories
            Behaviors.Add(typeof(ModelLoadBehavior), () => new ModelLoadBehavior(Name));
        }
    }
}
