using SpiceSharp.Circuits;
using SpiceSharp.Behaviors.DIO;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="Diode"/>
    /// </summary>
    public class DiodeModel : Model
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public DiodeModel(Identifier name) : base(name)
        {
            RegisterBehavior(new ModelTemperatureBehavior());
        }
    }
}
