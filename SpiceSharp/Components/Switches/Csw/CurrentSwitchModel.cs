using SpiceSharp.Circuits;
using SpiceSharp.Behaviors.CSW;

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
        public CurrentSwitchModel(CircuitIdentifier name) : base(name)
        {
            // CurrentSwitch has a priority of -1, so this needs to be even earlier
            Priority = -2;
            RegisterBehavior(new ModelLoadBehavior());
        }
    }
}
