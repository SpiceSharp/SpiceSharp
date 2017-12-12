using SpiceSharp.Circuits;
using SpiceSharp.Components.ComponentBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="MOS1"/>
    /// </summary>
    public class MOS1Model : CircuitModel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public MOS1Model(CircuitIdentifier name) : base(name)
        {
            RegisterBehavior(new MOS1ModelTemperatureBehavior());
            RegisterBehavior(new MOS1ModelNoiseBehavior());
        }
    }
}
