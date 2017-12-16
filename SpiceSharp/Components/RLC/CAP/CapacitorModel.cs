using SpiceSharp.Circuits;
using SpiceSharp.Behaviors.CAP;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a semiconductor <see cref="Capacitor"/>
    /// </summary>
    public class CapacitorModel : Model
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public CapacitorModel(Identifier name) : base(name)
        {
            RegisterBehavior(new ModelTemperatureBehavior());
        }
    }
}
