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
    /// This class represents a model for bipolar transistors
    /// </summary>
    public class BipolarModel : CircuitModel
    {

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("type"), SpiceInfo("NPN or PNP")]
        public int BJTtype { get; private set; } = NPN;
        [SpiceName("npn"), SpiceInfo("NPN type device")]
        public void BJTSetNPN(Circuit ckt) { BJTtype = NPN; }
        [SpiceName("pnp"), SpiceInfo("PNP type device")]
        public void BJTSetPNP(Circuit ckt) { BJTtype = PNP; }
        [SpiceName("is"), SpiceInfo("Saturation Current")]
        public Parameter<double> BJTsatCur { get; } = new Parameter<double>(1e-16);
        [SpiceName("bf"), SpiceInfo("Ideal forward beta")]
        public Parameter<double> BJTbetaF { get; } = new Parameter<double>(100);
        [SpiceName("nf"), SpiceInfo("Forward emission coefficient")]
        public Parameter<double> BJTemissionCoeffF { get; } = new Parameter<double>(1);
        [SpiceName("ne"), SpiceInfo("B-E leakage emission coefficient")]
        public Parameter<double> BJTleakBEemissionCoeff { get; } = new Parameter<double>(1.5);
        [SpiceName("br"), SpiceInfo("Ideal reverse beta")]
        public Parameter<double> BJTbetaR { get; } = new Parameter<double>(1);
        [SpiceName("nr"), SpiceInfo("Reverse emission coefficient")]
        public Parameter<double> BJTemissionCoeffR { get; } = new Parameter<double>(1);
        [SpiceName("nc"), SpiceInfo("B-C leakage emission coefficient")]
        public Parameter<double> BJTleakBCemissionCoeff { get; } = new Parameter<double>(2);
        [SpiceName("rb"), SpiceInfo("Zero bias base resistance")]
        public Parameter<double> BJTbaseResist { get; } = new Parameter<double>(0);
        [SpiceName("re"), SpiceInfo("Emitter resistance")]
        public Parameter<double> BJTemitterResist { get; } = new Parameter<double>(0);
        [SpiceName("rc"), SpiceInfo("Collector resistance")]
        public Parameter<double> BJTcollectorResist { get; } = new Parameter<double>(0);
        [SpiceName("cje"), SpiceInfo("Zero bias B-E depletion capacitance")]
        public Parameter<double> BJTdepletionCapBE { get; } = new Parameter<double>(0);
        [SpiceName("vje"), SpiceName("pe"), SpiceInfo("B-E built in potential")]
        public Parameter<double> BJTpotentialBE { get; } = new Parameter<double>(.75);
        [SpiceName("mje"), SpiceName("me"), SpiceInfo("B-E junction grading coefficient")]
        public Parameter<double> BJTjunctionExpBE { get; } = new Parameter<double>(.33);
        [SpiceName("tf"), SpiceInfo("Ideal forward transit time")]
        public Parameter<double> BJTtransitTimeF { get; } = new Parameter<double>(0);
        [SpiceName("xtf"), SpiceInfo("Coefficient for bias dependence of TF")]
        public Parameter<double> BJTtransitTimeBiasCoeffF { get; } = new Parameter<double>(0);
        [SpiceName("itf"), SpiceInfo("High current dependence of TF")]
        public Parameter<double> BJTtransitTimeHighCurrentF { get; } = new Parameter<double>(0);
        [SpiceName("ptf"), SpiceInfo("Excess phase")]
        public Parameter<double> BJTexcessPhase { get; } = new Parameter<double>(0);
        [SpiceName("cjc"), SpiceInfo("Zero bias B-C depletion capacitance")]
        public Parameter<double> BJTdepletionCapBC { get; } = new Parameter<double>(0);
        [SpiceName("vjc"), SpiceName("pc"), SpiceInfo("B-C built in potential")]
        public Parameter<double> BJTpotentialBC { get; } = new Parameter<double>(.75);
        [SpiceName("mjc"), SpiceName("mc"), SpiceInfo("B-C junction grading coefficient")]
        public Parameter<double> BJTjunctionExpBC { get; } = new Parameter<double>(.33);
        [SpiceName("xcjc"), SpiceInfo("Fraction of B-C cap to internal base")]
        public Parameter<double> BJTbaseFractionBCcap { get; } = new Parameter<double>(1);
        [SpiceName("tr"), SpiceInfo("Ideal reverse transit time")]
        public Parameter<double> BJTtransitTimeR { get; } = new Parameter<double>(0);
        [SpiceName("cjs"), SpiceName("ccs"), SpiceInfo("Zero bias C-S capacitance")]
        public Parameter<double> BJTcapCS { get; } = new Parameter<double>(0);
        [SpiceName("vjs"), SpiceName("ps"), SpiceInfo("Substrate junction built in potential")]
        public Parameter<double> BJTpotentialSubstrate { get; } = new Parameter<double>(.75);
        [SpiceName("mjs"), SpiceName("ms"), SpiceInfo("Substrate junction grading coefficient")]
        public Parameter<double> BJTexponentialSubstrate { get; } = new Parameter<double>(0);
        [SpiceName("xtb"), SpiceInfo("Forward and reverse beta temp. exp.")]
        public Parameter<double> BJTbetaExp { get; } = new Parameter<double>(0);
        [SpiceName("eg"), SpiceInfo("Energy gap for IS temp. dependency")]
        public Parameter<double> BJTenergyGap { get; } = new Parameter<double>(1.11);
        [SpiceName("xti"), SpiceInfo("Temp. exponent for IS")]
        public Parameter<double> BJTtempExpIS { get; } = new Parameter<double>(3);
        [SpiceName("kf"), SpiceInfo("Flicker Noise Coefficient")]
        public Parameter<double> BJTfNcoef { get; } = new Parameter<double>(0);
        [SpiceName("af"), SpiceInfo("Flicker Noise Exponent")]
        public Parameter<double> BJTfNexp { get; } = new Parameter<double>(1);
        [SpiceName("ikr"), SpiceInfo("Reverse beta roll-off corner current")]
        public Parameter<double> BJTrollOffR { get; } = new Parameter<double>();
        [SpiceName("var"), SpiceName("vb"), SpiceInfo("Reverse Early Voltage")]
        public Parameter<double> BJTearlyVoltR { get; } = new Parameter<double>();
        [SpiceName("vtf"), SpiceInfo("Voltage giving VBC dependence of TF")]
        public Parameter<double> BJTtransitTimeFVBC { get; } = new Parameter<double>();
        [SpiceName("isc"), SpiceInfo("B-C leakage saturation current")]
        public Parameter<double> BJTleakBCcurrent { get; } = new Parameter<double>();
        [SpiceName("tnom"), SpiceInfo("Parameter measurement temperature in Kelvin")]
        public Parameter<double> BJTtnom { get; } = new Parameter<double>();
        [SpiceName("ikf"), SpiceName("ik"), SpiceInfo("Forward beta roll-off corner current")]
        public Parameter<double> BJTrollOffF { get; } = new Parameter<double>();
        [SpiceName("vaf"), SpiceName("va"), SpiceInfo("Forward Early voltage")]
        public Parameter<double> BJTearlyVoltF { get; } = new Parameter<double>();
        [SpiceName("rbm"), SpiceInfo("Minimum base resistance")]
        public Parameter<double> BJTminBaseResist { get; } = new Parameter<double>();
        [SpiceName("fc"), SpiceInfo("Forward bias junction fit parameter")]
        public Parameter<double> BJTdepletionCapCoeff { get; } = new Parameter<double>();
        [SpiceName("ise"), SpiceInfo("B-E leakage saturation current")]
        public Parameter<double> BJTleakBEcurrent { get; } = new Parameter<double>();
        [SpiceName("irb"), SpiceInfo("Current for base resistance=(rb+rbm)/2")]
        public double BJTbaseCurrentHalfResist { get; private set; } = 0.0;

        [SpiceName("c2"), SpiceInfo("B-E leakage current factor")]
        public Parameter<double> BJTc2 { get; } = new Parameter<double>();
        [SpiceName("c4"), SpiceInfo("B-C leakage current factor")]
        public Parameter<double> BJTc4 { get; } = new Parameter<double>();

        [SpiceName("invearlyvoltf"), SpiceInfo("Inverse early voltage: forward")]
        public double GetInvEarlyF(Circuit ckt) => BJTinvEarlyVoltF;
        [SpiceName("invearlyvoltr"), SpiceInfo("Inverse early voltage: reverse")]
        public double GetInvEarlyR(Circuit ckt) => BJTinvEarlyVoltR;
        [SpiceName("invrollofff"), SpiceInfo("Inverse roll off: forward")]
        public double GetInvRollOffF(Circuit ckt) => BJTinvRollOffF;
        [SpiceName("invrollr"), SpiceInfo("Inverse roll off: reverse")]
        public double GetInvRollOffR(Circuit ckt) => BJTinvRollOffR;
        [SpiceName("collectorconduct"), SpiceInfo("Collector conductance")]
        public double GetColConduct(Circuit ckt) => BJTcollectorConduct;
        [SpiceName("EmitterConduct"), SpiceInfo("Emitter conductance")]
        public double GetEmitterConduct(Circuit ckt) => BJTemitterConduct;
        [SpiceName("transtimevbcfact"), SpiceInfo("Transit time VBC factor")]
        public double GetTransVBCFact(Circuit ckt) => BJTtransitTimeVBCFactor;
        [SpiceName("excessphasefactor"), SpiceInfo("Excess phase factor")]
        public double GetExcessPhaseFactor(Circuit ckt) => BJTexcessPhaseFactor;

        /// <summary>
        /// Internally calculated constants
        /// </summary>
        public double BJTinvEarlyVoltF { get; private set; }    /* inverse of BJTearlyVoltF */
        public double BJTinvEarlyVoltR { get; private set; }    /* inverse of BJTearlyVoltR */
        public double BJTinvRollOffF { get; private set; }  /* inverse of BJTrollOffF */
        public double BJTinvRollOffR { get; private set; }  /* inverse of BJTrollOffR */
        public double BJTcollectorConduct { get; private set; } /* collector conductance */
        public double BJTemitterConduct { get; private set; }   /* emitter conductance */
        public double BJTtransitTimeVBCFactor { get; private set; } /* */
        public double BJTexcessPhaseFactor { get; private set; }
        public double BJTf2 { get; private set; }
        public double BJTf3 { get; private set; }
        public double BJTf6 { get; private set; }
        public double BJTf7 { get; private set; }

        public double xfc { get; private set; }
        public double fact1 { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        private const int NPN = 1;
        private const int PNP = -1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the model</param>
        public BipolarModel(string name) : base(name) { }

        /// <summary>
        /// Do temperature-dependent calculations
        /// This method is taken from BJTtemp.c in Spice 3.5f
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            var state = ckt.State;

            if (!BJTtnom.Given)
                BJTtnom.Value = state.NominalTemperature;
            fact1 = BJTtnom / Circuit.CONSTRefTemp;

            if (!BJTleakBEcurrent.Given)
            {
                if (BJTc2.Given)
                {
                    BJTleakBEcurrent.Value = BJTc2 * BJTsatCur;
                }
                else
                {
                    BJTleakBEcurrent.Value = 0;
                }
            }
            if (!BJTleakBCcurrent.Given)
            {
                if (BJTc4.Given)
                {
                    BJTleakBCcurrent.Value = BJTc4 * BJTsatCur;
                }
                else
                {
                    BJTleakBCcurrent.Value = 0;
                }
            }
            if (!BJTminBaseResist.Given)
            {
                BJTminBaseResist.Value = BJTbaseResist;
            }

            /*
             * COMPATABILITY WARNING!
             * special note:  for backward compatability to much older models, spice 2G
             * implemented a special case which checked if B-E leakage saturation
             * current was >1, then it was instead a the B-E leakage saturation current
             * divided by IS, and multiplied it by IS at this point.  This was not
             * handled correctly in the 2G code, and there is some question on its 
             * reasonability, since it is also undocumented, so it has been left out
             * here.  It could easily be added with 1 line.  (The same applies to the B-C
             * leakage saturation current).   TQ  6/29/84
             */

            if (BJTearlyVoltF.Given && BJTearlyVoltF != 0)
            {
                BJTinvEarlyVoltF = 1 / BJTearlyVoltF;
            }
            else
            {
                BJTinvEarlyVoltF = 0;
            }
            if (BJTrollOffF.Given && BJTrollOffF != 0)
            {
                BJTinvRollOffF = 1 / BJTrollOffF;
            }
            else
            {
                BJTinvRollOffF = 0;
            }
            if (BJTearlyVoltR.Given && BJTearlyVoltR != 0)
            {
                BJTinvEarlyVoltR = 1 / BJTearlyVoltR;
            }
            else
            {
                BJTinvEarlyVoltR = 0;
            }
            if (BJTrollOffR.Given && BJTrollOffR != 0)
            {
                BJTinvRollOffR = 1 / BJTrollOffR;
            }
            else
            {
                BJTinvRollOffR = 0;
            }
            if (BJTcollectorResist.Given && BJTcollectorResist != 0)
            {
                BJTcollectorConduct = 1 / BJTcollectorResist;
            }
            else
            {
                BJTcollectorConduct = 0;
            }
            if (BJTemitterResist.Given && BJTemitterResist != 0)
            {
                BJTemitterConduct = 1 / BJTemitterResist;
            }
            else
            {
                BJTemitterConduct = 0;
            }
            if (BJTtransitTimeFVBC.Given && BJTtransitTimeFVBC != 0)
            {
                BJTtransitTimeVBCFactor = 1 / (BJTtransitTimeFVBC * 1.44);
            }
            else
            {
                BJTtransitTimeVBCFactor = 0;
            }
            BJTexcessPhaseFactor = (BJTexcessPhase / (180.0 / Circuit.CONSTPI)) * BJTtransitTimeF;
            if (BJTdepletionCapCoeff.Given)
            {
                if (BJTdepletionCapCoeff > .9999)
                {
                    BJTdepletionCapCoeff.Value = .9999;
                    CircuitWarning.Warning(this, string.Format("BJT model {0}, parameter fc limited to 0.9999", Name));
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
