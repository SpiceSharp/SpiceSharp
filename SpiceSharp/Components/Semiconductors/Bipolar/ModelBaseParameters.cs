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
        public double BipolarType { get; protected set; } = Npn;
        [ParameterName("tnom"), DerivedProperty(), ParameterInfo("Parameter measurement temperature")]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Constants.CelsiusKelvin;
            set => NominalTemperature.Value = value + Constants.CelsiusKelvin;
        }
        public GivenParameter<double> NominalTemperature { get; } = new GivenParameter<double>(Constants.ReferenceTemperature);
        [ParameterName("is"), ParameterInfo("Saturation Current")]
        public GivenParameter<double> SatCur { get; } = new GivenParameter<double>(1e-16);
        [ParameterName("bf"), ParameterInfo("Ideal forward beta")]
        public GivenParameter<double> BetaF { get; } = new GivenParameter<double>(100);
        [ParameterName("nf"), ParameterInfo("Forward emission coefficient")]
        public GivenParameter<double> EmissionCoefficientForward { get; } = new GivenParameter<double>(1);
        [ParameterName("vaf"), ParameterName("va"), ParameterInfo("Forward Early voltage")]
        public GivenParameter<double> EarlyVoltageForward { get; } = new GivenParameter<double>();
        [ParameterName("ikf"), ParameterName("ik"), ParameterInfo("Forward beta roll-off corner current")]
        public GivenParameter<double> RollOffForward { get; } = new GivenParameter<double>();
        [ParameterName("ise"), ParameterInfo("B-E leakage saturation current")]
        public GivenParameter<double> LeakBeCurrent { get; } = new GivenParameter<double>();
        [ParameterName("ne"), ParameterInfo("B-E leakage emission coefficient")]
        public GivenParameter<double> LeakBeEmissionCoefficient { get; } = new GivenParameter<double>(1.5);
        [ParameterName("br"), ParameterInfo("Ideal reverse beta")]
        public GivenParameter<double> BetaR { get; } = new GivenParameter<double>(1);
        [ParameterName("nr"), ParameterInfo("Reverse emission coefficient")]
        public GivenParameter<double> EmissionCoefficientReverse { get; } = new GivenParameter<double>(1);
        [ParameterName("var"), ParameterName("vb"), ParameterInfo("Reverse Early voltage")]
        public GivenParameter<double> EarlyVoltageReverse { get; } = new GivenParameter<double>();
        [ParameterName("ikr"), ParameterInfo("reverse beta roll-off corner current")]
        public GivenParameter<double> RollOffReverse { get; } = new GivenParameter<double>();
        [ParameterName("isc"), ParameterInfo("B-C leakage saturation current")]
        public GivenParameter<double> LeakBcCurrent { get; } = new GivenParameter<double>();
        [ParameterName("nc"), ParameterInfo("B-C leakage emission coefficient")]
        public GivenParameter<double> LeakBcEmissionCoefficient { get; } = new GivenParameter<double>(2);
        [ParameterName("rb"), ParameterInfo("Zero bias base resistance")]
        public GivenParameter<double> BaseResist { get; } = new GivenParameter<double>();
        [ParameterName("irb"), ParameterInfo("Current for base resistance=(rb+rbm)/2")]
        public GivenParameter<double> BaseCurrentHalfResist { get; } = new GivenParameter<double>();
        [ParameterName("rbm"), ParameterInfo("Minimum base resistance")]
        public GivenParameter<double> MinimumBaseResistance { get; } = new GivenParameter<double>();
        [ParameterName("re"), ParameterInfo("Emitter resistance")]
        public GivenParameter<double> EmitterResistance { get; } = new GivenParameter<double>();
        [ParameterName("rc"), ParameterInfo("Collector resistance")]
        public GivenParameter<double> CollectorResistance { get; } = new GivenParameter<double>();
        [ParameterName("cje"), ParameterInfo("Zero bias B-E depletion capacitance")]
        public GivenParameter<double> DepletionCapBe { get; } = new GivenParameter<double>();
        [ParameterName("vje"), ParameterName("pe"), ParameterInfo("B-E built in potential")]
        public GivenParameter<double> PotentialBe { get; } = new GivenParameter<double>(0.75);
        [ParameterName("mje"), ParameterName("me"), ParameterInfo("B-E junction grading coefficient")]
        public GivenParameter<double> JunctionExpBe { get; } = new GivenParameter<double>(.33);
        [ParameterName("tf"), ParameterInfo("Ideal forward transit time")]
        public GivenParameter<double> TransitTimeForward { get; } = new GivenParameter<double>();
        [ParameterName("xtf"), ParameterInfo("Coefficient for bias dependence of TF")]
        public GivenParameter<double> TransitTimeBiasCoefficientForward { get; } = new GivenParameter<double>();
        [ParameterName("vtf"), ParameterInfo("Voltage giving VBC dependence of TF")]
        public GivenParameter<double> TransitTimeForwardVoltageBc { get; } = new GivenParameter<double>();
        [ParameterName("itf"), ParameterInfo("High current dependence of TF")]
        public GivenParameter<double> TransitTimeHighCurrentForward { get; } = new GivenParameter<double>();
        [ParameterName("ptf"), ParameterInfo("Excess phase")]
        public GivenParameter<double> ExcessPhase { get; } = new GivenParameter<double>();
        [ParameterName("cjc"), ParameterInfo("Zero bias B-C depletion capacitance")]
        public GivenParameter<double> DepletionCapBc { get; } = new GivenParameter<double>();
        [ParameterName("vjc"), ParameterName("pc"), ParameterInfo("B-C built in potential")]
        public GivenParameter<double> PotentialBc { get; } = new GivenParameter<double>(0.75);
        [ParameterName("mjc"), ParameterName("mc"), ParameterInfo("B-C junction grading coefficient")]
        public GivenParameter<double> JunctionExpBc { get; } = new GivenParameter<double>(.33);
        [ParameterName("xcjc"), ParameterInfo("Fraction of B-C cap to internal base")]
        public GivenParameter<double> BaseFractionBcCap { get; } = new GivenParameter<double>(1);
        [ParameterName("tr"), ParameterInfo("Ideal reverse transit time")]
        public GivenParameter<double> TransitTimeReverse { get; } = new GivenParameter<double>();
        [ParameterName("cjs"), ParameterName("ccs"), ParameterInfo("Zero bias C-S capacitance")]
        public GivenParameter<double> CapCs { get; } = new GivenParameter<double>();
        [ParameterName("vjs"), ParameterName("ps"), ParameterInfo("Substrate junction built in potential")]
        public GivenParameter<double> PotentialSubstrate { get; } = new GivenParameter<double>(.75);
        [ParameterName("mjs"), ParameterName("ms"), ParameterInfo("Substrate junction grading coefficient")]
        public GivenParameter<double> ExponentialSubstrate { get; } = new GivenParameter<double>();
        [ParameterName("xtb"), ParameterInfo("Forward and reverse beta temperature exponent")]
        public GivenParameter<double> BetaExponent { get; } = new GivenParameter<double>();
        [ParameterName("eg"), ParameterInfo("Energy gap for IS temperature dependency")]
        public GivenParameter<double> EnergyGap { get; } = new GivenParameter<double>(1.11);
        [ParameterName("xti"), ParameterInfo("Temp. exponent for IS")]
        public GivenParameter<double> TempExpIs { get; } = new GivenParameter<double>(3);
        [ParameterName("fc"), ParameterInfo("Forward bias junction fit parameter")]
        public GivenParameter<double> DepletionCapCoefficient { get; } = new GivenParameter<double>();

        /// <summary>
        /// Parameter that is not accessible in Spice 3f5
        /// </summary>
        public GivenParameter<double> C2 { get; } = new GivenParameter<double>();

        /// <summary>
        /// Parameter that is not accessible in Spice 3f5
        /// </summary>
        public GivenParameter<double> C4 { get; } = new GivenParameter<double>();

        /// <summary>
        /// Constants
        /// </summary>
        public const int Npn = 1;
        public const int Pnp = -1;
    }
}
