using SpiceSharp.Circuits;
using SpiceSharp.Behaviors.MOS1;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="MOS1"/>
    /// </summary>
    public class MOS1Model : Model
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public MOS1Model(CircuitIdentifier name) : base(name)
        {
            RegisterBehavior(new ModelTemperatureBehavior());
            RegisterBehavior(new ModelNoiseBehavior());
        }
    }
}
