using SpiceSharp.Circuits;
using SpiceSharp.Parameters;

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
        [SpiceName("cj"), SpiceInfo("Bottom capacitance per area")]
        public Parameter CAPcj { get; } = new Parameter();
        [SpiceName("cjsw"), SpiceInfo("Sidewall capacitance per meter")]
        public Parameter CAPcjsw { get; } = new Parameter();
        [SpiceName("defw"), SpiceInfo("Default width")]
        public Parameter CAPdefWidth { get; } = new Parameter(10.0e-6);
        [SpiceName("narrow"), SpiceInfo("Width correction factor")]
        public Parameter CAPnarrow { get; } = new Parameter();
        [SpiceName("c"), SpiceInfo("Capacitor model")]
        public void SetCapFlag(Circuit ckt, bool flag) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public CapacitorModel(CircuitIdentifier name) : base(name)
        {
        }
    }
}
