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
        [PropertyNameAttribute("is"), PropertyInfoAttribute("Saturation current")]
        public Parameter DIOsatCur { get; } = new Parameter(1e-14);
        [PropertyNameAttribute("tnom"), PropertyInfoAttribute("Parameter measurement temperature")]
        public double DIO_TNOM
        {
            get => DIOnomTemp - Circuit.CONSTCtoK;
            set => DIOnomTemp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter DIOnomTemp { get; } = new Parameter();
        [PropertyNameAttribute("rs"), PropertyInfoAttribute("Ohmic resistance")]
        public Parameter DIOresist { get; } = new Parameter();
        [PropertyNameAttribute("n"), PropertyInfoAttribute("Emission Coefficient")]
        public Parameter DIOemissionCoeff { get; } = new Parameter(1);
        [PropertyNameAttribute("tt"), PropertyInfoAttribute("Transit Time")]
        public Parameter DIOtransitTime { get; } = new Parameter();
        [PropertyNameAttribute("cjo"), PropertyNameAttribute("cj0"), PropertyInfoAttribute("Junction capacitance")]
        public Parameter DIOjunctionCap { get; } = new Parameter();
        [PropertyNameAttribute("vj"), PropertyInfoAttribute("Junction potential")]
        public Parameter DIOjunctionPot { get; } = new Parameter(1);
        [PropertyNameAttribute("m"), PropertyInfoAttribute("Grading coefficient")]
        public Parameter DIOgradingCoeff { get; } = new Parameter(.5);
        [PropertyNameAttribute("eg"), PropertyInfoAttribute("Activation energy")]
        public Parameter DIOactivationEnergy { get; } = new Parameter(1.11);
        [PropertyNameAttribute("xti"), PropertyInfoAttribute("Saturation current temperature exp.")]
        public Parameter DIOsaturationCurrentExp { get; } = new Parameter(3);
        [PropertyNameAttribute("fc"), PropertyInfoAttribute("Forward bias junction fit parameter")]
        public Parameter DIOdepletionCapCoeff { get; } = new Parameter(.5);
        [PropertyNameAttribute("bv"), PropertyInfoAttribute("Reverse breakdown voltage")]
        public Parameter DIObreakdownVoltage { get; } = new Parameter();
        [PropertyNameAttribute("ibv"), PropertyInfoAttribute("Current at reverse breakdown voltage")]
        public Parameter DIObreakdownCurrent { get; } = new Parameter(1e-3);
    }
}
