using SpiceSharp.Attributes;

namespace SpiceSharp.Components.ResistorBehaviors
{
    /// <summary>
    /// Parameters for a <see cref="ResistorModel"/>
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyName("tnom"), PropertyInfo("Parameter measurement temperature", Interesting = false)]
        public double RES_TNOM
        {
            get => REStnom - Circuit.CelsiusKelvin;
            set => REStnom.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter REStnom { get; } = new Parameter(300.15);
        [PropertyName("tc1"), PropertyInfo("First order temperature coefficient")]
        public Parameter REStempCoeff1 { get; } = new Parameter();
        [PropertyName("tc2"), PropertyInfo("Second order temperature oefficient")]
        public Parameter REStempCoeff2 { get; } = new Parameter();
        [PropertyName("rsh"), PropertyInfo("Sheet resistance")]
        public Parameter RESsheetRes { get; } = new Parameter();
        [PropertyName("defw"), PropertyInfo("Default device width")]
        public Parameter RESdefWidth { get; } = new Parameter(10.0e-6);
        [PropertyName("narrow"), PropertyInfo("Narrowing of resistor")]
        public Parameter RESnarrow { get; } = new Parameter();
    }
}
