using SpiceSharp.Circuits;
using SpiceSharp.Behaviors.MOS2;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="MOS2"/>
    /// </summary>
    public class MOS2Model : Model
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public MOS2Model(CircuitIdentifier name) : base(name)
        {
            RegisterBehavior(new ModelTemperatureBehavior());
        }
    }
}
