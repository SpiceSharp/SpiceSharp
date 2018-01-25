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
        [PropertyNameAttribute("tnom"), PropertyInfoAttribute("Parameter measurement temperature", Interesting = false)]
        public double RES_TNOM
        {
            get => REStnom - Circuit.CONSTCtoK;
            set => REStnom.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter REStnom { get; } = new Parameter(300.15);
        [PropertyNameAttribute("tc1"), PropertyInfoAttribute("First order temperature coefficient")]
        public Parameter REStempCoeff1 { get; } = new Parameter();
        [PropertyNameAttribute("tc2"), PropertyInfoAttribute("Second order temperature oefficient")]
        public Parameter REStempCoeff2 { get; } = new Parameter();
        [PropertyNameAttribute("rsh"), PropertyInfoAttribute("Sheet resistance")]
        public Parameter RESsheetRes { get; } = new Parameter();
        [PropertyNameAttribute("defw"), PropertyInfoAttribute("Default device width")]
        public Parameter RESdefWidth { get; } = new Parameter(10.0e-6);
        [PropertyNameAttribute("narrow"), PropertyInfoAttribute("Narrowing of resistor")]
        public Parameter RESnarrow { get; } = new Parameter();
    }
}
