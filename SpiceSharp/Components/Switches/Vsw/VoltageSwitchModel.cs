using SpiceSharp.Circuits;
using SpiceSharp.Behaviors.VSW;

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
        public VoltageSwitchModel(CircuitIdentifier name) : base(name)
        {
            RegisterBehavior(new ModelLoadBehavior());
        }
    }
}
