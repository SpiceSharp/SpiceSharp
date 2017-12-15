using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Components.ComponentBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a semiconductor <see cref="Capacitor"/>
    /// </summary>
    public class CapacitorModel : CircuitModel
    {
        /// <summary>
        /// Parameters
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="flag">Flag</param>
        [SpiceName("c"), SpiceInfo("Capacitor model")]
        public void SetCapFlag(Circuit ckt, bool flag) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public CapacitorModel(CircuitIdentifier name) : base(name)
        {
            RegisterBehavior(new CapacitorModelTemperatureBehavior());
        }
    }
}
