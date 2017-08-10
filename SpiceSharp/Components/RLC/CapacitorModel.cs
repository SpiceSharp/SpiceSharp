using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class represents a capacitor model
    /// </summary>
    public class CapacitorModel : CircuitModel
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("cj"), SpiceInfo("Bottom capacitance per area")]
        public Parameter<double> CAPcj { get; } = new Parameter<double>();
        [SpiceName("cjsw"), SpiceInfo("Sidewall capacitance per meter")]
        public Parameter<double> CAPcjsw { get; } = new Parameter<double>();
        [SpiceName("defw"), SpiceInfo("Default width")]
        public Parameter<double> CAPdefWidth { get; } = new Parameter<double>(10.0e-6);
        [SpiceName("narrow"), SpiceInfo("Width correction factor")]
        public Parameter<double> CAPnarrow { get; } = new Parameter<double>();
        [SpiceName("c"), SpiceInfo("Capacitor model")]
        public void SetCapFlag(Circuit ckt, bool flag) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public CapacitorModel(string name) : base(name)
        {
        }
    }
}
