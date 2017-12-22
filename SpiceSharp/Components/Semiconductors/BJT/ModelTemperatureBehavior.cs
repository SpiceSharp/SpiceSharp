using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Circuits;
using SpiceSharp.Attributes;

namespace SpiceSharp.Behaviors.BJT
{
    /// <summary>
    /// Temperature behavior for a <see cref="Components.BJTModel"/>
    /// </summary>
    public class ModelTemperatureBehavior : Behaviors.TemperatureBehavior
    {
        /// <summary>
        /// Parameters
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
        public double BJTtype { get; internal set; }
        [SpiceName("tnom"), SpiceInfo("Parameter measurement temperature")]
        public double BJT_TNOM
        {
            get => BJTtnom - Circuit.CONSTCtoK;
            set => BJTtnom.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter BJTtnom { get; } = new Parameter();
        [SpiceName("is"), SpiceInfo("Saturation Current")]
        public Parameter BJTsatCur { get; } = new Parameter(1e-16);
        [SpiceName("bf"), SpiceInfo("Ideal forward beta")]
        public Parameter BJTbetaF { get; } = new Parameter(100);
        [SpiceName("nf"), SpiceInfo("Forward emission coefficient")]
        public Parameter BJTemissionCoeffF { get; } = new Parameter(1);
        [SpiceName("vaf"), SpiceName("va"), SpiceInfo("Forward Early voltage")]
        public Parameter BJTearlyVoltF { get; } = new Parameter();
        [SpiceName("ikf"), SpiceName("ik"), SpiceInfo("Forward beta roll-off corner current")]
        public Parameter BJTrollOffF { get; } = new Parameter();
        [SpiceName("ise"), SpiceInfo("B-E leakage saturation current")]
        public Parameter BJTleakBEcurrent { get; } = new Parameter();
        [SpiceName("ne"), SpiceInfo("B-E leakage emission coefficient")]
        public Parameter BJTleakBEemissionCoeff { get; } = new Parameter(1.5);
        [SpiceName("br"), SpiceInfo("Ideal reverse beta")]
        public Parameter BJTbetaR { get; } = new Parameter(1);
        [SpiceName("nr"), SpiceInfo("Reverse emission coefficient")]
        public Parameter BJTemissionCoeffR { get; } = new Parameter(1);
        [SpiceName("var"), SpiceName("vb"), SpiceInfo("Reverse Early voltage")]
        public Parameter BJTearlyVoltR { get; } = new Parameter();
        [SpiceName("ikr"), SpiceInfo("reverse beta roll-off corner current")]
        public Parameter BJTrollOffR { get; } = new Parameter();
        [SpiceName("isc"), SpiceInfo("B-C leakage saturation current")]
        public Parameter BJTleakBCcurrent { get; } = new Parameter();
        [SpiceName("nc"), SpiceInfo("B-C leakage emission coefficient")]
        public Parameter BJTleakBCemissionCoeff { get; } = new Parameter(2);
        [SpiceName("rb"), SpiceInfo("Zero bias base resistance")]
        public Parameter BJTbaseResist { get; } = new Parameter();
        [SpiceName("irb"), SpiceInfo("Current for base resistance=(rb+rbm)/2")]
        public Parameter BJTbaseCurrentHalfResist { get; } = new Parameter();
        [SpiceName("rbm"), SpiceInfo("Minimum base resistance")]
        public Parameter BJTminBaseResist { get; } = new Parameter();
        [SpiceName("re"), SpiceInfo("Emitter resistance")]
        public Parameter BJTemitterResist { get; } = new Parameter();
        [SpiceName("rc"), SpiceInfo("Collector resistance")]
        public Parameter BJTcollectorResist { get; } = new Parameter();
        [SpiceName("cje"), SpiceInfo("Zero bias B-E depletion capacitance")]
        public Parameter BJTdepletionCapBE { get; } = new Parameter();
        [SpiceName("vje"), SpiceName("pe"), SpiceInfo("B-E built in potential")]
        public Parameter BJTpotentialBE { get; } = new Parameter(0.75);
        [SpiceName("mje"), SpiceName("me"), SpiceInfo("B-E junction grading coefficient")]
        public Parameter BJTjunctionExpBE { get; } = new Parameter(.33);
        [SpiceName("tf"), SpiceInfo("Ideal forward transit time")]
        public Parameter BJTtransitTimeF { get; } = new Parameter();
        [SpiceName("xtf"), SpiceInfo("Coefficient for bias dependence of TF")]
        public Parameter BJTtransitTimeBiasCoeffF { get; } = new Parameter();
        [SpiceName("vtf"), SpiceInfo("Voltage giving VBC dependence of TF")]
        public Parameter BJTtransitTimeFVBC { get; } = new Parameter();
        [SpiceName("itf"), SpiceInfo("High current dependence of TF")]
        public Parameter BJTtransitTimeHighCurrentF { get; } = new Parameter();
        [SpiceName("ptf"), SpiceInfo("Excess phase")]
        public Parameter BJTexcessPhase { get; } = new Parameter();
        [SpiceName("cjc"), SpiceInfo("Zero bias B-C depletion capacitance")]
        public Parameter BJTdepletionCapBC { get; } = new Parameter();
        [SpiceName("vjc"), SpiceName("pc"), SpiceInfo("B-C built in potential")]
        public Parameter BJTpotentialBC { get; } = new Parameter(0.75);
        [SpiceName("mjc"), SpiceName("mc"), SpiceInfo("B-C junction grading coefficient")]
        public Parameter BJTjunctionExpBC { get; } = new Parameter(.33);
        [SpiceName("xcjc"), SpiceInfo("Fraction of B-C cap to internal base")]
        public Parameter BJTbaseFractionBCcap { get; } = new Parameter(1);
        [SpiceName("tr"), SpiceInfo("Ideal reverse transit time")]
        public Parameter BJTtransitTimeR { get; } = new Parameter();
        [SpiceName("cjs"), SpiceName("ccs"), SpiceInfo("Zero bias C-S capacitance")]
        public Parameter BJTcapCS { get; } = new Parameter();
        [SpiceName("vjs"), SpiceName("ps"), SpiceInfo("Substrate junction built in potential")]
        public Parameter BJTpotentialSubstrate { get; } = new Parameter(.75);
        [SpiceName("mjs"), SpiceName("ms"), SpiceInfo("Substrate junction grading coefficient")]
        public Parameter BJTexponentialSubstrate { get; } = new Parameter();
        [SpiceName("xtb"), SpiceInfo("Forward and reverse beta temp. exp.")]
        public Parameter BJTbetaExp { get; } = new Parameter();
        [SpiceName("eg"), SpiceInfo("Energy gap for IS temp. dependency")]
        public Parameter BJTenergyGap { get; } = new Parameter(1.11);
        [SpiceName("xti"), SpiceInfo("Temp. exponent for IS")]
        public Parameter BJTtempExpIS { get; } = new Parameter(3);
        [SpiceName("fc"), SpiceInfo("Forward bias junction fit parameter")]
        public Parameter BJTdepletionCapCoeff { get; } = new Parameter();
        [SpiceName("invearlyvoltf"), SpiceInfo("Inverse early voltage:forward")]
        public double BJTinvEarlyVoltF { get; internal set; }
        [SpiceName("invearlyvoltr"), SpiceInfo("Inverse early voltage:reverse")]
        public double BJTinvEarlyVoltR { get; internal set; }
        [SpiceName("invrollofff"), SpiceInfo("Inverse roll off - forward")]
        public double BJTinvRollOffF { get; internal set; }
        [SpiceName("invrolloffr"), SpiceInfo("Inverse roll off - reverse")]
        public double BJTinvRollOffR { get; internal set; }
        [SpiceName("collectorconduct"), SpiceInfo("Collector conductance")]
        public double BJTcollectorConduct { get; internal set; }
        [SpiceName("emitterconduct"), SpiceInfo("Emitter conductance")]
        public double BJTemitterConduct { get; internal set; }
        [SpiceName("transtimevbcfact"), SpiceInfo("Transit time VBC factor")]
        public double BJTtransitTimeVBCFactor { get; internal set; }
        [SpiceName("excessphasefactor"), SpiceInfo("Excess phase fact.")]
        public double BJTexcessPhaseFactor { get; internal set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int NPN = 1;
        public const int PNP = -1;

        /// <summary>
        /// Shared parameters
        /// </summary>
        public double fact1 { get; protected set; }
        public double xfc { get; protected set; }

        /// <summary>
        /// Extra variables
        /// </summary>
        public Parameter BJTc2 { get; } = new Parameter();
        public Parameter BJTc4 { get; } = new Parameter();
        public double BJTf2 { get; protected set; }
        public double BJTf3 { get; protected set; }
        public double BJTf6 { get; protected set; }
        public double BJTf7 { get; protected set; }

        /// <summary>
        /// Private variables
        /// </summary>
        private Identifier name;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component"></param>
        /// <param name="ckt"></param>
        public override void Setup(Entity component, Circuit ckt)
        {
            // Store the name for error reporting
            name = component.Name;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="ckt">Circuit</param>
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
                    throw new CircuitException($"BJT model {name}, parameter fc limited to 0.9999");
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
        }
    }
}
