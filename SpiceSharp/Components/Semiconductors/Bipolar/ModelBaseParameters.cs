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
        [PropertyNameAttribute("npn"), PropertyInfoAttribute("NPN type device")]
        public void SetNPN(bool value)
        {
            if (value)
                BJTtype = NPN;
        }
        [PropertyNameAttribute("pnp"), PropertyInfoAttribute("PNP type device")]
        public void SetPNP(bool value)
        {
            if (value)
                BJTtype = PNP;
        }
        [PropertyNameAttribute("type"), PropertyInfoAttribute("NPN or PNP")]
        public string GetTYPE(Circuit ckt)
        {
            if (BJTtype == NPN)
                return "npn";
            else
                return "pnp";
        }
        public double BJTtype { get; internal set; } = NPN;
        [PropertyNameAttribute("tnom"), PropertyInfoAttribute("Parameter measurement temperature")]
        public double BJT_TNOM
        {
            get => BJTtnom - Circuit.CONSTCtoK;
            set => BJTtnom.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter BJTtnom { get; } = new Parameter();
        [PropertyNameAttribute("is"), PropertyInfoAttribute("Saturation Current")]
        public Parameter BJTsatCur { get; } = new Parameter(1e-16);
        [PropertyNameAttribute("bf"), PropertyInfoAttribute("Ideal forward beta")]
        public Parameter BJTbetaF { get; } = new Parameter(100);
        [PropertyNameAttribute("nf"), PropertyInfoAttribute("Forward emission coefficient")]
        public Parameter BJTemissionCoeffF { get; } = new Parameter(1);
        [PropertyNameAttribute("vaf"), PropertyNameAttribute("va"), PropertyInfoAttribute("Forward Early voltage")]
        public Parameter BJTearlyVoltF { get; } = new Parameter();
        [PropertyNameAttribute("ikf"), PropertyNameAttribute("ik"), PropertyInfoAttribute("Forward beta roll-off corner current")]
        public Parameter BJTrollOffF { get; } = new Parameter();
        [PropertyNameAttribute("ise"), PropertyInfoAttribute("B-E leakage saturation current")]
        public Parameter BJTleakBEcurrent { get; } = new Parameter();
        [PropertyNameAttribute("ne"), PropertyInfoAttribute("B-E leakage emission coefficient")]
        public Parameter BJTleakBEemissionCoeff { get; } = new Parameter(1.5);
        [PropertyNameAttribute("br"), PropertyInfoAttribute("Ideal reverse beta")]
        public Parameter BJTbetaR { get; } = new Parameter(1);
        [PropertyNameAttribute("nr"), PropertyInfoAttribute("Reverse emission coefficient")]
        public Parameter BJTemissionCoeffR { get; } = new Parameter(1);
        [PropertyNameAttribute("var"), PropertyNameAttribute("vb"), PropertyInfoAttribute("Reverse Early voltage")]
        public Parameter BJTearlyVoltR { get; } = new Parameter();
        [PropertyNameAttribute("ikr"), PropertyInfoAttribute("reverse beta roll-off corner current")]
        public Parameter BJTrollOffR { get; } = new Parameter();
        [PropertyNameAttribute("isc"), PropertyInfoAttribute("B-C leakage saturation current")]
        public Parameter BJTleakBCcurrent { get; } = new Parameter();
        [PropertyNameAttribute("nc"), PropertyInfoAttribute("B-C leakage emission coefficient")]
        public Parameter BJTleakBCemissionCoeff { get; } = new Parameter(2);
        [PropertyNameAttribute("rb"), PropertyInfoAttribute("Zero bias base resistance")]
        public Parameter BJTbaseResist { get; } = new Parameter();
        [PropertyNameAttribute("irb"), PropertyInfoAttribute("Current for base resistance=(rb+rbm)/2")]
        public Parameter BJTbaseCurrentHalfResist { get; } = new Parameter();
        [PropertyNameAttribute("rbm"), PropertyInfoAttribute("Minimum base resistance")]
        public Parameter BJTminBaseResist { get; } = new Parameter();
        [PropertyNameAttribute("re"), PropertyInfoAttribute("Emitter resistance")]
        public Parameter BJTemitterResist { get; } = new Parameter();
        [PropertyNameAttribute("rc"), PropertyInfoAttribute("Collector resistance")]
        public Parameter BJTcollectorResist { get; } = new Parameter();
        [PropertyNameAttribute("cje"), PropertyInfoAttribute("Zero bias B-E depletion capacitance")]
        public Parameter BJTdepletionCapBE { get; } = new Parameter();
        [PropertyNameAttribute("vje"), PropertyNameAttribute("pe"), PropertyInfoAttribute("B-E built in potential")]
        public Parameter BJTpotentialBE { get; } = new Parameter(0.75);
        [PropertyNameAttribute("mje"), PropertyNameAttribute("me"), PropertyInfoAttribute("B-E junction grading coefficient")]
        public Parameter BJTjunctionExpBE { get; } = new Parameter(.33);
        [PropertyNameAttribute("tf"), PropertyInfoAttribute("Ideal forward transit time")]
        public Parameter BJTtransitTimeF { get; } = new Parameter();
        [PropertyNameAttribute("xtf"), PropertyInfoAttribute("Coefficient for bias dependence of TF")]
        public Parameter BJTtransitTimeBiasCoeffF { get; } = new Parameter();
        [PropertyNameAttribute("vtf"), PropertyInfoAttribute("Voltage giving VBC dependence of TF")]
        public Parameter BJTtransitTimeFVBC { get; } = new Parameter();
        [PropertyNameAttribute("itf"), PropertyInfoAttribute("High current dependence of TF")]
        public Parameter BJTtransitTimeHighCurrentF { get; } = new Parameter();
        [PropertyNameAttribute("ptf"), PropertyInfoAttribute("Excess phase")]
        public Parameter BJTexcessPhase { get; } = new Parameter();
        [PropertyNameAttribute("cjc"), PropertyInfoAttribute("Zero bias B-C depletion capacitance")]
        public Parameter BJTdepletionCapBC { get; } = new Parameter();
        [PropertyNameAttribute("vjc"), PropertyNameAttribute("pc"), PropertyInfoAttribute("B-C built in potential")]
        public Parameter BJTpotentialBC { get; } = new Parameter(0.75);
        [PropertyNameAttribute("mjc"), PropertyNameAttribute("mc"), PropertyInfoAttribute("B-C junction grading coefficient")]
        public Parameter BJTjunctionExpBC { get; } = new Parameter(.33);
        [PropertyNameAttribute("xcjc"), PropertyInfoAttribute("Fraction of B-C cap to internal base")]
        public Parameter BJTbaseFractionBCcap { get; } = new Parameter(1);
        [PropertyNameAttribute("tr"), PropertyInfoAttribute("Ideal reverse transit time")]
        public Parameter BJTtransitTimeR { get; } = new Parameter();
        [PropertyNameAttribute("cjs"), PropertyNameAttribute("ccs"), PropertyInfoAttribute("Zero bias C-S capacitance")]
        public Parameter BJTcapCS { get; } = new Parameter();
        [PropertyNameAttribute("vjs"), PropertyNameAttribute("ps"), PropertyInfoAttribute("Substrate junction built in potential")]
        public Parameter BJTpotentialSubstrate { get; } = new Parameter(.75);
        [PropertyNameAttribute("mjs"), PropertyNameAttribute("ms"), PropertyInfoAttribute("Substrate junction grading coefficient")]
        public Parameter BJTexponentialSubstrate { get; } = new Parameter();
        [PropertyNameAttribute("xtb"), PropertyInfoAttribute("Forward and reverse beta temp. exp.")]
        public Parameter BJTbetaExp { get; } = new Parameter();
        [PropertyNameAttribute("eg"), PropertyInfoAttribute("Energy gap for IS temp. dependency")]
        public Parameter BJTenergyGap { get; } = new Parameter(1.11);
        [PropertyNameAttribute("xti"), PropertyInfoAttribute("Temp. exponent for IS")]
        public Parameter BJTtempExpIS { get; } = new Parameter(3);
        [PropertyNameAttribute("fc"), PropertyInfoAttribute("Forward bias junction fit parameter")]
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
