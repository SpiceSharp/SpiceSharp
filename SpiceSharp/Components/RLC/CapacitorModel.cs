using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class represents a capacitor model
    /// </summary>
    public class CapacitorModel : Parameterized
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

        /// <summary>
        /// Load the data for this model
        /// </summary>
        /// <param name="ckt"></param>
        /// <returns></returns>
        public bool Load(Circuit ckt)
        {
            if (!ckt.State.IsDc)
                return false;
            return (ckt.State.IsDc && ckt.State.Init.HasFlag(CircuitState.InitFlags.InitJct)) || ckt.State.UseIC;
        }
    }
}
