using SpiceSharp.Attributes;

namespace SpiceSharp.Components.BipolarBehaviors
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
                Type = NPN;
        }
        [PropertyName("pnp"), PropertyInfo("PNP type device")]
        public void SetPNP(bool value)
        {
            if (value)
                Type = PNP;
        }
        [PropertyName("type"), PropertyInfo("NPN or PNP")]
        public string GetTYPE(Circuit circuit)
        {
            if (Type == NPN)
                return "npn";
            else
                return "pnp";
        }
        public double Type { get; internal set; } = NPN;
        [PropertyName("tnom"), PropertyInfo("Parameter measurement temperature")]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Circuit.CelsiusKelvin;
            set => NominalTemperature.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter NominalTemperature { get; } = new Parameter();
        [PropertyName("is"), PropertyInfo("Saturation Current")]
        public Parameter SatCur { get; } = new Parameter(1e-16);
        [PropertyName("bf"), PropertyInfo("Ideal forward beta")]
        public Parameter BetaF { get; } = new Parameter(100);
        [PropertyName("nf"), PropertyInfo("Forward emission coefficient")]
        public Parameter EmissionCoeffF { get; } = new Parameter(1);
        [PropertyName("vaf"), PropertyName("va"), PropertyInfo("Forward Early voltage")]
        public Parameter EarlyVoltF { get; } = new Parameter();
        [PropertyName("ikf"), PropertyName("ik"), PropertyInfo("Forward beta roll-off corner current")]
        public Parameter RollOffF { get; } = new Parameter();
        [PropertyName("ise"), PropertyInfo("B-E leakage saturation current")]
        public Parameter LeakBEcurrent { get; } = new Parameter();
        [PropertyName("ne"), PropertyInfo("B-E leakage emission coefficient")]
        public Parameter LeakBEemissionCoeff { get; } = new Parameter(1.5);
        [PropertyName("br"), PropertyInfo("Ideal reverse beta")]
        public Parameter BetaR { get; } = new Parameter(1);
        [PropertyName("nr"), PropertyInfo("Reverse emission coefficient")]
        public Parameter EmissionCoeffR { get; } = new Parameter(1);
        [PropertyName("var"), PropertyName("vb"), PropertyInfo("Reverse Early voltage")]
        public Parameter EarlyVoltR { get; } = new Parameter();
        [PropertyName("ikr"), PropertyInfo("reverse beta roll-off corner current")]
        public Parameter RollOffR { get; } = new Parameter();
        [PropertyName("isc"), PropertyInfo("B-C leakage saturation current")]
        public Parameter LeakBCcurrent { get; } = new Parameter();
        [PropertyName("nc"), PropertyInfo("B-C leakage emission coefficient")]
        public Parameter LeakBCemissionCoeff { get; } = new Parameter(2);
        [PropertyName("rb"), PropertyInfo("Zero bias base resistance")]
        public Parameter BaseResist { get; } = new Parameter();
        [PropertyName("irb"), PropertyInfo("Current for base resistance=(rb+rbm)/2")]
        public Parameter BaseCurrentHalfResist { get; } = new Parameter();
        [PropertyName("rbm"), PropertyInfo("Minimum base resistance")]
        public Parameter MinBaseResist { get; } = new Parameter();
        [PropertyName("re"), PropertyInfo("Emitter resistance")]
        public Parameter EmitterResist { get; } = new Parameter();
        [PropertyName("rc"), PropertyInfo("Collector resistance")]
        public Parameter CollectorResist { get; } = new Parameter();
        [PropertyName("cje"), PropertyInfo("Zero bias B-E depletion capacitance")]
        public Parameter DepletionCapBE { get; } = new Parameter();
        [PropertyName("vje"), PropertyName("pe"), PropertyInfo("B-E built in potential")]
        public Parameter PotentialBE { get; } = new Parameter(0.75);
        [PropertyName("mje"), PropertyName("me"), PropertyInfo("B-E junction grading coefficient")]
        public Parameter JunctionExpBE { get; } = new Parameter(.33);
        [PropertyName("tf"), PropertyInfo("Ideal forward transit time")]
        public Parameter TransitTimeF { get; } = new Parameter();
        [PropertyName("xtf"), PropertyInfo("Coefficient for bias dependence of TF")]
        public Parameter TransitTimeBiasCoeffF { get; } = new Parameter();
        [PropertyName("vtf"), PropertyInfo("Voltage giving VBC dependence of TF")]
        public Parameter TransitTimeFVBC { get; } = new Parameter();
        [PropertyName("itf"), PropertyInfo("High current dependence of TF")]
        public Parameter TransitTimeHighCurrentF { get; } = new Parameter();
        [PropertyName("ptf"), PropertyInfo("Excess phase")]
        public Parameter ExcessPhase { get; } = new Parameter();
        [PropertyName("cjc"), PropertyInfo("Zero bias B-C depletion capacitance")]
        public Parameter DepletionCapBC { get; } = new Parameter();
        [PropertyName("vjc"), PropertyName("pc"), PropertyInfo("B-C built in potential")]
        public Parameter PotentialBC { get; } = new Parameter(0.75);
        [PropertyName("mjc"), PropertyName("mc"), PropertyInfo("B-C junction grading coefficient")]
        public Parameter JunctionExpBC { get; } = new Parameter(.33);
        [PropertyName("xcjc"), PropertyInfo("Fraction of B-C cap to internal base")]
        public Parameter BaseFractionBCcap { get; } = new Parameter(1);
        [PropertyName("tr"), PropertyInfo("Ideal reverse transit time")]
        public Parameter TransitTimeR { get; } = new Parameter();
        [PropertyName("cjs"), PropertyName("ccs"), PropertyInfo("Zero bias C-S capacitance")]
        public Parameter CapCS { get; } = new Parameter();
        [PropertyName("vjs"), PropertyName("ps"), PropertyInfo("Substrate junction built in potential")]
        public Parameter PotentialSubstrate { get; } = new Parameter(.75);
        [PropertyName("mjs"), PropertyName("ms"), PropertyInfo("Substrate junction grading coefficient")]
        public Parameter ExponentialSubstrate { get; } = new Parameter();
        [PropertyName("xtb"), PropertyInfo("Forward and reverse beta temp. exp.")]
        public Parameter BetaExp { get; } = new Parameter();
        [PropertyName("eg"), PropertyInfo("Energy gap for IS temp. dependency")]
        public Parameter EnergyGap { get; } = new Parameter(1.11);
        [PropertyName("xti"), PropertyInfo("Temp. exponent for IS")]
        public Parameter TempExpIS { get; } = new Parameter(3);
        [PropertyName("fc"), PropertyInfo("Forward bias junction fit parameter")]
        public Parameter DepletionCapCoeff { get; } = new Parameter();

        public Parameter C2 { get; } = new Parameter();
        public Parameter C4 { get; } = new Parameter();

        /// <summary>
        /// Constants
        /// </summary>
        public const int NPN = 1;
        public const int PNP = -1;
    }
}
