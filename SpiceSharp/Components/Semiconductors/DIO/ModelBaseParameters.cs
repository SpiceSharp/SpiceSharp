using SpiceSharp.Attributes;

namespace SpiceSharp.Components.DIO
{
    /// <summary>
    /// Base parameters for a <see cref="DiodeModel"/>
    /// </summary>
    public class ModelBaseParameters : Parameters
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("is"), SpiceInfo("Saturation current")]
        public Parameter DIOsatCur { get; } = new Parameter(1e-14);
        [SpiceName("tnom"), SpiceInfo("Parameter measurement temperature")]
        public double DIO_TNOM
        {
            get => DIOnomTemp - Circuit.CONSTCtoK;
            set => DIOnomTemp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter DIOnomTemp { get; } = new Parameter();
        [SpiceName("rs"), SpiceInfo("Ohmic resistance")]
        public Parameter DIOresist { get; } = new Parameter();
        [SpiceName("n"), SpiceInfo("Emission Coefficient")]
        public Parameter DIOemissionCoeff { get; } = new Parameter(1);
        [SpiceName("tt"), SpiceInfo("Transit Time")]
        public Parameter DIOtransitTime { get; } = new Parameter();
        [SpiceName("cjo"), SpiceName("cj0"), SpiceInfo("Junction capacitance")]
        public Parameter DIOjunctionCap { get; } = new Parameter();
        [SpiceName("vj"), SpiceInfo("Junction potential")]
        public Parameter DIOjunctionPot { get; } = new Parameter(1);
        [SpiceName("m"), SpiceInfo("Grading coefficient")]
        public Parameter DIOgradingCoeff { get; } = new Parameter(.5);
        [SpiceName("eg"), SpiceInfo("Activation energy")]
        public Parameter DIOactivationEnergy { get; } = new Parameter(1.11);
        [SpiceName("xti"), SpiceInfo("Saturation current temperature exp.")]
        public Parameter DIOsaturationCurrentExp { get; } = new Parameter(3);
        [SpiceName("fc"), SpiceInfo("Forward bias junction fit parameter")]
        public Parameter DIOdepletionCapCoeff { get; } = new Parameter(.5);
        [SpiceName("bv"), SpiceInfo("Reverse breakdown voltage")]
        public Parameter DIObreakdownVoltage { get; } = new Parameter();
        [SpiceName("ibv"), SpiceInfo("Current at reverse breakdown voltage")]
        public Parameter DIObreakdownCurrent { get; } = new Parameter(1e-3);
    }
}
