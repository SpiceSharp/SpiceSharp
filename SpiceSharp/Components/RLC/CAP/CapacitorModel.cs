using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Behaviors.CAP;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a semiconductor <see cref="Capacitor"/>
    /// </summary>
    public class CapacitorModel : Model
    {
        /// <summary>
        /// Parameters
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="flag">Flag</param>
        [SpiceName("c"), SpiceInfo("Capacitor model")]
        public void SetCapFlag(Circuit ckt, bool flag)
        {
            // Do nothing...
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public CapacitorModel(CircuitIdentifier name) : base(name)
        {
            RegisterBehavior(new ModelTemperatureBehavior());
        }
    }
}
