using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Parameters;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class represents a model for a diode
    /// </summary>
    public class DiodeModel : Parameterized
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("is"), SpiceInfo("Saturation current")]
        public Parameter<double> DIOsatCur { get; } = new Parameter<double>(1.0e-14);
        [SpiceName("tnom"), SpiceInfo("Parameer measurement temperature", Interesting = false)]
        public Parameter<double> DIOnomTemp { get; } = new Parameter<double>(300.15);
        [SpiceName("rs"), SpiceInfo("Ohmic resistance")]
        public Parameter<double> DIOresist { get; } = new Parameter<double>();
        [SpiceName("n"), SpiceInfo("Emission Coefficient")]
        public Parameter<double> DIOemissionCoeff { get; } = new Parameter<double>(1.0);
        [SpiceName("tt"), SpiceInfo("Transit Time")]
        public Parameter<double> DIOtransitTime { get; } = new Parameter<double>();
        [SpiceName("cjo"), SpiceName("cj0"), SpiceInfo("Junction capacitance")]
        public Parameter<double> DIOjunctionCap { get; } = new Parameter<double>();
        [SpiceName("vj"), SpiceInfo("Junction potential")]
        public Parameter<double> DIOjunctionPot { get; } = new Parameter<double>(1.0);
        [SpiceName("m"), SpiceInfo("Grading coefficient")]
        public Parameter<double> DIOgradingCoeff { get; } = new Parameter<double>(0.5);
        [SpiceName("eg"), SpiceInfo("Activation energy")]
        public Parameter<double> DIOactivationEnergy { get; } = new Parameter<double>(1.11);
        [SpiceName("xti"), SpiceInfo("Saturation current temperature exp.")]
        public Parameter<double> DIOsaturationCurrentExp { get; } = new Parameter<double>(3.0);
        [SpiceName("kf"), SpiceInfo("Flicker noise coefficnet")]
        public Parameter<double> DIOfNcoef { get; } = new Parameter<double>();
        [SpiceName("af"), SpiceInfo("Flicker noise exponent")]
        public Parameter<double> DIOfNexp { get; } = new Parameter<double>(1.0);
        [SpiceName("fc"), SpiceInfo("Forward bias junction fit parameter")]
        public Parameter<double> DIOdepletionCapCoeff { get; } = new Parameter<double>(0.5);
        [SpiceName("bv"), SpiceInfo("Reverse breakdown voltage")]
        public Parameter<double> DIObreakdownVoltage { get; } = new Parameter<double>();
        [SpiceName("ibv"), SpiceInfo("Current at reverse breakdown voltage")]
        public Parameter<double> DIObreakdownCurrent { get; } = new Parameter<double>(1e-3);

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
        public void Temperature(Circuit ckt)
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
            DIOf3 = 1.0 - DIOdepletionCapCoeff *
                    (1.0 + DIOgradingCoeff);
        }
    }
}
