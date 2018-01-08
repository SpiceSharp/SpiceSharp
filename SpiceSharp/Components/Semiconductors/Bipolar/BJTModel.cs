using SpiceSharp.Circuits;
using SpiceSharp.Behaviors.Bipolar;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="BJT"/>
    /// </summary>
    public class BJTModel : Model
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BJTModel(Identifier name) : base(name)
        {
            RegisterBehavior(new ModelTemperatureBehavior());
            RegisterBehavior(new ModelNoiseBehavior());
        }
    }
}
