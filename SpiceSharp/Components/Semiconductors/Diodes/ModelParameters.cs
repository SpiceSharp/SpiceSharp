using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Diodes
{
    /// <summary>
    /// Base parameters for a <see cref="DiodeModel" />
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public partial class ModelParameters : ParameterSet<ModelParameters>
    {
        /// <summary>
        /// Gets or sets the saturation current.
        /// </summary>
        /// <value>
        /// The saturation current.
        /// </value>
        [ParameterName("is"), ParameterInfo("Saturation current", Units = "A")]
        [GreaterThan(0), Finite]
        private double _saturationCurrent = 1e-14;

        /// <summary>
        /// Gets or sets the nominal temperature in degrees Celsius.
        /// </summary>
        /// <value>
        /// The nominal temperature in degrees Celsius.
        /// </value>
        [ParameterName("tnom"), ParameterInfo("Parameter measurement temperature", Units = "\u00b0C")]
        [DerivedProperty, GreaterThan(-Constants.CelsiusKelvin), Finite]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Constants.CelsiusKelvin;
            set => NominalTemperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the nominal temperature in degrees Kelvin.
        /// </summary>
        /// <value>
        /// The nominal temperature in degrees Kelvin.
        /// </value>
        [GreaterThan(0), Finite]
        private GivenParameter<double> _nominalTemperature = new(Constants.ReferenceTemperature, false);

        /// <summary>
        /// Gets or sets the ohmic resistance.
        /// </summary>
        /// <value>
        /// The ohmic resistance.
        /// </value>
        [ParameterName("rs"), ParameterInfo("Ohmic resistance", Units = "\u03a9")]
        [GreaterThanOrEquals(0), Finite]
        private double _resistance;

        /// <summary>
        /// Gets or sets the emission coefficient.
        /// </summary>
        /// <value>
        /// The emission coefficient.
        /// </value>
        [ParameterName("n"), ParameterInfo("Emission Coefficient")]
        [GreaterThan(0), Finite]
        private double _emissionCoefficient = 1;

        /// <summary>
        /// Gets or sets the transit time.
        /// </summary>
        /// <value>
        /// The transit time.
        /// </value>
        [ParameterName("tt"), ParameterInfo("Transit Time", Units = "s")]
        [GreaterThanOrEquals(0), Finite]
        private double _transitTime;

        /// <summary>
        /// Gets or sets the junction capacitance.
        /// </summary>
        /// <value>
        /// The junction capacitance.
        /// </value>
        [ParameterName("cjo"), ParameterName("cj0"), ParameterInfo("Junction capacitance", Units = "F")]
        [GreaterThanOrEquals(0), Finite]
        private double _junctionCap;

        /// <summary>
        /// Gets or sets the junction built-in potential.
        /// </summary>
        /// <value>
        /// The junction built-in potential.
        /// </value>
        [ParameterName("vj"), ParameterInfo("Junction potential", Units = "V")]
        [GreaterThan(0), Finite]
        private double _junctionPotential = 1;

        /// <summary>
        /// Gets or sets the grading coefficient.
        /// </summary>
        /// <value>
        /// The grading coefficient.
        /// </value>
        [ParameterName("m"), ParameterInfo("Grading coefficient")]
        [GreaterThan(0), UpperLimit(0.9), Finite]
        private double _gradingCoefficient = 0.5;

        /// <summary>
        /// Gets or sets the activation energy.
        /// </summary>
        /// <value>
        /// The activation energy.
        /// </value>
        [ParameterName("eg"), ParameterInfo("Activation energy", Units = "eV")]
        [GreaterThan(0), LowerLimit(0.1), Finite]
        private double _activationEnergy = 1.11;

        /// <summary>
        /// Gets the saturation current temperature exponent.
        /// </summary>
        /// <value>
        /// The saturation current temperature exponent.
        /// </value>
        [ParameterName("xti"), ParameterInfo("Saturation current temperature exponent")]
        [GreaterThanOrEquals(0), Finite]
        private double _saturationCurrentExp = 3;

        /// <summary>
        /// Gets the forward bias junction fit parameter.
        /// </summary>
        /// <value>
        /// The forward bias junction fit parameter.
        /// </value>
        [ParameterName("fc"), ParameterInfo("Forward bias junction fit parameter")]
        [GreaterThan(0), UpperLimit(0.95), Finite]
        private double _depletionCapCoefficient = 0.5;

        /// <summary>
        /// Gets or sets the reverse breakdown voltage.
        /// </summary>
        /// <value>
        /// The breakdown voltage.
        /// </value>
        [ParameterName("bv"), ParameterInfo("Reverse breakdown voltage", Units = "V")]
        [Finite]
        private GivenParameter<double> _breakdownVoltage = new(-1.0, false);

        /// <summary>
        /// Gets the current at the reverse breakdown voltage.
        /// </summary>
        /// <value>
        /// The breakdown current.
        /// </value>
        [ParameterName("ibv"), ParameterInfo("Current at reverse breakdown voltage", Units = "A")]
        [Finite]
        private double _breakdownCurrent = 1e-3;

        /// <summary>
        /// Gets or sets the flicker noise coefficient.
        /// </summary>
        /// <value>
        /// The flicker noise coefficient.
        /// </value>
        [ParameterName("kf"), ParameterInfo("flicker noise coefficient")]
        [Finite]
        private double _flickerNoiseCoefficient;

        /// <summary>
        /// Gets or sets the flicker noise exponent.
        /// </summary>
        /// <value>
        /// The flicker noise exponent.
        /// </value>
        [ParameterName("af"), ParameterInfo("flicker noise exponent")]
        [Finite]
        private double _flickerNoiseExponent = 1;

        /// <summary>
        /// Gets or sets the LTspice ideal diode forward resistance.
        /// </summary>
        /// <remarks>
        /// When any ideal-diode parameter is given, the diode uses a piecewise-linear
        /// LTspice-compatible approximation instead of the Berkeley exponential model.
        /// </remarks>
        [ParameterName("ron"), ParameterInfo("Ideal diode forward resistance", Units = "Ohm")]
        [GreaterThan(0), Finite]
        private GivenParameter<double> _idealOnResistance = new(1.0, false);

        /// <summary>
        /// Gets or sets the LTspice ideal diode off resistance.
        /// </summary>
        /// <remarks>
        /// If omitted for an ideal diode, the off conductance follows the simulation Gmin.
        /// </remarks>
        [ParameterName("roff"), ParameterInfo("Ideal diode off resistance", Units = "Ohm")]
        [GreaterThan(0), Finite]
        private GivenParameter<double> _idealOffResistance = new(0.0, false);

        /// <summary>
        /// Gets or sets the LTspice ideal diode forward voltage.
        /// </summary>
        [ParameterName("vfwd"), ParameterInfo("Ideal diode forward voltage", Units = "V")]
        [Finite]
        private GivenParameter<double> _idealForwardVoltage = new(0.0, false);

        /// <summary>
        /// Gets or sets the LTspice ideal diode reverse breakdown voltage magnitude.
        /// </summary>
        [ParameterName("vrev"), ParameterInfo("Ideal diode reverse breakdown voltage", Units = "V")]
        [GreaterThanOrEquals(0), Finite]
        private GivenParameter<double> _idealReverseVoltage = new(0.0, false);

        /// <summary>
        /// Gets or sets the LTspice ideal diode reverse breakdown resistance.
        /// </summary>
        /// <remarks>
        /// If omitted, reverse breakdown uses <see cref="IdealOnResistance"/>.
        /// </remarks>
        [ParameterName("rrev"), ParameterInfo("Ideal diode reverse breakdown resistance", Units = "Ohm")]
        [GreaterThan(0), Finite]
        private GivenParameter<double> _idealReverseResistance = new(0.0, false);

        /// <summary>
        /// Gets or sets the LTspice ideal diode forward current limit.
        /// </summary>
        [ParameterName("ilimit"), ParameterInfo("Ideal diode forward current limit", Units = "A")]
        [GreaterThan(0), Finite]
        private GivenParameter<double> _idealForwardCurrentLimit = new(0.0, false);

        /// <summary>
        /// Gets or sets the LTspice ideal diode reverse current limit.
        /// </summary>
        [ParameterName("revilimit"), ParameterInfo("Ideal diode reverse current limit", Units = "A")]
        [GreaterThan(0), Finite]
        private GivenParameter<double> _idealReverseCurrentLimit = new(0.0, false);

        /// <summary>
        /// Gets or sets the LTspice ideal diode forward transition width.
        /// </summary>
        [ParameterName("epsilon"), ParameterInfo("Ideal diode forward smoothing voltage", Units = "V")]
        [GreaterThanOrEquals(0), Finite]
        private GivenParameter<double> _idealForwardEpsilon = new(0.0, false);

        /// <summary>
        /// Gets or sets the LTspice ideal diode reverse transition width.
        /// </summary>
        [ParameterName("revepsilon"), ParameterInfo("Ideal diode reverse smoothing voltage", Units = "V")]
        [GreaterThanOrEquals(0), Finite]
        private GivenParameter<double> _idealReverseEpsilon = new(0.0, false);

        /// <summary>
        /// Gets whether this model uses the LTspice idealized diode branch.
        /// </summary>
        public bool IsIdeal => _idealOnResistance.Given
            || _idealOffResistance.Given
            || _idealForwardVoltage.Given
            || _idealReverseVoltage.Given
            || _idealReverseResistance.Given
            || _idealForwardCurrentLimit.Given
            || _idealReverseCurrentLimit.Given
            || _idealForwardEpsilon.Given
            || _idealReverseEpsilon.Given;
    }
}