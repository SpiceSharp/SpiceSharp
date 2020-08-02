using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components.Bipolars
{
    /// <summary>
    /// Base parameters for a <see cref="BipolarJunctionTransistorModel"/>
    /// </summary>
    /// <seealso cref="ParameterSet"/>
    [GeneratedParameters]
    public class ModelParameters : ParameterSet
    {
        private double _c4;
        private double _c2;
        private GivenParameter<double> _depletionCapCoefficient = new GivenParameter<double>(0.5, false);
        private double _energyGap = 1.11;
        private double _betaExponent;
        private double _exponentialSubstrate;
        private double _potentialSubstrate = 0.75;
        private double _capCs;
        private double _transitTimeReverse;
        private double _baseFractionBcCap = 1.0;
        private double _junctionExpBc = 0.33;
        private double _potentialBc = 0.75;
        private double _depletionCapBc;
        private double _excessPhase;
        private double _transitTimeHighCurrentForward;
        private double _transitTimeForwardVoltageBc;
        private double _transitTimeBiasCoefficientForward;
        private double _transitTimeForward;
        private double _junctionExpBe = 0.33;
        private double _potentialBe = 0.75;
        private double _depletionCapBe;
        private double _collectorResistance;
        private double _emitterResistance;
        private GivenParameter<double> _minimumBaseResistance;
        private double _baseCurrentHalfResist;
        private double _baseResist;
        private double _leakBcEmissionCoefficient = 2;
        private GivenParameter<double> _leakBcCurrent;
        private double _rollOffReverse;
        private double _earlyVoltageReverse;
        private double _emissionCoefficientReverse = 1;
        private double _betaR = 1;
        private double _leakBeEmissionCoefficient = 1.5;
        private GivenParameter<double> _leakBeCurrent;
        private double _rollOffForward;
        private double _earlyVoltageForward;
        private double _emissionCoefficientForward = 1;
        private double _betaF = 100;
        private double _satCur = 1e-16;
        private GivenParameter<double> _nominalTemperature = new GivenParameter<double>(Constants.ReferenceTemperature, false);

        /// <summary>
        /// Scalar used for NPN transistors.
        /// </summary>
        public const int Npn = 1;

        /// <summary>
        /// Scalar used for PNP transistors.
        /// </summary>
        public const int Pnp = -1;

        /// <summary>
        /// Set the model to be an NPN transistor.
        /// </summary>
        /// <param name="value">If set to <c>true</c>, the model is set to describe an NPN transistor.</param>
        [ParameterName("npn"), ParameterInfo("NPN type device")]
        public void SetNpn(bool value)
        {
            if (value)
                BipolarType = Npn;
        }

        /// <summary>
        /// Set the model to be a PNP transistor.
        /// </summary>
        /// <param name="value">If set to <c>true</c>, the model is set to describe a PNP transistor.</param>
        [ParameterName("pnp"), ParameterInfo("PNP type device")]
        public void SetPnp(bool value)
        {
            if (value)
                BipolarType = Pnp;
        }

        /// <summary>
        /// Gets the type of the model ("npn" or "pnp").
        /// </summary>
        /// <value>
        /// The name of the type.
        /// </value>
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
        /// <value>
        /// The type of the bipolar.
        /// </value>
        public double BipolarType { get; protected set; } = Npn;

        /// <summary>
        /// Gets or sets the nominal temperature in degrees Celsius.
        /// </summary>
        /// <value>
        /// The nominal temperature in degrees Celsius.
        /// </value>
        [ParameterName("tnom"), ParameterInfo("Parameter measurement temperature", Units = "\u00b0C")]
        [DerivedProperty(), GreaterThan(Constants.CelsiusKelvin)]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Constants.CelsiusKelvin;
            set => NominalTemperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets or sets the nominal temperature parameter in degrees Kelvin.
        /// </summary>
        /// <value>
        /// The nominal temperature in degrees Kelvin.
        /// </value>
        [GreaterThan(0)]
        public GivenParameter<double> NominalTemperature
        {
            get => _nominalTemperature;
            set
            {
                Utility.GreaterThan(value, nameof(NominalTemperature), 0);
                _nominalTemperature = value;
            }
        }

        /// <summary>
        /// Gets or sets the saturation current.
        /// </summary>
        /// <value>
        /// The saturation current.
        /// </value>
        [ParameterName("is"), ParameterInfo("Saturation Current", Units = "A")]
        [GreaterThan(0)]
        public double SatCur
        {
            get => _satCur;
            set
            {
                Utility.GreaterThan(value, nameof(SatCur), 0);
                _satCur = value;
            }
        }

        /// <summary>
        /// Gets or sets the ideal forward beta parameter.
        /// </summary>
        /// <value>
        /// The ideal forward beta parameter.
        /// </value>
        [ParameterName("bf"), ParameterInfo("Ideal forward beta")]
        [GreaterThan(0)]
        public double BetaF
        {
            get => _betaF;
            set
            {
                Utility.GreaterThan(value, nameof(BetaF), 0);
                _betaF = value;
            }
        }

        /// <summary>
        /// Gets or sets the forward emission coefficient.
        /// </summary>
        /// <value>
        /// The forward emission coefficient.
        /// </value>
        [ParameterName("nf"), ParameterInfo("Forward emission coefficient")]
        [GreaterThan(0)]
        public double EmissionCoefficientForward
        {
            get => _emissionCoefficientForward;
            set
            {
                Utility.GreaterThan(value, nameof(EmissionCoefficientForward), 0);
                _emissionCoefficientForward = value;
            }
        }

        /// <summary>
        /// Gets or sets the forward Early voltage.
        /// </summary>
        /// <value>
        /// The forward Early voltage.
        /// </value>
        [ParameterName("vaf"), ParameterName("va"), ParameterInfo("Forward Early voltage", Units = "V")]
        [GreaterThanOrEquals(0)]
        public double EarlyVoltageForward
        {
            get => _earlyVoltageForward;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(EarlyVoltageForward), 0);
                _earlyVoltageForward = value;
            }
        }

        /// <summary>
        /// Gets or sets the forward beta roll-off corner current.
        /// </summary>
        /// <value>
        /// The forward beta roll-off corner current.
        /// </value>
        [ParameterName("ikf"), ParameterName("ik"), ParameterInfo("Forward beta roll-off corner current")]
        [GreaterThanOrEquals(0)]
        public double RollOffForward
        {
            get => _rollOffForward;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(RollOffForward), 0);
                _rollOffForward = value;
            }
        }

        /// <summary>
        /// Gets or sets the base-emitter saturation current.
        /// </summary>
        /// <value>
        /// The base-emitter leakage saturation current.
        /// </value>
        [ParameterName("ise"), ParameterInfo("B-E leakage saturation current", Units = "A")]
        [GreaterThanOrEquals(0)]
        public GivenParameter<double> LeakBeCurrent
        {
            get => _leakBeCurrent;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(LeakBeCurrent), 0);
                _leakBeCurrent = value;
            }
        }

        /// <summary>
        /// Gets or sets the base-emitter emission coefficient.
        /// </summary>
        /// <value>
        /// The base-emitter emission coefficient.
        /// </value>
        [ParameterName("ne"), ParameterInfo("B-E leakage emission coefficient")]
        [GreaterThan(0)]
        public double LeakBeEmissionCoefficient
        {
            get => _leakBeEmissionCoefficient;
            set
            {
                Utility.GreaterThan(value, nameof(LeakBeEmissionCoefficient), 0);
                _leakBeEmissionCoefficient = value;
            }
        }

        /// <summary>
        /// Gets or sets the ideal reverse beta parameter.
        /// </summary>
        /// <value>
        /// The ideal reverse beta parameter.
        /// </value>
        [ParameterName("br"), ParameterInfo("Ideal reverse beta")]
        [GreaterThan(0)]
        public double BetaR
        {
            get => _betaR;
            set
            {
                Utility.GreaterThan(value, nameof(BetaR), 0);
                _betaR = value;
            }
        }

        /// <summary>
        /// Gets or sets the reverse emission coefficient.
        /// </summary>
        /// <value>
        /// The reverse emission coefficient.
        /// </value>
        [ParameterName("nr"), ParameterInfo("Reverse emission coefficient")]
        [GreaterThan(0)]
        public double EmissionCoefficientReverse
        {
            get => _emissionCoefficientReverse;
            set
            {
                Utility.GreaterThan(value, nameof(EmissionCoefficientReverse), 0);
                _emissionCoefficientReverse = value;
            }
        }

        /// <summary>
        /// Gets or sets the reverse Early voltage.
        /// </summary>
        /// <value>
        /// The reverse Early voltage.
        /// </value>
        [ParameterName("var"), ParameterName("vb"), ParameterInfo("Reverse Early voltage", Units = "V")]
        [GreaterThanOrEquals(0)]
        public double EarlyVoltageReverse
        {
            get => _earlyVoltageReverse;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(EarlyVoltageReverse), 0);
                _earlyVoltageReverse = value;
            }
        }

        /// <summary>
        /// Gets or sets the reverse beta roll-off corner current.
        /// </summary>
        /// <value>
        /// The reverse beta roll-off corner current.
        /// </value>
        [ParameterName("ikr"), ParameterInfo("reverse beta roll-off corner current")]
        [GreaterThanOrEquals(0)]
        public double RollOffReverse
        {
            get => _rollOffReverse;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(RollOffReverse), 0);
                _rollOffReverse = value;
            }
        }

        /// <summary>
        /// Gets or sets the base-collector saturation current.
        /// </summary>
        /// <value>
        /// The base-collector saturation current.
        /// </value>
        [ParameterName("isc"), ParameterInfo("B-C leakage saturation current", Units = "A")]
        [GreaterThanOrEquals(0)]
        public GivenParameter<double> LeakBcCurrent
        {
            get => _leakBcCurrent;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(LeakBcCurrent), 0);
                _leakBcCurrent = value;
            }
        }

        /// <summary>
        /// Gets or sets the base-collector emission coefficient parameter.
        /// </summary>
        /// <value>
        /// The base-collector emission coefficient.
        /// </value>
        [ParameterName("nc"), ParameterInfo("B-C leakage emission coefficient")]
        [GreaterThan(0)]
        public double LeakBcEmissionCoefficient
        {
            get => _leakBcEmissionCoefficient;
            set
            {
                Utility.GreaterThan(value, nameof(LeakBcEmissionCoefficient), 0);
                _leakBcEmissionCoefficient = value;
            }
        }

        /// <summary>
        /// Gets or sets the zero-bias base resistance.
        /// </summary>
        /// <value>
        /// The zero-bias base resistance.
        /// </value>
        [ParameterName("rb"), ParameterInfo("Zero bias base resistance")]
        [GreaterThanOrEquals(0)]
        public double BaseResist
        {
            get => _baseResist;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(BaseResist), 0);
                _baseResist = value;
            }
        }

        /// <summary>
        /// Gets or sets the current for base resistance (rb + rbm) / 2.
        /// </summary>
        /// <value>
        /// The current for base resistance = (rb + rbm) / 2.
        /// </value>
        [ParameterName("irb"), ParameterInfo("Current for base resistance=(rb+rbm)/2", Units = "A")]
        [GreaterThanOrEquals(0)]
        public double BaseCurrentHalfResist
        {
            get => _baseCurrentHalfResist;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(BaseCurrentHalfResist), 0);
                _baseCurrentHalfResist = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum base resistance.
        /// </summary>
        /// <value>
        /// The minimum base resistance.
        /// </value>
        [ParameterName("rbm"), ParameterInfo("Minimum base resistance", Units = "\u03a9")]
        [GreaterThanOrEquals(0)]
        public GivenParameter<double> MinimumBaseResistance
        {
            get => _minimumBaseResistance;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(MinimumBaseResistance), 0);
                _minimumBaseResistance = value;
            }
        }

        /// <summary>
        /// Gets or sets the emitter resistance.
        /// </summary>
        /// <value>
        /// The emitter resistance.
        /// </value>
        [ParameterName("re"), ParameterInfo("Emitter resistance", Units = "\u03a9")]
        [GreaterThanOrEquals(0)]
        public double EmitterResistance
        {
            get => _emitterResistance;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(EmitterResistance), 0);
                _emitterResistance = value;
            }
        }

        /// <summary>
        /// Gets or sets the collector resistance.
        /// </summary>
        /// <value>
        /// The collector resistance.
        /// </value>
        [ParameterName("rc"), ParameterInfo("Collector resistance", Units = "\u03a9")]
        [GreaterThanOrEquals(0)]
        public double CollectorResistance
        {
            get => _collectorResistance;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(CollectorResistance), 0);
                _collectorResistance = value;
            }
        }

        /// <summary>
        /// Gets or sets the zero-bias base-emitter depletion capacitance parameter.
        /// </summary>
        /// <value>
        /// The zero-bias base-emitter depletion capacitance.
        /// </value>
        [ParameterName("cje"), ParameterInfo("Zero bias B-E depletion capacitance", Units = "F")]
        [GreaterThanOrEquals(0)]
        public double DepletionCapBe
        {
            get => _depletionCapBe;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(DepletionCapBe), 0);
                _depletionCapBe = value;
            }
        }

        /// <summary>
        /// Gets the base-emitter built-in potential.
        /// </summary>
        /// <value>
        /// Gets or sets the base-emitter built-in potential.
        /// </value>
        [ParameterName("vje"), ParameterName("pe"), ParameterInfo("B-E built in potential", Units = "V")]
        [GreaterThan(0)]
        public double PotentialBe
        {
            get => _potentialBe;
            set
            {
                Utility.GreaterThan(value, nameof(PotentialBe), 0);
                _potentialBe = value;
            }
        }

        /// <summary>
        /// Gets or sets the base-emitter junction grading coefficient.
        /// </summary>
        /// <value>
        /// The base-emitter junction grading coefficient.
        /// </value>
        [ParameterName("mje"), ParameterName("me"), ParameterInfo("B-E junction grading coefficient")]
        [GreaterThan(0)]
        public double JunctionExpBe
        {
            get => _junctionExpBe;
            set
            {
                Utility.GreaterThan(value, nameof(JunctionExpBe), 0);
                _junctionExpBe = value;
            }
        }

        /// <summary>
        /// Gets or sets the ideal forward transit time.
        /// </summary>
        /// <value>
        /// The ideal forward transit time.
        /// </value>
        [ParameterName("tf"), ParameterInfo("Ideal forward transit time", Units = "s")]
        [GreaterThanOrEquals(0)]
        public double TransitTimeForward
        {
            get => _transitTimeForward;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(TransitTimeForward), 0);
                _transitTimeForward = value;
            }
        }

        /// <summary>
        /// Gets or sets the coefficient for bias dependence parameter of the forward transit time.
        /// </summary>
        /// <value>
        /// The coefficient for bias dependence of the forward transit time.
        /// </value>
        [ParameterName("xtf"), ParameterInfo("Coefficient for bias dependence of TF")]
        [GreaterThanOrEquals(0)]
        public double TransitTimeBiasCoefficientForward
        {
            get => _transitTimeBiasCoefficientForward;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(TransitTimeBiasCoefficientForward), 0);
                _transitTimeBiasCoefficientForward = value;
            }
        }

        /// <summary>
        /// Gets or sets the voltage giving the base-collector voltage dependence of the forward transit time.
        /// </summary>
        /// <value>
        /// The voltage giving the base-collector voltage dependence of the forward transit time.
        /// </value>
        [ParameterName("vtf"), ParameterInfo("Voltage giving VBC dependence of TF")]
        [GreaterThanOrEquals(0)]
        public double TransitTimeForwardVoltageBc
        {
            get => _transitTimeForwardVoltageBc;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(TransitTimeForwardVoltageBc), 0);
                _transitTimeForwardVoltageBc = value;
            }
        }

        /// <summary>
        /// Gets the high-current dependence of the forward transit time.
        /// </summary>
        /// <value>
        /// The high-current dependence of the forward transit time.
        /// </value>
        [ParameterName("itf"), ParameterInfo("High current dependence of TF")]
        [GreaterThanOrEquals(0)]
        public double TransitTimeHighCurrentForward
        {
            get => _transitTimeHighCurrentForward;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(TransitTimeHighCurrentForward), 0);
                _transitTimeHighCurrentForward = value;
            }
        }

        /// <summary>
        /// Gets or sets the excess phase.
        /// </summary>
        /// <value>
        /// The excess phase.
        /// </value>
        [ParameterName("ptf"), ParameterInfo("Excess phase")]
        [GreaterThanOrEquals(0)]
        public double ExcessPhase
        {
            get => _excessPhase;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(ExcessPhase), 0);
                _excessPhase = value;
            }
        }

        /// <summary>
        /// Gets or sets the zero-bias base-collector depletion capacitance.
        /// </summary>
        /// <value>
        /// The zero-bias base-collector depletion capacitance.
        /// </value>
        [ParameterName("cjc"), ParameterInfo("Zero bias B-C depletion capacitance")]
        [GreaterThanOrEquals(0)]
        public double DepletionCapBc
        {
            get => _depletionCapBc;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(DepletionCapBc), 0);
                _depletionCapBc = value;
            }
        }

        /// <summary>
        /// Gets or sets the base-collector built-in potential.
        /// </summary>
        /// <value>
        /// The base-collector built-in potential.
        /// </value>
        [ParameterName("vjc"), ParameterName("pc"), ParameterInfo("B-C built in potential", Units = "V")]
        [GreaterThan(0)]
        public double PotentialBc
        {
            get => _potentialBc;
            set
            {
                Utility.GreaterThan(value, nameof(PotentialBc), 0);
                _potentialBc = value;
            }
        }

        /// <summary>
        /// Gets or sets the base-collector junction grading coefficient parameter.
        /// </summary>
        /// <value>
        /// The base-collector junction grading coefficient.
        /// </value>
        [ParameterName("mjc"), ParameterName("mc"), ParameterInfo("B-C junction grading coefficient")]
        [GreaterThan(0)]
        public double JunctionExpBc
        {
            get => _junctionExpBc;
            set
            {
                Utility.GreaterThan(value, nameof(JunctionExpBc), 0);
                _junctionExpBc = value;
            }
        }

        /// <summary>
        /// Gets or sets the fraction of base-collector capacitance to the internal base.
        /// </summary>
        /// <value>
        /// The fraction of base-collector capacitance to the internal base.
        /// </value>
        [ParameterName("xcjc"), ParameterInfo("Fraction of B-C cap to internal base")]
        [GreaterThan(0)]
        public double BaseFractionBcCap
        {
            get => _baseFractionBcCap;
            set
            {
                Utility.GreaterThan(value, nameof(BaseFractionBcCap), 0);
                _baseFractionBcCap = value;
            }
        }

        /// <summary>
        /// Gets or sets the ideal reverse transit time.
        /// </summary>
        /// <value>
        /// The ideal reverse transit time.
        /// </value>
        [ParameterName("tr"), ParameterInfo("Ideal reverse transit time", Units = "s")]
        [GreaterThanOrEquals(0)]
        public double TransitTimeReverse
        {
            get => _transitTimeReverse;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(TransitTimeReverse), 0);
                _transitTimeReverse = value;
            }
        }

        /// <summary>
        /// Gets the zero-bias collector-substrate capacitance.
        /// </summary>
        /// <value>
        /// The zero-bias collector-substrate capacitance.
        /// </value>
        [ParameterName("cjs"), ParameterName("ccs"), ParameterInfo("Zero bias C-S capacitance", Units = "F")]
        [GreaterThanOrEquals(0)]
        public double CapCs
        {
            get => _capCs;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(CapCs), 0);
                _capCs = value;
            }
        }

        /// <summary>
        /// Gets or sets the substrate junction built-in potential.
        /// </summary>
        /// <value>
        /// The substrate junction built-in potential.
        /// </value>
        [ParameterName("vjs"), ParameterName("ps"), ParameterInfo("Substrate junction built-in potential", Units = "V")]
        [GreaterThan(0)]
        public double PotentialSubstrate
        {
            get => _potentialSubstrate;
            set
            {
                Utility.GreaterThan(value, nameof(PotentialSubstrate), 0);
                _potentialSubstrate = value;
            }
        }

        /// <summary>
        /// Gets or sets the substrate junction grading coefficient.
        /// </summary>
        /// <value>
        /// The substrate junction grading coefficient.
        /// </value>
        [ParameterName("mjs"), ParameterName("ms"), ParameterInfo("Substrate junction grading coefficient")]
        [GreaterThanOrEquals(0)]
        public double ExponentialSubstrate
        {
            get => _exponentialSubstrate;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(ExponentialSubstrate), 0);
                _exponentialSubstrate = value;
            }
        }

        /// <summary>
        /// Gets or sets the forward and reverse beta temperature exponent.
        /// </summary>
        /// <value>
        /// The forward and reverse beta temperature exponent.
        /// </value>
        [ParameterName("xtb"), ParameterInfo("Forward and reverse beta temperature exponent")]
        [GreaterThanOrEquals(0)]
        public double BetaExponent
        {
            get => _betaExponent;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(BetaExponent), 0);
                _betaExponent = value;
            }
        }

        /// <summary>
        /// Gets the energy gap for saturation current temperature dependency.
        /// </summary>
        /// <value>
        /// The energy gap for saturation current temperature dependency.
        /// </value>
        [ParameterName("eg"), ParameterInfo("Energy gap for IS temperature dependency")]
        [GreaterThan(0)]
        public double EnergyGap
        {
            get => _energyGap;
            set
            {
                Utility.GreaterThan(value, nameof(EnergyGap), 0);
                _energyGap = value;
            }
        }

        /// <summary>
        /// Gets the temperature exponent for the saturation current.
        /// </summary>
        /// <value>
        /// The temperature exponent for the saturation current.
        /// </value>
        [ParameterName("xti"), ParameterInfo("Temperature exponent for IS")]
        public double TempExpIs { get; set; } = 3;

        /// <summary>
        /// Gets the forward bias junction fit parameter.
        /// </summary>
        /// <value>
        /// The forward bias junction fit parameter.
        /// </value>
        [ParameterName("fc"), ParameterInfo("Forward bias junction fit parameter")]
        [GreaterThanOrEquals(0), UpperLimit(0.9999)]
        public GivenParameter<double> DepletionCapCoefficient
        {
            get => _depletionCapCoefficient;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(DepletionCapCoefficient), 0);
                value = Utility.UpperLimit(value, this, nameof(DepletionCapCoefficient), 0.9999);
                _depletionCapCoefficient = value;
            }
        }

        /// <summary>
        /// Gets or sets a parameter that is not accessible in Spice 3f5
        /// </summary>
        /// <value>
        /// The scaling parameter c2.
        /// </value>
        [GreaterThanOrEquals(0)]
        public double C2
        {
            get => _c2;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(C2), 0);
                _c2 = value;
            }
        }

        /// <summary>
        /// Gets or sets a parameter that is not accessible in Spice 3f5
        /// </summary>
        /// <value>
        /// The scaling parameter c4.
        /// </value>
        [GreaterThanOrEquals(0)]
        public double C4
        {
            get => _c4;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(C4), 0);
                _c4 = value;
            }
        }

        /// <summary>
        /// Gets or sets the flicker noise coefficient.
        /// </summary>
        /// <value>
        /// The flicker noise coefficient.
        /// </value>
        [ParameterName("kf"), ParameterInfo("Flicker Noise Coefficient")]
        public double FlickerNoiseCoefficient { get; set; }

        /// <summary>
        /// Gets or sets the flicker noise exponent.
        /// </summary>
        /// <value>
        /// The flicker noise exponent.
        /// </value>
        [ParameterName("af"), ParameterInfo("Flicker Noise Exponent")]
        public double FlickerNoiseExponent { get; set; } = 1;
    }
}
