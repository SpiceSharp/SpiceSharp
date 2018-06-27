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
        [ParameterName("tnom"), ComputedProperty(), ParameterInfo("Parameter measurement temperature")]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Circuit.CelsiusKelvin;
            set => NominalTemperature.Value = value + Circuit.CelsiusKelvin;
        }
        public GivenParameter NominalTemperature { get; } = new GivenParameter(Circuit.ReferenceTemperature);
        [ParameterName("is"), ParameterInfo("Saturation Current")]
        public GivenParameter SatCur { get; } = new GivenParameter(1e-16);
        [ParameterName("bf"), ParameterInfo("Ideal forward beta")]
        public GivenParameter BetaF { get; } = new GivenParameter(100);
        [ParameterName("nf"), ParameterInfo("Forward emission coefficient")]
        public GivenParameter EmissionCoefficientForward { get; } = new GivenParameter(1);
        [ParameterName("vaf"), ParameterName("va"), ParameterInfo("Forward Early voltage")]
        public GivenParameter EarlyVoltageForward { get; } = new GivenParameter();
        [ParameterName("ikf"), ParameterName("ik"), ParameterInfo("Forward beta roll-off corner current")]
        public GivenParameter RollOffForward { get; } = new GivenParameter();
        [ParameterName("ise"), ParameterInfo("B-E leakage saturation current")]
        public GivenParameter LeakBeCurrent { get; } = new GivenParameter();
        [ParameterName("ne"), ParameterInfo("B-E leakage emission coefficient")]
        public GivenParameter LeakBeEmissionCoefficient { get; } = new GivenParameter(1.5);
        [ParameterName("br"), ParameterInfo("Ideal reverse beta")]
        public GivenParameter BetaR { get; } = new GivenParameter(1);
        [ParameterName("nr"), ParameterInfo("Reverse emission coefficient")]
        public GivenParameter EmissionCoefficientReverse { get; } = new GivenParameter(1);
        [ParameterName("var"), ParameterName("vb"), ParameterInfo("Reverse Early voltage")]
        public GivenParameter EarlyVoltageReverse { get; } = new GivenParameter();
        [ParameterName("ikr"), ParameterInfo("reverse beta roll-off corner current")]
        public GivenParameter RollOffReverse { get; } = new GivenParameter();
        [ParameterName("isc"), ParameterInfo("B-C leakage saturation current")]
        public GivenParameter LeakBcCurrent { get; } = new GivenParameter();
        [ParameterName("nc"), ParameterInfo("B-C leakage emission coefficient")]
        public GivenParameter LeakBcEmissionCoefficient { get; } = new GivenParameter(2);
        [ParameterName("rb"), ParameterInfo("Zero bias base resistance")]
        public GivenParameter BaseResist { get; } = new GivenParameter();
        [ParameterName("irb"), ParameterInfo("Current for base resistance=(rb+rbm)/2")]
        public GivenParameter BaseCurrentHalfResist { get; } = new GivenParameter();
        [ParameterName("rbm"), ParameterInfo("Minimum base resistance")]
        public GivenParameter MinimumBaseResistance { get; } = new GivenParameter();
        [ParameterName("re"), ParameterInfo("Emitter resistance")]
        public GivenParameter EmitterResistance { get; } = new GivenParameter();
        [ParameterName("rc"), ParameterInfo("Collector resistance")]
        public GivenParameter CollectorResistance { get; } = new GivenParameter();
        [ParameterName("cje"), ParameterInfo("Zero bias B-E depletion capacitance")]
        public GivenParameter DepletionCapBe { get; } = new GivenParameter();
        [ParameterName("vje"), ParameterName("pe"), ParameterInfo("B-E built in potential")]
        public GivenParameter PotentialBe { get; } = new GivenParameter(0.75);
        [ParameterName("mje"), ParameterName("me"), ParameterInfo("B-E junction grading coefficient")]
        public GivenParameter JunctionExpBe { get; } = new GivenParameter(.33);
        [ParameterName("tf"), ParameterInfo("Ideal forward transit time")]
        public GivenParameter TransitTimeForward { get; } = new GivenParameter();
        [ParameterName("xtf"), ParameterInfo("Coefficient for bias dependence of TF")]
        public GivenParameter TransitTimeBiasCoefficientForward { get; } = new GivenParameter();
        [ParameterName("vtf"), ParameterInfo("Voltage giving VBC dependence of TF")]
        public GivenParameter TransitTimeForwardVoltageBc { get; } = new GivenParameter();
        [ParameterName("itf"), ParameterInfo("High current dependence of TF")]
        public GivenParameter TransitTimeHighCurrentForward { get; } = new GivenParameter();
        [ParameterName("ptf"), ParameterInfo("Excess phase")]
        public GivenParameter ExcessPhase { get; } = new GivenParameter();
        [ParameterName("cjc"), ParameterInfo("Zero bias B-C depletion capacitance")]
        public GivenParameter DepletionCapBc { get; } = new GivenParameter();
        [ParameterName("vjc"), ParameterName("pc"), ParameterInfo("B-C built in potential")]
        public GivenParameter PotentialBc { get; } = new GivenParameter(0.75);
        [ParameterName("mjc"), ParameterName("mc"), ParameterInfo("B-C junction grading coefficient")]
        public GivenParameter JunctionExpBc { get; } = new GivenParameter(.33);
        [ParameterName("xcjc"), ParameterInfo("Fraction of B-C cap to internal base")]
        public GivenParameter BaseFractionBcCap { get; } = new GivenParameter(1);
        [ParameterName("tr"), ParameterInfo("Ideal reverse transit time")]
        public GivenParameter TransitTimeReverse { get; } = new GivenParameter();
        [ParameterName("cjs"), ParameterName("ccs"), ParameterInfo("Zero bias C-S capacitance")]
        public GivenParameter CapCs { get; } = new GivenParameter();
        [ParameterName("vjs"), ParameterName("ps"), ParameterInfo("Substrate junction built in potential")]
        public GivenParameter PotentialSubstrate { get; } = new GivenParameter(.75);
        [ParameterName("mjs"), ParameterName("ms"), ParameterInfo("Substrate junction grading coefficient")]
        public GivenParameter ExponentialSubstrate { get; } = new GivenParameter();
        [ParameterName("xtb"), ParameterInfo("Forward and reverse beta temperature exponent")]
        public GivenParameter BetaExponent { get; } = new GivenParameter();
        [ParameterName("eg"), ParameterInfo("Energy gap for IS temperature dependency")]
        public GivenParameter EnergyGap { get; } = new GivenParameter(1.11);
        [ParameterName("xti"), ParameterInfo("Temp. exponent for IS")]
        public GivenParameter TempExpIs { get; } = new GivenParameter(3);
        [ParameterName("fc"), ParameterInfo("Forward bias junction fit parameter")]
        public GivenParameter DepletionCapCoefficient { get; } = new GivenParameter();

        /// <summary>
        /// Parameter that is not accessible in Spice 3f5
        /// </summary>
        public GivenParameter C2 { get; } = new GivenParameter();

        /// <summary>
        /// Parameter that is not accessible in Spice 3f5
        /// </summary>
        public GivenParameter C4 { get; } = new GivenParameter();

        /// <summary>
        /// Constants
        /// </summary>
        public const int Npn = 1;
        public const int Pnp = -1;
    }
}
