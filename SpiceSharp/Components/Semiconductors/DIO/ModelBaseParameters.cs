using SpiceSharp.Attributes;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="DiodeModel"/>
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets the saturation current parameter.
        /// </summary>
        [ParameterName("is"), ParameterInfo("Saturation current")]
        public double SaturationCurrent { get; set; } = 1e-14;

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
        /// Gets the ohmic resistance parameter.
        /// </summary>
        [ParameterName("rs"), ParameterInfo("Ohmic resistance")]
        public double Resistance { get; set; }

        /// <summary>
        /// Gets the mission coefficient parameter.
        /// </summary>
        [ParameterName("n"), ParameterInfo("Emission Coefficient")]
        public double EmissionCoefficient { get; set; } = 1;

        /// <summary>
        /// Gets the transit time parameter.
        /// </summary>
        [ParameterName("tt"), ParameterInfo("Transit Time")]
        public double TransitTime { get; set; }

        /// <summary>
        /// Gets the junction capacitance parameter.
        /// </summary>
        [ParameterName("cjo"), ParameterName("cj0"), ParameterInfo("Junction capacitance")]
        public double JunctionCap { get; set; }

        /// <summary>
        /// Gets the junction built-in potential parameter.
        /// </summary>
        [ParameterName("vj"), ParameterInfo("Junction potential")]
        public double JunctionPotential { get; set; } = 1;

        /// <summary>
        /// Gets the grading coefficient parameter.
        /// </summary>
        [ParameterName("m"), ParameterInfo("Grading coefficient")]
        public double GradingCoefficient { get; set; } = 0.5;

        /// <summary>
        /// Gets the activation energy parameter.
        /// </summary>
        [ParameterName("eg"), ParameterInfo("Activation energy")]
        public double ActivationEnergy { get; set; } = 1.11;

        /// <summary>
        /// Gets the saturation current temperature exponent parameter.
        /// </summary>
        [ParameterName("xti"), ParameterInfo("Saturation current temperature exponent")]
        public double SaturationCurrentExp { get; set; } = 3;

        /// <summary>
        /// Gets the forward bias junction fit parameter.
        /// </summary>
        [ParameterName("fc"), ParameterInfo("Forward bias junction fit parameter")]
        public double DepletionCapCoefficient { get; set; } = 0.5;

        /// <summary>
        /// Gets the reverse breakdown voltage parameter.
        /// </summary>
        [ParameterName("bv"), ParameterInfo("Reverse breakdown voltage")]
        public GivenParameter<double> BreakdownVoltage { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the current parameter at the reverse breakdown voltage.
        /// </summary>
        [ParameterName("ibv"), ParameterInfo("Current at reverse breakdown voltage")]
        public double BreakdownCurrent { get; set; } = 1e-3;
    }
}
