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
        [PropertyName("npn"), PropertyInfo("NPN type device")]
        public void SetNPN(bool value)
        {
            if (value)
                BJTtype = NPN;
        }
        [PropertyName("pnp"), PropertyInfo("PNP type device")]
        public void SetPNP(bool value)
        {
            if (value)
                BJTtype = PNP;
        }
        [PropertyName("type"), PropertyInfo("NPN or PNP")]
        public string GetTYPE(Circuit ckt)
        {
            if (BJTtype == NPN)
                return "npn";
            else
                return "pnp";
        }
        public double BJTtype { get; internal set; } = NPN;
        [PropertyName("tnom"), PropertyInfo("Parameter measurement temperature")]
        public double BJT_TNOM
        {
            get => BJTtnom - Circuit.CelsiusKelvin;
            set => BJTtnom.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter BJTtnom { get; } = new Parameter();
        [PropertyName("is"), PropertyInfo("Saturation Current")]
        public Parameter BJTsatCur { get; } = new Parameter(1e-16);
        [PropertyName("bf"), PropertyInfo("Ideal forward beta")]
        public Parameter BJTbetaF { get; } = new Parameter(100);
        [PropertyName("nf"), PropertyInfo("Forward emission coefficient")]
        public Parameter BJTemissionCoeffF { get; } = new Parameter(1);
        [PropertyName("vaf"), PropertyName("va"), PropertyInfo("Forward Early voltage")]
        public Parameter BJTearlyVoltF { get; } = new Parameter();
        [PropertyName("ikf"), PropertyName("ik"), PropertyInfo("Forward beta roll-off corner current")]
        public Parameter BJTrollOffF { get; } = new Parameter();
        [PropertyName("ise"), PropertyInfo("B-E leakage saturation current")]
        public Parameter BJTleakBEcurrent { get; } = new Parameter();
        [PropertyName("ne"), PropertyInfo("B-E leakage emission coefficient")]
        public Parameter BJTleakBEemissionCoeff { get; } = new Parameter(1.5);
        [PropertyName("br"), PropertyInfo("Ideal reverse beta")]
        public Parameter BJTbetaR { get; } = new Parameter(1);
        [PropertyName("nr"), PropertyInfo("Reverse emission coefficient")]
        public Parameter BJTemissionCoeffR { get; } = new Parameter(1);
        [PropertyName("var"), PropertyName("vb"), PropertyInfo("Reverse Early voltage")]
        public Parameter BJTearlyVoltR { get; } = new Parameter();
        [PropertyName("ikr"), PropertyInfo("reverse beta roll-off corner current")]
        public Parameter BJTrollOffR { get; } = new Parameter();
        [PropertyName("isc"), PropertyInfo("B-C leakage saturation current")]
        public Parameter BJTleakBCcurrent { get; } = new Parameter();
        [PropertyName("nc"), PropertyInfo("B-C leakage emission coefficient")]
        public Parameter BJTleakBCemissionCoeff { get; } = new Parameter(2);
        [PropertyName("rb"), PropertyInfo("Zero bias base resistance")]
        public Parameter BJTbaseResist { get; } = new Parameter();
        [PropertyName("irb"), PropertyInfo("Current for base resistance=(rb+rbm)/2")]
        public Parameter BJTbaseCurrentHalfResist { get; } = new Parameter();
        [PropertyName("rbm"), PropertyInfo("Minimum base resistance")]
        public Parameter BJTminBaseResist { get; } = new Parameter();
        [PropertyName("re"), PropertyInfo("Emitter resistance")]
        public Parameter BJTemitterResist { get; } = new Parameter();
        [PropertyName("rc"), PropertyInfo("Collector resistance")]
        public Parameter BJTcollectorResist { get; } = new Parameter();
        [PropertyName("cje"), PropertyInfo("Zero bias B-E depletion capacitance")]
        public Parameter BJTdepletionCapBE { get; } = new Parameter();
        [PropertyName("vje"), PropertyName("pe"), PropertyInfo("B-E built in potential")]
        public Parameter BJTpotentialBE { get; } = new Parameter(0.75);
        [PropertyName("mje"), PropertyName("me"), PropertyInfo("B-E junction grading coefficient")]
        public Parameter BJTjunctionExpBE { get; } = new Parameter(.33);
        [PropertyName("tf"), PropertyInfo("Ideal forward transit time")]
        public Parameter BJTtransitTimeF { get; } = new Parameter();
        [PropertyName("xtf"), PropertyInfo("Coefficient for bias dependence of TF")]
        public Parameter BJTtransitTimeBiasCoeffF { get; } = new Parameter();
        [PropertyName("vtf"), PropertyInfo("Voltage giving VBC dependence of TF")]
        public Parameter BJTtransitTimeFVBC { get; } = new Parameter();
        [PropertyName("itf"), PropertyInfo("High current dependence of TF")]
        public Parameter BJTtransitTimeHighCurrentF { get; } = new Parameter();
        [PropertyName("ptf"), PropertyInfo("Excess phase")]
        public Parameter BJTexcessPhase { get; } = new Parameter();
        [PropertyName("cjc"), PropertyInfo("Zero bias B-C depletion capacitance")]
        public Parameter BJTdepletionCapBC { get; } = new Parameter();
        [PropertyName("vjc"), PropertyName("pc"), PropertyInfo("B-C built in potential")]
        public Parameter BJTpotentialBC { get; } = new Parameter(0.75);
        [PropertyName("mjc"), PropertyName("mc"), PropertyInfo("B-C junction grading coefficient")]
        public Parameter BJTjunctionExpBC { get; } = new Parameter(.33);
        [PropertyName("xcjc"), PropertyInfo("Fraction of B-C cap to internal base")]
        public Parameter BJTbaseFractionBCcap { get; } = new Parameter(1);
        [PropertyName("tr"), PropertyInfo("Ideal reverse transit time")]
        public Parameter BJTtransitTimeR { get; } = new Parameter();
        [PropertyName("cjs"), PropertyName("ccs"), PropertyInfo("Zero bias C-S capacitance")]
        public Parameter BJTcapCS { get; } = new Parameter();
        [PropertyName("vjs"), PropertyName("ps"), PropertyInfo("Substrate junction built in potential")]
        public Parameter BJTpotentialSubstrate { get; } = new Parameter(.75);
        [PropertyName("mjs"), PropertyName("ms"), PropertyInfo("Substrate junction grading coefficient")]
        public Parameter BJTexponentialSubstrate { get; } = new Parameter();
        [PropertyName("xtb"), PropertyInfo("Forward and reverse beta temp. exp.")]
        public Parameter BJTbetaExp { get; } = new Parameter();
        [PropertyName("eg"), PropertyInfo("Energy gap for IS temp. dependency")]
        public Parameter BJTenergyGap { get; } = new Parameter(1.11);
        [PropertyName("xti"), PropertyInfo("Temp. exponent for IS")]
        public Parameter BJTtempExpIS { get; } = new Parameter(3);
        [PropertyName("fc"), PropertyInfo("Forward bias junction fit parameter")]
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
