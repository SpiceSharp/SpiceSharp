using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A class representing a resistor model
    /// </summary>
    public class ResistorModel : Parameterized
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("tnom"), SpiceInfo("Parameter measurement temperature (in Kelvin)", Interesting = false)]
        public Parameter<double> REStnom { get; } = new Parameter<double>();
        [SpiceName("tc1"), SpiceInfo("First order temperature coefficient")]
        public Parameter<double> REStempCoeff1 { get; } = new Parameter<double>();
        [SpiceName("tc2"), SpiceInfo("Second order temperature oefficient")]
        public Parameter<double> REStempCoeff2 { get; } = new Parameter<double>();
        [SpiceName("rsh"), SpiceInfo("Sheet resistance")]
        public Parameter<double> RESsheetRes { get; } = new Parameter<double>();
        [SpiceName("defw"), SpiceInfo("Default device width")]
        public Parameter<double> RESdefWidth { get; } = new Parameter<double>(10.0e-6);
        [SpiceName("narrow"), SpiceInfo("Narrowing of resistor")]
        public Parameter<double> RESnarrow { get; } = new Parameter<double>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public ResistorModel(string name) : base(name)
        {
        }
    }
}
