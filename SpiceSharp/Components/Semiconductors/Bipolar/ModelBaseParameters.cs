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
        [ParameterName("npn"), PropertyInfo("NPN type device")]
        public void SetNpn(bool value)
        {
            if (value)
                BipolarType = Npn;
        }
        [ParameterName("pnp"), PropertyInfo("PNP type device")]
        public void SetPnp(bool value)
        {
            if (value)
                BipolarType = Pnp;
        }
        [ParameterName("type"), PropertyInfo("NPN or PNP")]
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
        [ParameterName("tnom"), PropertyInfo("Parameter measurement temperature")]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Circuit.CelsiusKelvin;
            set => NominalTemperature.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter NominalTemperature { get; } = new Parameter();
        [ParameterName("is"), PropertyInfo("Saturation Current")]
        public Parameter SatCur { get; } = new Parameter(1e-16);
        [ParameterName("bf"), PropertyInfo("Ideal forward beta")]
        public Parameter BetaF { get; } = new Parameter(100);
        [ParameterName("nf"), PropertyInfo("Forward emission coefficient")]
        public Parameter EmissionCoefficientForward { get; } = new Parameter(1);
        [ParameterName("vaf"), ParameterName("va"), PropertyInfo("Forward Early voltage")]
        public Parameter EarlyVoltageForward { get; } = new Parameter();
        [ParameterName("ikf"), ParameterName("ik"), PropertyInfo("Forward beta roll-off corner current")]
        public Parameter RollOffForward { get; } = new Parameter();
        [ParameterName("ise"), PropertyInfo("B-E leakage saturation current")]
        public Parameter LeakBeCurrent { get; } = new Parameter();
        [ParameterName("ne"), PropertyInfo("B-E leakage emission coefficient")]
        public Parameter LeakBeEmissionCoefficient { get; } = new Parameter(1.5);
        [ParameterName("br"), PropertyInfo("Ideal reverse beta")]
        public Parameter BetaR { get; } = new Parameter(1);
        [ParameterName("nr"), PropertyInfo("Reverse emission coefficient")]
        public Parameter EmissionCoefficientReverse { get; } = new Parameter(1);
        [ParameterName("var"), ParameterName("vb"), PropertyInfo("Reverse Early voltage")]
        public Parameter EarlyVoltageReverse { get; } = new Parameter();
        [ParameterName("ikr"), PropertyInfo("reverse beta roll-off corner current")]
        public Parameter RollOffReverse { get; } = new Parameter();
        [ParameterName("isc"), PropertyInfo("B-C leakage saturation current")]
        public Parameter LeakBcCurrent { get; } = new Parameter();
        [ParameterName("nc"), PropertyInfo("B-C leakage emission coefficient")]
        public Parameter LeakBcEmissionCoefficient { get; } = new Parameter(2);
        [ParameterName("rb"), PropertyInfo("Zero bias base resistance")]
        public Parameter BaseResist { get; } = new Parameter();
        [ParameterName("irb"), PropertyInfo("Current for base resistance=(rb+rbm)/2")]
        public Parameter BaseCurrentHalfResist { get; } = new Parameter();
        [ParameterName("rbm"), PropertyInfo("Minimum base resistance")]
        public Parameter MinimumBaseResistance { get; } = new Parameter();
        [ParameterName("re"), PropertyInfo("Emitter resistance")]
        public Parameter EmitterResistance { get; } = new Parameter();
        [ParameterName("rc"), PropertyInfo("Collector resistance")]
        public Parameter CollectorResistance { get; } = new Parameter();
        [ParameterName("cje"), PropertyInfo("Zero bias B-E depletion capacitance")]
        public Parameter DepletionCapBe { get; } = new Parameter();
        [ParameterName("vje"), ParameterName("pe"), PropertyInfo("B-E built in potential")]
        public Parameter PotentialBe { get; } = new Parameter(0.75);
        [ParameterName("mje"), ParameterName("me"), PropertyInfo("B-E junction grading coefficient")]
        public Parameter JunctionExpBe { get; } = new Parameter(.33);
        [ParameterName("tf"), PropertyInfo("Ideal forward transit time")]
        public Parameter TransitTimeForward { get; } = new Parameter();
        [ParameterName("xtf"), PropertyInfo("Coefficient for bias dependence of TF")]
        public Parameter TransitTimeBiasCoefficientForward { get; } = new Parameter();
        [ParameterName("vtf"), PropertyInfo("Voltage giving VBC dependence of TF")]
        public Parameter TransitTimeForwardVoltageBc { get; } = new Parameter();
        [ParameterName("itf"), PropertyInfo("High current dependence of TF")]
        public Parameter TransitTimeHighCurrentForward { get; } = new Parameter();
        [ParameterName("ptf"), PropertyInfo("Excess phase")]
        public Parameter ExcessPhase { get; } = new Parameter();
        [ParameterName("cjc"), PropertyInfo("Zero bias B-C depletion capacitance")]
        public Parameter DepletionCapBc { get; } = new Parameter();
        [ParameterName("vjc"), ParameterName("pc"), PropertyInfo("B-C built in potential")]
        public Parameter PotentialBc { get; } = new Parameter(0.75);
        [ParameterName("mjc"), ParameterName("mc"), PropertyInfo("B-C junction grading coefficient")]
        public Parameter JunctionExpBc { get; } = new Parameter(.33);
        [ParameterName("xcjc"), PropertyInfo("Fraction of B-C cap to internal base")]
        public Parameter BaseFractionBcCap { get; } = new Parameter(1);
        [ParameterName("tr"), PropertyInfo("Ideal reverse transit time")]
        public Parameter TransitTimeReverse { get; } = new Parameter();
        [ParameterName("cjs"), ParameterName("ccs"), PropertyInfo("Zero bias C-S capacitance")]
        public Parameter CapCs { get; } = new Parameter();
        [ParameterName("vjs"), ParameterName("ps"), PropertyInfo("Substrate junction built in potential")]
        public Parameter PotentialSubstrate { get; } = new Parameter(.75);
        [ParameterName("mjs"), ParameterName("ms"), PropertyInfo("Substrate junction grading coefficient")]
        public Parameter ExponentialSubstrate { get; } = new Parameter();
        [ParameterName("xtb"), PropertyInfo("Forward and reverse beta temperature exponent")]
        public Parameter BetaExponent { get; } = new Parameter();
        [ParameterName("eg"), PropertyInfo("Energy gap for IS temperature dependency")]
        public Parameter EnergyGap { get; } = new Parameter(1.11);
        [ParameterName("xti"), PropertyInfo("Temp. exponent for IS")]
        public Parameter TempExpIs { get; } = new Parameter(3);
        [ParameterName("fc"), PropertyInfo("Forward bias junction fit parameter")]
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
