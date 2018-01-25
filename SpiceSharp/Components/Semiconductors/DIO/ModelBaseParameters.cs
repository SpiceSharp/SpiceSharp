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
        [NameAttribute("is"), InfoAttribute("Saturation current")]
        public Parameter DIOsatCur { get; } = new Parameter(1e-14);
        [NameAttribute("tnom"), InfoAttribute("Parameter measurement temperature")]
        public double DIO_TNOM
        {
            get => DIOnomTemp - Circuit.CONSTCtoK;
            set => DIOnomTemp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter DIOnomTemp { get; } = new Parameter();
        [NameAttribute("rs"), InfoAttribute("Ohmic resistance")]
        public Parameter DIOresist { get; } = new Parameter();
        [NameAttribute("n"), InfoAttribute("Emission Coefficient")]
        public Parameter DIOemissionCoeff { get; } = new Parameter(1);
        [NameAttribute("tt"), InfoAttribute("Transit Time")]
        public Parameter DIOtransitTime { get; } = new Parameter();
        [NameAttribute("cjo"), NameAttribute("cj0"), InfoAttribute("Junction capacitance")]
        public Parameter DIOjunctionCap { get; } = new Parameter();
        [NameAttribute("vj"), InfoAttribute("Junction potential")]
        public Parameter DIOjunctionPot { get; } = new Parameter(1);
        [NameAttribute("m"), InfoAttribute("Grading coefficient")]
        public Parameter DIOgradingCoeff { get; } = new Parameter(.5);
        [NameAttribute("eg"), InfoAttribute("Activation energy")]
        public Parameter DIOactivationEnergy { get; } = new Parameter(1.11);
        [NameAttribute("xti"), InfoAttribute("Saturation current temperature exp.")]
        public Parameter DIOsaturationCurrentExp { get; } = new Parameter(3);
        [NameAttribute("fc"), InfoAttribute("Forward bias junction fit parameter")]
        public Parameter DIOdepletionCapCoeff { get; } = new Parameter(.5);
        [NameAttribute("bv"), InfoAttribute("Reverse breakdown voltage")]
        public Parameter DIObreakdownVoltage { get; } = new Parameter();
        [NameAttribute("ibv"), InfoAttribute("Current at reverse breakdown voltage")]
        public Parameter DIObreakdownCurrent { get; } = new Parameter(1e-3);
    }
}
