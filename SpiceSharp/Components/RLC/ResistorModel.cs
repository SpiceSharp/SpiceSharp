using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A class representing a resistor model
    /// </summary>
    public class ResistorModel : CircuitModel<ResistorModel>
    {
        /// <summary>
        /// Register our parameters
        /// </summary>
        static ResistorModel()
        {
            Register();
        }

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("tnom"), SpiceInfo("Parameter measurement temperature", Interesting = false)]
        public double RES_TNOM
        {
            get => REStnom - Circuit.CONSTCtoK;
            set => REStnom.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter REStnom { get; } = new Parameter(300.15);
        [SpiceName("tc1"), SpiceInfo("First order temperature coefficient")]
        public Parameter REStempCoeff1 { get; } = new Parameter();
        [SpiceName("tc2"), SpiceInfo("Second order temperature oefficient")]
        public Parameter REStempCoeff2 { get; } = new Parameter();
        [SpiceName("rsh"), SpiceInfo("Sheet resistance")]
        public Parameter RESsheetRes { get; } = new Parameter();
        [SpiceName("defw"), SpiceInfo("Default device width")]
        public Parameter RESdefWidth { get; } = new Parameter(10.0e-6);
        [SpiceName("narrow"), SpiceInfo("Narrowing of resistor")]
        public Parameter RESnarrow { get; } = new Parameter();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public ResistorModel(string name) : base(name)
        {
        }
    }
}
