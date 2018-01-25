using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Bipolar
{
    /// <summary>
    /// Base parameters for a <see cref="BJTModel"/>
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [NameAttribute("npn"), InfoAttribute("NPN type device")]
        public void SetNPN(bool value)
        {
            if (value)
                BJTtype = NPN;
        }
        [NameAttribute("pnp"), InfoAttribute("PNP type device")]
        public void SetPNP(bool value)
        {
            if (value)
                BJTtype = PNP;
        }
        [NameAttribute("type"), InfoAttribute("NPN or PNP")]
        public string GetTYPE(Circuit ckt)
        {
            if (BJTtype == NPN)
                return "npn";
            else
                return "pnp";
        }
        public double BJTtype { get; internal set; } = NPN;
        [NameAttribute("tnom"), InfoAttribute("Parameter measurement temperature")]
        public double BJT_TNOM
        {
            get => BJTtnom - Circuit.CONSTCtoK;
            set => BJTtnom.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter BJTtnom { get; } = new Parameter();
        [NameAttribute("is"), InfoAttribute("Saturation Current")]
        public Parameter BJTsatCur { get; } = new Parameter(1e-16);
        [NameAttribute("bf"), InfoAttribute("Ideal forward beta")]
        public Parameter BJTbetaF { get; } = new Parameter(100);
        [NameAttribute("nf"), InfoAttribute("Forward emission coefficient")]
        public Parameter BJTemissionCoeffF { get; } = new Parameter(1);
        [NameAttribute("vaf"), NameAttribute("va"), InfoAttribute("Forward Early voltage")]
        public Parameter BJTearlyVoltF { get; } = new Parameter();
        [NameAttribute("ikf"), NameAttribute("ik"), InfoAttribute("Forward beta roll-off corner current")]
        public Parameter BJTrollOffF { get; } = new Parameter();
        [NameAttribute("ise"), InfoAttribute("B-E leakage saturation current")]
        public Parameter BJTleakBEcurrent { get; } = new Parameter();
        [NameAttribute("ne"), InfoAttribute("B-E leakage emission coefficient")]
        public Parameter BJTleakBEemissionCoeff { get; } = new Parameter(1.5);
        [NameAttribute("br"), InfoAttribute("Ideal reverse beta")]
        public Parameter BJTbetaR { get; } = new Parameter(1);
        [NameAttribute("nr"), InfoAttribute("Reverse emission coefficient")]
        public Parameter BJTemissionCoeffR { get; } = new Parameter(1);
        [NameAttribute("var"), NameAttribute("vb"), InfoAttribute("Reverse Early voltage")]
        public Parameter BJTearlyVoltR { get; } = new Parameter();
        [NameAttribute("ikr"), InfoAttribute("reverse beta roll-off corner current")]
        public Parameter BJTrollOffR { get; } = new Parameter();
        [NameAttribute("isc"), InfoAttribute("B-C leakage saturation current")]
        public Parameter BJTleakBCcurrent { get; } = new Parameter();
        [NameAttribute("nc"), InfoAttribute("B-C leakage emission coefficient")]
        public Parameter BJTleakBCemissionCoeff { get; } = new Parameter(2);
        [NameAttribute("rb"), InfoAttribute("Zero bias base resistance")]
        public Parameter BJTbaseResist { get; } = new Parameter();
        [NameAttribute("irb"), InfoAttribute("Current for base resistance=(rb+rbm)/2")]
        public Parameter BJTbaseCurrentHalfResist { get; } = new Parameter();
        [NameAttribute("rbm"), InfoAttribute("Minimum base resistance")]
        public Parameter BJTminBaseResist { get; } = new Parameter();
        [NameAttribute("re"), InfoAttribute("Emitter resistance")]
        public Parameter BJTemitterResist { get; } = new Parameter();
        [NameAttribute("rc"), InfoAttribute("Collector resistance")]
        public Parameter BJTcollectorResist { get; } = new Parameter();
        [NameAttribute("cje"), InfoAttribute("Zero bias B-E depletion capacitance")]
        public Parameter BJTdepletionCapBE { get; } = new Parameter();
        [NameAttribute("vje"), NameAttribute("pe"), InfoAttribute("B-E built in potential")]
        public Parameter BJTpotentialBE { get; } = new Parameter(0.75);
        [NameAttribute("mje"), NameAttribute("me"), InfoAttribute("B-E junction grading coefficient")]
        public Parameter BJTjunctionExpBE { get; } = new Parameter(.33);
        [NameAttribute("tf"), InfoAttribute("Ideal forward transit time")]
        public Parameter BJTtransitTimeF { get; } = new Parameter();
        [NameAttribute("xtf"), InfoAttribute("Coefficient for bias dependence of TF")]
        public Parameter BJTtransitTimeBiasCoeffF { get; } = new Parameter();
        [NameAttribute("vtf"), InfoAttribute("Voltage giving VBC dependence of TF")]
        public Parameter BJTtransitTimeFVBC { get; } = new Parameter();
        [NameAttribute("itf"), InfoAttribute("High current dependence of TF")]
        public Parameter BJTtransitTimeHighCurrentF { get; } = new Parameter();
        [NameAttribute("ptf"), InfoAttribute("Excess phase")]
        public Parameter BJTexcessPhase { get; } = new Parameter();
        [NameAttribute("cjc"), InfoAttribute("Zero bias B-C depletion capacitance")]
        public Parameter BJTdepletionCapBC { get; } = new Parameter();
        [NameAttribute("vjc"), NameAttribute("pc"), InfoAttribute("B-C built in potential")]
        public Parameter BJTpotentialBC { get; } = new Parameter(0.75);
        [NameAttribute("mjc"), NameAttribute("mc"), InfoAttribute("B-C junction grading coefficient")]
        public Parameter BJTjunctionExpBC { get; } = new Parameter(.33);
        [NameAttribute("xcjc"), InfoAttribute("Fraction of B-C cap to internal base")]
        public Parameter BJTbaseFractionBCcap { get; } = new Parameter(1);
        [NameAttribute("tr"), InfoAttribute("Ideal reverse transit time")]
        public Parameter BJTtransitTimeR { get; } = new Parameter();
        [NameAttribute("cjs"), NameAttribute("ccs"), InfoAttribute("Zero bias C-S capacitance")]
        public Parameter BJTcapCS { get; } = new Parameter();
        [NameAttribute("vjs"), NameAttribute("ps"), InfoAttribute("Substrate junction built in potential")]
        public Parameter BJTpotentialSubstrate { get; } = new Parameter(.75);
        [NameAttribute("mjs"), NameAttribute("ms"), InfoAttribute("Substrate junction grading coefficient")]
        public Parameter BJTexponentialSubstrate { get; } = new Parameter();
        [NameAttribute("xtb"), InfoAttribute("Forward and reverse beta temp. exp.")]
        public Parameter BJTbetaExp { get; } = new Parameter();
        [NameAttribute("eg"), InfoAttribute("Energy gap for IS temp. dependency")]
        public Parameter BJTenergyGap { get; } = new Parameter(1.11);
        [NameAttribute("xti"), InfoAttribute("Temp. exponent for IS")]
        public Parameter BJTtempExpIS { get; } = new Parameter(3);
        [NameAttribute("fc"), InfoAttribute("Forward bias junction fit parameter")]
        public Parameter BJTdepletionCapCoeff { get; } = new Parameter();

        public Parameter BJTc2 { get; } = new Parameter();
        public Parameter BJTc4 { get; } = new Parameter();

        /// <summary>
        /// Constants
        /// </summary>
        public const int NPN = 1;
        public const int PNP = -1;
    }
}
