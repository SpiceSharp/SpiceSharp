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
            // CurrentSwitch has a priority of -1, so this needs to be even earlier
            Priority = -2;

            // Add parameters
            ParameterSets.Add(new ModelBaseParameters());

            // Add factories
            Behaviors.Add(typeof(ModelLoadBehavior), () => new ModelLoadBehavior(Name));
        }
    }
}
