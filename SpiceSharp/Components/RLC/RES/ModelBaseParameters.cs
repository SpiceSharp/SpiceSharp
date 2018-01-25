using SpiceSharp.Attributes;

namespace SpiceSharp.Components.RES
{
    /// <summary>
    /// Parameters for a <see cref="ResistorModel"/>
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [NameAttribute("tnom"), InfoAttribute("Parameter measurement temperature", Interesting = false)]
        public double RES_TNOM
        {
            get => REStnom - Circuit.CONSTCtoK;
            set => REStnom.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter REStnom { get; } = new Parameter(300.15);
        [NameAttribute("tc1"), InfoAttribute("First order temperature coefficient")]
        public Parameter REStempCoeff1 { get; } = new Parameter();
        [NameAttribute("tc2"), InfoAttribute("Second order temperature oefficient")]
        public Parameter REStempCoeff2 { get; } = new Parameter();
        [NameAttribute("rsh"), InfoAttribute("Sheet resistance")]
        public Parameter RESsheetRes { get; } = new Parameter();
        [NameAttribute("defw"), InfoAttribute("Default device width")]
        public Parameter RESdefWidth { get; } = new Parameter(10.0e-6);
        [NameAttribute("narrow"), InfoAttribute("Narrowing of resistor")]
        public Parameter RESnarrow { get; } = new Parameter();
    }
}
