using SpiceSharp.Attributes;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="BipolarJunctionTransistorModel"/>
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("npn"), ParameterInfo("NPN type device")]
        public void SetNpn(bool value)
        {
            if (value)
                BipolarType = Npn;
        }
        [ParameterName("pnp"), ParameterInfo("PNP type device")]
        public void SetPnp(bool value)
        {
            if (value)
                BipolarType = Pnp;
        }
        [ParameterName("type"), ParameterInfo("NPN or PNP")]
        public string TypeName
        {
            get
            {
                if (BipolarType > 0)
                    return "npn";
                return "pnp";
            }
        }
        public double BipolarType { get; internal set; } = Npn;
        [ParameterName("tnom"), ParameterInfo("Parameter measurement temperature")]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Circuit.CelsiusKelvin;
            set => NominalTemperature.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter NominalTemperature { get; } = new Parameter();
        [ParameterName("is"), ParameterInfo("Saturation Current")]
        public Parameter SatCur { get; } = new Parameter(1e-16);
        [ParameterName("bf"), ParameterInfo("Ideal forward beta")]
        public Parameter BetaF { get; } = new Parameter(100);
        [ParameterName("nf"), ParameterInfo("Forward emission coefficient")]
        public Parameter EmissionCoefficientForward { get; } = new Parameter(1);
        [ParameterName("vaf"), ParameterName("va"), ParameterInfo("Forward Early voltage")]
        public Parameter EarlyVoltageForward { get; } = new Parameter();
        [ParameterName("ikf"), ParameterName("ik"), ParameterInfo("Forward beta roll-off corner current")]
        public Parameter RollOffForward { get; } = new Parameter();
        [ParameterName("ise"), ParameterInfo("B-E leakage saturation current")]
        public Parameter LeakBeCurrent { get; } = new Parameter();
        [ParameterName("ne"), ParameterInfo("B-E leakage emission coefficient")]
        public Parameter LeakBeEmissionCoefficient { get; } = new Parameter(1.5);
        [ParameterName("br"), ParameterInfo("Ideal reverse beta")]
        public Parameter BetaR { get; } = new Parameter(1);
        [ParameterName("nr"), ParameterInfo("Reverse emission coefficient")]
        public Parameter EmissionCoefficientReverse { get; } = new Parameter(1);
        [ParameterName("var"), ParameterName("vb"), ParameterInfo("Reverse Early voltage")]
        public Parameter EarlyVoltageReverse { get; } = new Parameter();
        [ParameterName("ikr"), ParameterInfo("reverse beta roll-off corner current")]
        public Parameter RollOffReverse { get; } = new Parameter();
        [ParameterName("isc"), ParameterInfo("B-C leakage saturation current")]
        public Parameter LeakBcCurrent { get; } = new Parameter();
        [ParameterName("nc"), ParameterInfo("B-C leakage emission coefficient")]
        public Parameter LeakBcEmissionCoefficient { get; } = new Parameter(2);
        [ParameterName("rb"), ParameterInfo("Zero bias base resistance")]
        public Parameter BaseResist { get; } = new Parameter();
        [ParameterName("irb"), ParameterInfo("Current for base resistance=(rb+rbm)/2")]
        public Parameter BaseCurrentHalfResist { get; } = new Parameter();
        [ParameterName("rbm"), ParameterInfo("Minimum base resistance")]
        public Parameter MinimumBaseResistance { get; } = new Parameter();
        [ParameterName("re"), ParameterInfo("Emitter resistance")]
        public Parameter EmitterResistance { get; } = new Parameter();
        [ParameterName("rc"), ParameterInfo("Collector resistance")]
        public Parameter CollectorResistance { get; } = new Parameter();
        [ParameterName("cje"), ParameterInfo("Zero bias B-E depletion capacitance")]
        public Parameter DepletionCapBe { get; } = new Parameter();
        [ParameterName("vje"), ParameterName("pe"), ParameterInfo("B-E built in potential")]
        public Parameter PotentialBe { get; } = new Parameter(0.75);
        [ParameterName("mje"), ParameterName("me"), ParameterInfo("B-E junction grading coefficient")]
        public Parameter JunctionExpBe { get; } = new Parameter(.33);
        [ParameterName("tf"), ParameterInfo("Ideal forward transit time")]
        public Parameter TransitTimeForward { get; } = new Parameter();
        [ParameterName("xtf"), ParameterInfo("Coefficient for bias dependence of TF")]
        public Parameter TransitTimeBiasCoefficientForward { get; } = new Parameter();
        [ParameterName("vtf"), ParameterInfo("Voltage giving VBC dependence of TF")]
        public Parameter TransitTimeForwardVoltageBc { get; } = new Parameter();
        [ParameterName("itf"), ParameterInfo("High current dependence of TF")]
        public Parameter TransitTimeHighCurrentForward { get; } = new Parameter();
        [ParameterName("ptf"), ParameterInfo("Excess phase")]
        public Parameter ExcessPhase { get; } = new Parameter();
        [ParameterName("cjc"), ParameterInfo("Zero bias B-C depletion capacitance")]
        public Parameter DepletionCapBc { get; } = new Parameter();
        [ParameterName("vjc"), ParameterName("pc"), ParameterInfo("B-C built in potential")]
        public Parameter PotentialBc { get; } = new Parameter(0.75);
        [ParameterName("mjc"), ParameterName("mc"), ParameterInfo("B-C junction grading coefficient")]
        public Parameter JunctionExpBc { get; } = new Parameter(.33);
        [ParameterName("xcjc"), ParameterInfo("Fraction of B-C cap to internal base")]
        public Parameter BaseFractionBcCap { get; } = new Parameter(1);
        [ParameterName("tr"), ParameterInfo("Ideal reverse transit time")]
        public Parameter TransitTimeReverse { get; } = new Parameter();
        [ParameterName("cjs"), ParameterName("ccs"), ParameterInfo("Zero bias C-S capacitance")]
        public Parameter CapCs { get; } = new Parameter();
        [ParameterName("vjs"), ParameterName("ps"), ParameterInfo("Substrate junction built in potential")]
        public Parameter PotentialSubstrate { get; } = new Parameter(.75);
        [ParameterName("mjs"), ParameterName("ms"), ParameterInfo("Substrate junction grading coefficient")]
        public Parameter ExponentialSubstrate { get; } = new Parameter();
        [ParameterName("xtb"), ParameterInfo("Forward and reverse beta temperature exponent")]
        public Parameter BetaExponent { get; } = new Parameter();
        [ParameterName("eg"), ParameterInfo("Energy gap for IS temperature dependency")]
        public Parameter EnergyGap { get; } = new Parameter(1.11);
        [ParameterName("xti"), ParameterInfo("Temp. exponent for IS")]
        public Parameter TempExpIs { get; } = new Parameter(3);
        [ParameterName("fc"), ParameterInfo("Forward bias junction fit parameter")]
        public Parameter DepletionCapCoefficient { get; } = new Parameter();

        /// <summary>
        /// Parameter that is not accessible in Spice 3f5
        /// </summary>
        public Parameter C2 { get; } = new Parameter();

        /// <summary>
        /// Parameter that is not accessible in Spice 3f5
        /// </summary>
        public Parameter C4 { get; } = new Parameter();

        /// <summary>
        /// Constants
        /// </summary>
        public const int Npn = 1;
        public const int Pnp = -1;
    }
}
