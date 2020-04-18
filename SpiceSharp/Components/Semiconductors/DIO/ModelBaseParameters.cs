using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="DiodeModel" />
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets the saturation current parameter.
        /// </summary>
        [ParameterName("is"), ParameterInfo("Saturation current", Units = "A")]
        [GreaterThan(0)]
        public double SaturationCurrent { get; set; } = 1e-14;

        /// <summary>
        /// Gets or sets the nominal temperature in degrees Celsius.
        /// </summary>
        [ParameterName("tnom"), DerivedProperty(), ParameterInfo("Parameter measurement temperature", Units = "\u00b0C")]
        [GreaterThan(Constants.CelsiusKelvin)]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Constants.CelsiusKelvin;
            set => NominalTemperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the nominal temperature parameter in degrees Kelvin.
        /// </summary>
        [GreaterThan(0)]
        public GivenParameter<double> NominalTemperature { get; set; } = new GivenParameter<double>(Constants.ReferenceTemperature, false);

        /// <summary>
        /// Gets the ohmic resistance parameter.
        /// </summary>
        [ParameterName("rs"), ParameterInfo("Ohmic resistance", Units = "\u03a9")]
        [GreaterThanOrEquals(0)]
        public double Resistance { get; set; }

        /// <summary>
        /// Gets the mission coefficient parameter.
        /// </summary>
        [ParameterName("n"), ParameterInfo("Emission Coefficient")]
        [GreaterThan(0)]
        public double EmissionCoefficient { get; set; } = 1;

        /// <summary>
        /// Gets the transit time parameter.
        /// </summary>
        [ParameterName("tt"), ParameterInfo("Transit Time", Units = "s")]
        [GreaterThanOrEquals(0)]
        public double TransitTime { get; set; }

        /// <summary>
        /// Gets the junction capacitance parameter.
        /// </summary>
        [ParameterName("cjo"), ParameterName("cj0"), ParameterInfo("Junction capacitance", Units = "F")]
        [GreaterThanOrEquals(0)]
        public double JunctionCap { get; set; }

        /// <summary>
        /// Gets the junction built-in potential parameter.
        /// </summary>
        [ParameterName("vj"), ParameterInfo("Junction potential", Units = "V")]
        [GreaterThan(0)]
        public double JunctionPotential { get; set; } = 1;

        /// <summary>
        /// Gets the grading coefficient parameter.
        /// </summary>
        [ParameterName("m"), ParameterInfo("Grading coefficient")]
        [GreaterThan(0)]
        public double GradingCoefficient { get; set; } = 0.5;

        /// <summary>
        /// Gets the activation energy parameter.
        /// </summary>
        [ParameterName("eg"), ParameterInfo("Activation energy", Units = "eV")]
        [GreaterThan(0)]
        public double ActivationEnergy { get; set; } = 1.11;

        /// <summary>
        /// Gets the saturation current temperature exponent parameter.
        /// </summary>
        [ParameterName("xti"), ParameterInfo("Saturation current temperature exponent")]
        [GreaterThan(0)]
        public double SaturationCurrentExp { get; set; } = 3;

        /// <summary>
        /// Gets the forward bias junction fit parameter.
        /// </summary>
        [ParameterName("fc"), ParameterInfo("Forward bias junction fit parameter")]
        [GreaterThan(0)]
        public double DepletionCapCoefficient { get; set; } = 0.5;

        /// <summary>
        /// Gets or sets the reverse breakdown voltage parameter. When NaN, the breakdown voltage is ignored.
        /// </summary>
        /// <value>
        /// The breakdown voltage.
        /// </value>
        [ParameterName("bv"), ParameterInfo("Reverse breakdown voltage", Units = "V")]
        [LessThan(0)]
        public GivenParameter<double> BreakdownVoltage { get; set; }

        /// <summary>
        /// Gets the current parameter at the reverse breakdown voltage.
        /// </summary>
        /// <value>
        /// The breakdown current.
        /// </value>
        [ParameterName("ibv"), ParameterInfo("Current at reverse breakdown voltage", Units = "A")]
        [GreaterThan(0)]
        public double BreakdownCurrent { get; set; } = 1e-3;
    }
}
