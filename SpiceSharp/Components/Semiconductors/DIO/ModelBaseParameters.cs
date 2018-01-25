using SpiceSharp.Attributes;

namespace SpiceSharp.Components.DIO
{
    /// <summary>
    /// Base parameters for a <see cref="DiodeModel"/>
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyName("is"), PropertyInfo("Saturation current")]
        public Parameter DIOsatCur { get; } = new Parameter(1e-14);
        [PropertyName("tnom"), PropertyInfo("Parameter measurement temperature")]
        public double DIO_TNOM
        {
            get => DIOnomTemp - Circuit.CelsiusKelvin;
            set => DIOnomTemp.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter DIOnomTemp { get; } = new Parameter();
        [PropertyName("rs"), PropertyInfo("Ohmic resistance")]
        public Parameter DIOresist { get; } = new Parameter();
        [PropertyName("n"), PropertyInfo("Emission Coefficient")]
        public Parameter DIOemissionCoeff { get; } = new Parameter(1);
        [PropertyName("tt"), PropertyInfo("Transit Time")]
        public Parameter DIOtransitTime { get; } = new Parameter();
        [PropertyName("cjo"), PropertyName("cj0"), PropertyInfo("Junction capacitance")]
        public Parameter DIOjunctionCap { get; } = new Parameter();
        [PropertyName("vj"), PropertyInfo("Junction potential")]
        public Parameter DIOjunctionPot { get; } = new Parameter(1);
        [PropertyName("m"), PropertyInfo("Grading coefficient")]
        public Parameter DIOgradingCoeff { get; } = new Parameter(.5);
        [PropertyName("eg"), PropertyInfo("Activation energy")]
        public Parameter DIOactivationEnergy { get; } = new Parameter(1.11);
        [PropertyName("xti"), PropertyInfo("Saturation current temperature exp.")]
        public Parameter DIOsaturationCurrentExp { get; } = new Parameter(3);
        [PropertyName("fc"), PropertyInfo("Forward bias junction fit parameter")]
        public Parameter DIOdepletionCapCoeff { get; } = new Parameter(.5);
        [PropertyName("bv"), PropertyInfo("Reverse breakdown voltage")]
        public Parameter DIObreakdownVoltage { get; } = new Parameter();
        [PropertyName("ibv"), PropertyInfo("Current at reverse breakdown voltage")]
        public Parameter DIObreakdownCurrent { get; } = new Parameter(1e-3);
    }
}
