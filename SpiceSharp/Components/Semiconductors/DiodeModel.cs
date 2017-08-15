using System;
using SpiceSharp.Parameters;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class represents a model for a diode
    /// </summary>
    public class DiodeModel : CircuitModel<DiodeModel>
    {
        /// <summary>
        /// Register our parameters
        /// </summary>
        static DiodeModel()
        {
            Register();
        }

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("is"), SpiceInfo("Saturation current")]
        public Parameter DIOsatCur { get; } = new Parameter(1.0e-14);
        [SpiceName("tnom"), SpiceInfo("Parameer measurement temperature in Kelvin", Interesting = false)]
        public Parameter DIOnomTemp { get; } = new Parameter(300.15);
        [SpiceName("rs"), SpiceInfo("Ohmic resistance")]
        public Parameter DIOresist { get; } = new Parameter();
        [SpiceName("n"), SpiceInfo("Emission Coefficient")]
        public Parameter DIOemissionCoeff { get; } = new Parameter(1.0);
        [SpiceName("tt"), SpiceInfo("Transit Time")]
        public Parameter DIOtransitTime { get; } = new Parameter();
        [SpiceName("cjo"), SpiceName("cj0"), SpiceInfo("Junction capacitance")]
        public Parameter DIOjunctionCap { get; } = new Parameter();
        [SpiceName("vj"), SpiceInfo("Junction potential")]
        public Parameter DIOjunctionPot { get; } = new Parameter(1.0);
        [SpiceName("m"), SpiceInfo("Grading coefficient")]
        public Parameter DIOgradingCoeff { get; } = new Parameter(0.5);
        [SpiceName("eg"), SpiceInfo("Activation energy")]
        public Parameter DIOactivationEnergy { get; } = new Parameter(1.11);
        [SpiceName("xti"), SpiceInfo("Saturation current temperature exp.")]
        public Parameter DIOsaturationCurrentExp { get; } = new Parameter(3.0);
        [SpiceName("kf"), SpiceInfo("Flicker noise coefficnet")]
        public Parameter DIOfNcoef { get; } = new Parameter();
        [SpiceName("af"), SpiceInfo("Flicker noise exponent")]
        public Parameter DIOfNexp { get; } = new Parameter(1.0);
        [SpiceName("fc"), SpiceInfo("Forward bias junction fit parameter")]
        public Parameter DIOdepletionCapCoeff { get; } = new Parameter(0.5);
        [SpiceName("bv"), SpiceInfo("Reverse breakdown voltage")]
        public Parameter DIObreakdownVoltage { get; } = new Parameter();
        [SpiceName("ibv"), SpiceInfo("Current at reverse breakdown voltage")]
        public Parameter DIObreakdownCurrent { get; } = new Parameter(1e-3);

        [SpiceName("cond"), SpiceInfo("Ohmic conductance", Interesting = false)]
        public double DIOconductance { get; private set; }
        public double DIOf2 { get; private set; }
        public double DIOf3 { get; private set; }
        public double xfc { get; private set; }
        public double vtnom { get; private set; }

        /// <summary>
        /// Gets the temperature this model is calculated for
        /// </summary>
        public double CurrentTemperature { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the model</param>
        public DiodeModel(string name) : base(name) { }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            var state = ckt.State;
            CurrentTemperature = state.Temperature;
            
            if (!DIOnomTemp.Given)
                DIOnomTemp.Value = state.NominalTemperature;
            vtnom = Circuit.CONSTKoverQ * DIOnomTemp;

            // limit grading coeff to max of .9
            if (DIOgradingCoeff > .9)
            {
                CircuitWarning.Warning(this, string.Format("Model {0}: Grading coefficient too large, limited to 0.9", Name));
                DIOgradingCoeff.Value = 0.9;
            }

            // limit activation energy to min of .1
            if (DIOactivationEnergy < 0.1)
            {
                CircuitWarning.Warning(this, string.Format("Model {0}: Activation energy too small, limited to 0.1", Name));
                DIOactivationEnergy.Value = 0.1;
            }

            // limit depletion cap coeff to max of .95
            if (DIOdepletionCapCoeff > .95)
            {
                CircuitWarning.Warning(this, string.Format("Model {0}: Coefficient Fc too large, limited to 0.95", Name));
                DIOdepletionCapCoeff.Value = 0.95;
            }
            if (!DIOresist.Given || DIOresist == 0)
                DIOconductance = 0;
            else
                DIOconductance = 1.0 / DIOresist;
            xfc = Math.Log(1.0 - DIOdepletionCapCoeff);

            DIOf2 = Math.Exp((1.0 + DIOgradingCoeff) * xfc);
            DIOf3 = 1.0 - DIOdepletionCapCoeff * (1.0 + DIOgradingCoeff);
        }
    }
}
