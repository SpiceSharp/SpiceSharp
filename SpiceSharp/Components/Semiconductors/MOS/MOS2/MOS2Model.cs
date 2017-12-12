using SpiceSharp.Circuits;
using SpiceSharp.Components.ComponentBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="MOS2"/>
    /// </summary>
    public class MOS2Model : CircuitModel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public MOS2Model(CircuitIdentifier name) : base(name)
        {
            RegisterBehavior(new MOS2ModelTemperatureBehavior());
        }
    }
}
