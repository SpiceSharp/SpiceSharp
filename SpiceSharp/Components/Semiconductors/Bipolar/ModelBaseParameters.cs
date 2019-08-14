using SpiceSharp.Attributes;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="BipolarJunctionTransistorModel"/>
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Set the model to be an NPN transistor.
        /// </summary>
        [ParameterName("npn"), ParameterInfo("NPN type device")]
        public void SetNpn(bool value)
        {
            if (value)
                BipolarType = Npn;
        }

        /// <summary>
        /// Set the model to be a PNP transistor.
        /// </summary>
        /// <param name="value"></param>
        [ParameterName("pnp"), ParameterInfo("PNP type device")]
        public void SetPnp(bool value)
        {
            if (value)
                BipolarType = Pnp;
        }

        /// <summary>
        /// Gets the type of the model ("npn" or "pnp").
        /// </summary>
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

        /// <summary>
        /// Gets or sets the scalar bipolar type (1.0 for NPN or -1.0 for PNP).
        /// </summary>
        public double BipolarType { get; protected set; } = Npn;

        /// <summary>
        /// Gets or sets the nominal temperature in degrees Celsius.
        /// </summary>
        [ParameterName("tnom"), DerivedProperty(), ParameterInfo("Parameter measurement temperature")]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Constants.CelsiusKelvin;
            set => NominalTemperature.Value = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the nominal temperature parameter in degrees Kelvin.
        /// </summary>
        public GivenParameter<double> NominalTemperature { get; } = new GivenParameter<double>(Constants.ReferenceTemperature);

        /// <summary>
        /// Gets the saturation current parameter.
        /// </summary>
        [ParameterName("is"), ParameterInfo("Saturation Current")]
        public GivenParameter<double> SatCur { get; } = new GivenParameter<double>(1e-16);

        /// <summary>
        /// Gets the ideal forward beta parameter.
        /// </summary>
        [ParameterName("bf"), ParameterInfo("Ideal forward beta")]
        public GivenParameter<double> BetaF { get; } = new GivenParameter<double>(100);

        /// <summary>
        /// Gets the forward emission coefficient parameter.
        /// </summary>
        [ParameterName("nf"), ParameterInfo("Forward emission coefficient")]
        public GivenParameter<double> EmissionCoefficientForward { get; } = new GivenParameter<double>(1);

        /// <summary>
        /// Gets the forward Early voltage parameter.
        /// </summary>
        [ParameterName("vaf"), ParameterName("va"), ParameterInfo("Forward Early voltage")]
        public GivenParameter<double> EarlyVoltageForward { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the forward beta roll-off corner current parameter.
        /// </summary>
        [ParameterName("ikf"), ParameterName("ik"), ParameterInfo("Forward beta roll-off corner current")]
        public GivenParameter<double> RollOffForward { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the base-emitter saturation current parameter.
        /// </summary>
        [ParameterName("ise"), ParameterInfo("B-E leakage saturation current")]
        public GivenParameter<double> LeakBeCurrent { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the base-emitter emission coefficient parameter.
        /// </summary>
        [ParameterName("ne"), ParameterInfo("B-E leakage emission coefficient")]
        public GivenParameter<double> LeakBeEmissionCoefficient { get; } = new GivenParameter<double>(1.5);

        /// <summary>
        /// Gets the ideal reverse beta parameter.
        /// </summary>
        [ParameterName("br"), ParameterInfo("Ideal reverse beta")]
        public GivenParameter<double> BetaR { get; } = new GivenParameter<double>(1);

        /// <summary>
        /// Gets the reverse emission coefficient parameter.
        /// </summary>
        [ParameterName("nr"), ParameterInfo("Reverse emission coefficient")]
        public GivenParameter<double> EmissionCoefficientReverse { get; } = new GivenParameter<double>(1);

        /// <summary>
        /// Gets the reverse Early voltage parameter.
        /// </summary>
        [ParameterName("var"), ParameterName("vb"), ParameterInfo("Reverse Early voltage")]
        public GivenParameter<double> EarlyVoltageReverse { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the reverse beta roll-off corner current parameter.
        /// </summary>
        [ParameterName("ikr"), ParameterInfo("reverse beta roll-off corner current")]
        public GivenParameter<double> RollOffReverse { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the base-collector saturation current parameter.
        /// </summary>
        [ParameterName("isc"), ParameterInfo("B-C leakage saturation current")]
        public GivenParameter<double> LeakBcCurrent { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the base-collector emission coefficient parameter.
        /// </summary>
        [ParameterName("nc"), ParameterInfo("B-C leakage emission coefficient")]
        public GivenParameter<double> LeakBcEmissionCoefficient { get; } = new GivenParameter<double>(2);

        /// <summary>
        /// Gets the zero-bias base resistance parameter.
        /// </summary>
        [ParameterName("rb"), ParameterInfo("Zero bias base resistance")]
        public GivenParameter<double> BaseResist { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the current for base resistance (rb + rbm) / 2 parameter.
        /// </summary>
        [ParameterName("irb"), ParameterInfo("Current for base resistance=(rb+rbm)/2")]
        public GivenParameter<double> BaseCurrentHalfResist { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the minimum base resistance parameter.
        /// </summary>
        [ParameterName("rbm"), ParameterInfo("Minimum base resistance")]
        public GivenParameter<double> MinimumBaseResistance { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the emitter resistance parameter.
        /// </summary>
        [ParameterName("re"), ParameterInfo("Emitter resistance")]
        public GivenParameter<double> EmitterResistance { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the collector resistance parameter.
        /// </summary>
        [ParameterName("rc"), ParameterInfo("Collector resistance")]
        public GivenParameter<double> CollectorResistance { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the zero-bias base-emitter depletion capacitance parameter.
        /// </summary>
        [ParameterName("cje"), ParameterInfo("Zero bias B-E depletion capacitance")]
        public GivenParameter<double> DepletionCapBe { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the base-emitter built-in potential parameter.
        /// </summary>
        [ParameterName("vje"), ParameterName("pe"), ParameterInfo("B-E built in potential")]
        public GivenParameter<double> PotentialBe { get; } = new GivenParameter<double>(0.75);

        /// <summary>
        /// Gets the base-emitter junction grading coefficient parameter.
        /// </summary>
        [ParameterName("mje"), ParameterName("me"), ParameterInfo("B-E junction grading coefficient")]
        public GivenParameter<double> JunctionExpBe { get; } = new GivenParameter<double>(.33);

        /// <summary>
        /// Gets the ideal forward transit time parameter.
        /// </summary>
        [ParameterName("tf"), ParameterInfo("Ideal forward transit time")]
        public GivenParameter<double> TransitTimeForward { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the coefficient for bias dependence parameter of the forward transit time.
        /// </summary>
        [ParameterName("xtf"), ParameterInfo("Coefficient for bias dependence of TF")]
        public GivenParameter<double> TransitTimeBiasCoefficientForward { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the voltage giving the base-collector voltage dependence parameter of the forward transit time.
        /// </summary>
        [ParameterName("vtf"), ParameterInfo("Voltage giving VBC dependence of TF")]
        public GivenParameter<double> TransitTimeForwardVoltageBc { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the high-current dependence parameter of the forward transit time.
        /// </summary>
        [ParameterName("itf"), ParameterInfo("High current dependence of TF")]
        public GivenParameter<double> TransitTimeHighCurrentForward { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the excess phase parameter.
        /// </summary>
        [ParameterName("ptf"), ParameterInfo("Excess phase")]
        public GivenParameter<double> ExcessPhase { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the zero-bias base-collector depletion capacitance parameter.
        /// </summary>
        [ParameterName("cjc"), ParameterInfo("Zero bias B-C depletion capacitance")]
        public GivenParameter<double> DepletionCapBc { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the base-collector built-in potential parameter.
        /// </summary>
        [ParameterName("vjc"), ParameterName("pc"), ParameterInfo("B-C built in potential")]
        public GivenParameter<double> PotentialBc { get; } = new GivenParameter<double>(0.75);

        /// <summary>
        /// Gets the base-collector junction grading coefficient parameter.
        /// </summary>
        [ParameterName("mjc"), ParameterName("mc"), ParameterInfo("B-C junction grading coefficient")]
        public GivenParameter<double> JunctionExpBc { get; } = new GivenParameter<double>(.33);

        /// <summary>
        /// Gets the fraction of base-collector capacitance to the internal base parameter.
        /// </summary>
        [ParameterName("xcjc"), ParameterInfo("Fraction of B-C cap to internal base")]
        public GivenParameter<double> BaseFractionBcCap { get; } = new GivenParameter<double>(1);

        /// <summary>
        /// Gets the ideal reverse transit time parameter.
        /// </summary>
        [ParameterName("tr"), ParameterInfo("Ideal reverse transit time")]
        public GivenParameter<double> TransitTimeReverse { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the zero-bias collector-substrate capacitance parameter.
        /// </summary>
        [ParameterName("cjs"), ParameterName("ccs"), ParameterInfo("Zero bias C-S capacitance")]
        public GivenParameter<double> CapCs { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the substrate junction built-in potential parameter.
        /// </summary>
        [ParameterName("vjs"), ParameterName("ps"), ParameterInfo("Substrate junction built in potential")]
        public GivenParameter<double> PotentialSubstrate { get; } = new GivenParameter<double>(.75);

        /// <summary>
        /// Gets the substrate junction grading coefficient parameter.
        /// </summary>
        [ParameterName("mjs"), ParameterName("ms"), ParameterInfo("Substrate junction grading coefficient")]
        public GivenParameter<double> ExponentialSubstrate { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the forward and reverse beta temperature exponent parameter.
        /// </summary>
        [ParameterName("xtb"), ParameterInfo("Forward and reverse beta temperature exponent")]
        public GivenParameter<double> BetaExponent { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the energy gap parameter for saturation current temperature dependency.
        /// </summary>
        [ParameterName("eg"), ParameterInfo("Energy gap for IS temperature dependency")]
        public GivenParameter<double> EnergyGap { get; } = new GivenParameter<double>(1.11);

        /// <summary>
        /// Gets the temperature exponent parameter for the saturation current.
        /// </summary>
        [ParameterName("xti"), ParameterInfo("Temperature exponent for IS")]
        public GivenParameter<double> TempExpIs { get; } = new GivenParameter<double>(3);

        /// <summary>
        /// Gets the forward bias junction fit parameter.
        /// </summary>
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
        /// Scalar used for NPN transistors.
        /// </summary>
        public const int Npn = 1;

        /// <summary>
        /// Scalar used for PNP transistors.
        /// </summary>
        public const int Pnp = -1;
    }
}
