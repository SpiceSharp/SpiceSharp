using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A class representing a resistor model
    /// </summary>
    public class ResistorModel : CircuitModel
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("tnom"), SpiceInfo("Parameter measurement temperature", Interesting = false)]
        public ParameterMethod<double> REStnom { get; } = new ParameterMethod<double>(300.15, (double celsius) => celsius + Circuit.CONSTCtoK, (double kelvin) => kelvin - Circuit.CONSTCtoK);
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
