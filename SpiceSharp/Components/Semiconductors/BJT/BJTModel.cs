using SpiceSharp.Circuits;
using SpiceSharp.Behaviors.BJT;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="BJT"/>
    /// </summary>
    public class BJTModel : CircuitModel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BJTModel(CircuitIdentifier name) : base(name)
        {
            RegisterBehavior(new ModelTemperatureBehavior());
            RegisterBehavior(new ModelNoiseBehavior());
        }
    }
}
