using SpiceSharp.Circuits;
using SpiceSharp.Components.ComponentBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="MOS3"/>
    /// </summary>
    public class MOS3Model : CircuitModel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public MOS3Model(CircuitIdentifier name) : base(name)
        {
            RegisterBehavior(new MOS3ModelTemperatureBehavior());
            RegisterBehavior(new MOS3ModelNoiseBehavior());
        }
    }
}
