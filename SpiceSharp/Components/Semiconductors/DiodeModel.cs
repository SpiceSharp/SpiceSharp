using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    public class DiodeModel : CircuitModel<DiodeModel>
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
        [SpiceName("kf"), SpiceInfo("flicker noise coefficient")]
        public Parameter DIOfNcoef { get; } = new Parameter();
        [SpiceName("af"), SpiceInfo("flicker noise exponent")]
        public Parameter DIOfNexp { get; } = new Parameter(1.0);
        [SpiceName("cond"), SpiceInfo("Ohmic conductance")]
        public double DIOconductance { get; private set; }

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("d"), SpiceInfo("Diode model")]
        public void SetDIO_D(bool value)
        {

        }

        /// <summary>
        /// Shared parameters
        /// </summary>
        public double vtnom { get; private set; }
        public double xfc { get; private set; }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double DIOf2 { get; private set; }
        public double DIOf3 { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public DiodeModel(string name) : base(name)
        {
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {

            if (!DIOnomTemp.Given)
            {
                DIOnomTemp.Value = ckt.State.NominalTemperature;
            }
            vtnom = Circuit.CONSTKoverQ * DIOnomTemp;
            /* limit grading coeff to max of .9 */
            if (DIOgradingCoeff > .9)
                CircuitWarning.Warning(this, $"{Name}: grading coefficient too large, limited to 0.9");

            /* limit activation energy to min of .1 */
            if (DIOactivationEnergy < .1)
                CircuitWarning.Warning(this, $"{Name}: activation energy too small, limited to 0.1");

            /* limit depletion cap coeff to max of .95 */
            if (DIOdepletionCapCoeff > .95)
                CircuitWarning.Warning(this, $"{Name}: coefficient Fc too large, limited to 0.95");
            if (!DIOresist.Given || DIOresist.Value == 0)
                DIOconductance = 0;
            else
                DIOconductance = 1 / DIOresist;
            xfc = Math.Log(1 - DIOdepletionCapCoeff);

            DIOf2 = Math.Exp((1 + DIOgradingCoeff) * xfc);
            DIOf3 = 1 - DIOdepletionCapCoeff * (1 + DIOgradingCoeff);
        }
    }
}
