using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    public class BJTModel : CircuitModel
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("tnom"), SpiceInfo("Parameter measurement temperature")]
        public ParameterMethod<double> BJTtnom { get; } = new ParameterMethod<double>(300.15, (double celsius) => celsius + Circuit.CONSTCtoK, (double kelvin) => kelvin - Circuit.CONSTCtoK);
        [SpiceName("is"), SpiceInfo("Saturation Current")]
        public Parameter<double> BJTsatCur { get; } = new Parameter<double>(1e-16);
        [SpiceName("bf"), SpiceInfo("Ideal forward beta")]
        public Parameter<double> BJTbetaF { get; } = new Parameter<double>(100);
        [SpiceName("nf"), SpiceInfo("Forward emission coefficient")]
        public Parameter<double> BJTemissionCoeffF { get; } = new Parameter<double>(1);
        [SpiceName("vaf"), SpiceName("va"), SpiceInfo("Forward Early voltage")]
        public Parameter<double> BJTearlyVoltF { get; } = new Parameter<double>();
        [SpiceName("ikf"), SpiceName("ik"), SpiceInfo("Forward beta roll-off corner current")]
        public Parameter<double> BJTrollOffF { get; } = new Parameter<double>();
        [SpiceName("ise"), SpiceInfo("B-E leakage saturation current")]
        public Parameter<double> BJTleakBEcurrent { get; } = new Parameter<double>();
        [SpiceName("ne"), SpiceInfo("B-E leakage emission coefficient")]
        public Parameter<double> BJTleakBEemissionCoeff { get; } = new Parameter<double>(1.5);
        [SpiceName("br"), SpiceInfo("Ideal reverse beta")]
        public Parameter<double> BJTbetaR { get; } = new Parameter<double>(1);
        [SpiceName("nr"), SpiceInfo("Reverse emission coefficient")]
        public Parameter<double> BJTemissionCoeffR { get; } = new Parameter<double>(1);
        [SpiceName("var"), SpiceName("vb"), SpiceInfo("Reverse Early voltage")]
        public Parameter<double> BJTearlyVoltR { get; } = new Parameter<double>();
        [SpiceName("ikr"), SpiceInfo("reverse beta roll-off corner current")]
        public Parameter<double> BJTrollOffR { get; } = new Parameter<double>();
        [SpiceName("isc"), SpiceInfo("B-C leakage saturation current")]
        public Parameter<double> BJTleakBCcurrent { get; } = new Parameter<double>();
        [SpiceName("nc"), SpiceInfo("B-C leakage emission coefficient")]
        public Parameter<double> BJTleakBCemissionCoeff { get; } = new Parameter<double>(2);
        [SpiceName("rb"), SpiceInfo("Zero bias base resistance")]
        public Parameter<double> BJTbaseResist { get; } = new Parameter<double>();
        [SpiceName("irb"), SpiceInfo("Current for base resistance=(rb+rbm)/2")]
        public Parameter<double> BJTbaseCurrentHalfResist { get; } = new Parameter<double>();
        [SpiceName("rbm"), SpiceInfo("Minimum base resistance")]
        public Parameter<double> BJTminBaseResist { get; } = new Parameter<double>();
        [SpiceName("re"), SpiceInfo("Emitter resistance")]
        public Parameter<double> BJTemitterResist { get; } = new Parameter<double>();
        [SpiceName("rc"), SpiceInfo("Collector resistance")]
        public Parameter<double> BJTcollectorResist { get; } = new Parameter<double>();
        [SpiceName("cje"), SpiceInfo("Zero bias B-E depletion capacitance")]
        public Parameter<double> BJTdepletionCapBE { get; } = new Parameter<double>();
        [SpiceName("vje"), SpiceName("pe"), SpiceInfo("B-E built in potential")]
        public Parameter<double> BJTpotentialBE { get; } = new Parameter<double>(.75);
        [SpiceName("mje"), SpiceName("me"), SpiceInfo("B-E junction grading coefficient")]
        public Parameter<double> BJTjunctionExpBE { get; } = new Parameter<double>(.33);
        [SpiceName("tf"), SpiceInfo("Ideal forward transit time")]
        public Parameter<double> BJTtransitTimeF { get; } = new Parameter<double>();
        [SpiceName("xtf"), SpiceInfo("Coefficient for bias dependence of TF")]
        public Parameter<double> BJTtransitTimeBiasCoeffF { get; } = new Parameter<double>();
        [SpiceName("vtf"), SpiceInfo("Voltage giving VBC dependence of TF")]
        public Parameter<double> BJTtransitTimeFVBC { get; } = new Parameter<double>();
        [SpiceName("itf"), SpiceInfo("High current dependence of TF")]
        public Parameter<double> BJTtransitTimeHighCurrentF { get; } = new Parameter<double>();
        [SpiceName("ptf"), SpiceInfo("Excess phase")]
        public Parameter<double> BJTexcessPhase { get; } = new Parameter<double>();
        [SpiceName("cjc"), SpiceInfo("Zero bias B-C depletion capacitance")]
        public Parameter<double> BJTdepletionCapBC { get; } = new Parameter<double>();
        [SpiceName("vjc"), SpiceName("pc"), SpiceInfo("B-C built in potential")]
        public Parameter<double> BJTpotentialBC { get; } = new Parameter<double>(.75);
        [SpiceName("mjc"), SpiceName("mc"), SpiceInfo("B-C junction grading coefficient")]
        public Parameter<double> BJTjunctionExpBC { get; } = new Parameter<double>(.33);
        [SpiceName("xcjc"), SpiceInfo("Fraction of B-C cap to internal base")]
        public Parameter<double> BJTbaseFractionBCcap { get; } = new Parameter<double>(1);
        [SpiceName("tr"), SpiceInfo("Ideal reverse transit time")]
        public Parameter<double> BJTtransitTimeR { get; } = new Parameter<double>();
        [SpiceName("cjs"), SpiceName("ccs"), SpiceInfo("Zero bias C-S capacitance")]
        public Parameter<double> BJTcapCS { get; } = new Parameter<double>();
        [SpiceName("vjs"), SpiceName("ps"), SpiceInfo("Substrate junction built in potential")]
        public Parameter<double> BJTpotentialSubstrate { get; } = new Parameter<double>(.75);
        [SpiceName("mjs"), SpiceName("ms"), SpiceInfo("Substrate junction grading coefficient")]
        public Parameter<double> BJTexponentialSubstrate { get; } = new Parameter<double>();
        [SpiceName("xtb"), SpiceInfo("Forward and reverse beta temp. exp.")]
        public Parameter<double> BJTbetaExp { get; } = new Parameter<double>();
        [SpiceName("eg"), SpiceInfo("Energy gap for IS temp. dependency")]
        public Parameter<double> BJTenergyGap { get; } = new Parameter<double>(1.11);
        [SpiceName("xti"), SpiceInfo("Temp. exponent for IS")]
        public Parameter<double> BJTtempExpIS { get; } = new Parameter<double>(3);
        [SpiceName("fc"), SpiceInfo("Forward bias junction fit parameter")]
        public Parameter<double> BJTdepletionCapCoeff { get; } = new Parameter<double>();
        [SpiceName("kf"), SpiceInfo("Flicker Noise Coefficient")]
        public Parameter<double> BJTfNcoef = new Parameter<double>();
        [SpiceName("af"), SpiceInfo("Flicker Noise Exponent")]
        public Parameter<double> BJTfNexp = new Parameter<double>(1);
        [SpiceName("invearlyvoltf"), SpiceInfo("Inverse early voltage:forward")]
        public double BJTinvEarlyVoltF { get; private set; }
        [SpiceName("invearlyvoltr"), SpiceInfo("Inverse early voltage:reverse")]
        public double BJTinvEarlyVoltR { get; private set; }
        [SpiceName("invrollofff"), SpiceInfo("Inverse roll off - forward")]
        public double BJTinvRollOffF { get; private set; }
        [SpiceName("invrolloffr"), SpiceInfo("Inverse roll off - reverse")]
        public double BJTinvRollOffR { get; private set; }
        [SpiceName("collectorconduct"), SpiceInfo("Collector conductance")]
        public double BJTcollectorConduct { get; private set; }
        [SpiceName("emitterconduct"), SpiceInfo("Emitter conductance")]
        public double BJTemitterConduct { get; private set; }
        [SpiceName("transtimevbcfact"), SpiceInfo("Transit time VBC factor")]
        public double BJTtransitTimeVBCFactor { get; private set; }
        [SpiceName("excessphasefactor"), SpiceInfo("Excess phase fact.")]
        public double BJTexcessPhaseFactor { get; private set; }

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("npn"), SpiceInfo("NPN type device")]
        public void SetNPN(bool value)
        {
            if (value)
                BJTtype = NPN;
        }
        [SpiceName("pnp"), SpiceInfo("PNP type device")]
        public void SetPNP(bool value)
        {
            if (value)
                BJTtype = PNP;
        }
        [SpiceName("type"), SpiceInfo("NPN or PNP")]
        public string GetTYPE(Circuit ckt)
        {
            if (BJTtype == NPN)
                return "npn";
            else
                return "pnp";
        }

        /// <summary>
        /// Shared parameters
        /// </summary>
        public double fact1 { get; private set; }
        public double xfc { get; private set; }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double BJTtype { get; private set; }
        public Parameter<double> BJTc2 { get; } = new Parameter<double>();
        public Parameter<double> BJTc4 { get; } = new Parameter<double>();
        public double BJTf2 { get; private set; }
        public double BJTf3 { get; private set; }
        public double BJTf6 { get; private set; }
        public double BJTf7 { get; private set; }

        private const int NPN = 1;
        private const int PNP = -1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BJTModel(string name) : base(name)
        {
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {

            if (BJTtype != NPN && BJTtype != PNP)
                BJTtype = NPN;

            /* 
			* COMPATABILITY WARNING!
			* special note:  for backward compatability to much older models, spice 2G
			* implemented a special case which checked if B - E leakage saturation
			* current was >1, then it was instead a the B - E leakage saturation current
			* divided by IS, and multiplied it by IS at this point.  This was not
			* handled correctly in the 2G code, and there is some question on its 
			* reasonability, since it is also undocumented, so it has been left out
			* here.  It could easily be added with 1 line.  (The same applies to the B - C
			* leakage saturation current).   TQ  6 / 29 / 84
			*/

            /* loop through all the instances of the model */
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {

            if (!BJTtnom.Given)
                BJTtnom.Value = ckt.State.NominalTemperature;
            fact1 = BJTtnom / Circuit.CONSTRefTemp;

            if (!BJTleakBEcurrent.Given)
            {
                if (BJTc2.Given)
                    BJTleakBEcurrent.Value = BJTc2 * BJTsatCur;
                else
                    BJTleakBEcurrent.Value = 0;
            }
            if (!BJTleakBCcurrent.Given)
            {
                if (BJTc4.Given)
                    BJTleakBCcurrent.Value = BJTc4 * BJTsatCur;
                else
                    BJTleakBCcurrent.Value = 0;
            }
            if (!BJTminBaseResist.Given)
                BJTminBaseResist.Value = BJTbaseResist;

            /* 
			* COMPATABILITY WARNING!
			* special note:  for backward compatability to much older models, spice 2G
			* implemented a special case which checked if B - E leakage saturation
			* current was >1, then it was instead a the B - E leakage saturation current
			* divided by IS, and multiplied it by IS at this point.  This was not
			* handled correctly in the 2G code, and there is some question on its 
			* reasonability, since it is also undocumented, so it has been left out
			* here.  It could easily be added with 1 line.  (The same applies to the B - C
			* leakage saturation current).   TQ  6 / 29 / 84
			*/

            if (BJTearlyVoltF.Given && BJTearlyVoltF != 0)
                BJTinvEarlyVoltF = 1 / BJTearlyVoltF;
            else
                BJTinvEarlyVoltF = 0;
            if (BJTrollOffF.Given && BJTrollOffF != 0)
                BJTinvRollOffF = 1 / BJTrollOffF;
            else
                BJTinvRollOffF = 0;
            if (BJTearlyVoltR.Given && BJTearlyVoltR != 0)
                BJTinvEarlyVoltR = 1 / BJTearlyVoltR;
            else
                BJTinvEarlyVoltR = 0;
            if (BJTrollOffR.Given && BJTrollOffR != 0)
                BJTinvRollOffR = 1 / BJTrollOffR;
            else
                BJTinvRollOffR = 0;
            if (BJTcollectorResist.Given && BJTcollectorResist != 0)
                BJTcollectorConduct = 1 / BJTcollectorResist;
            else
                BJTcollectorConduct = 0;
            if (BJTemitterResist.Given && BJTemitterResist != 0)
                BJTemitterConduct = 1 / BJTemitterResist;
            else
                BJTemitterConduct = 0;
            if (BJTtransitTimeFVBC.Given && BJTtransitTimeFVBC != 0)
                BJTtransitTimeVBCFactor = 1 / (BJTtransitTimeFVBC * 1.44);
            else
                BJTtransitTimeVBCFactor = 0;
            BJTexcessPhaseFactor = (BJTexcessPhase / (180.0 / Circuit.CONSTPI)) * BJTtransitTimeF;
            if (BJTdepletionCapCoeff.Given)
            {
                if (BJTdepletionCapCoeff > 0.9999)
                {
                    BJTdepletionCapCoeff.Value = 0.9999;
                    throw new CircuitException($"BJT model {Name}, parameter fc limited to 0.9999");
                }
            }
            else
            {
                BJTdepletionCapCoeff.Value = .5;
            }
            xfc = Math.Log(1 - BJTdepletionCapCoeff);
            BJTf2 = Math.Exp((1 + BJTjunctionExpBE) * xfc);
            BJTf3 = 1 - BJTdepletionCapCoeff * (1 + BJTjunctionExpBE);
            BJTf6 = Math.Exp((1 + BJTjunctionExpBC) * xfc);
            BJTf7 = 1 - BJTdepletionCapCoeff * (1 + BJTjunctionExpBC);

            /* loop through all the instances of the model */
        }
    }
}
