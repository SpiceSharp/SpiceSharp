using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Capacitors
{
    /// <summary>
    /// Parameters for a <see cref="CapacitorModel"/>.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public partial class ModelParameters : ParameterSet<ModelParameters>
    {
        /// <summary>
        /// Gets the bottom junction capacitance parameter.
        /// </summary>
        [ParameterName("cj"), ParameterInfo("Bottom capacitance per area", Units = "F/m^2")]
        [GreaterThanOrEquals(0)]
        private double _junctionCap;

        /// <summary>
        /// Gets the junction sidewall capacitance parameter.
        /// </summary>
        [ParameterName("cjsw"), ParameterInfo("Sidewall capacitance per meter", Units = "F/m")]
        [GreaterThanOrEquals(0)]
        private double _junctionCapSidewall;

        /// <summary>
        /// Gets or sets the default width parameter.
        /// </summary>
        /// <value>
        /// The default width.
        /// </value>
        [ParameterName("defw"), ParameterInfo("Default width", Units = "m")]
        [GreaterThanOrEquals(0)]
        private double _defaultWidth = 10e-6;

        /// <summary>
        /// Gets the width correction factor parameter.
        /// </summary>
        [ParameterName("narrow"), ParameterInfo("Width correction factor", Units = "m")]
        public double Narrow { get; set; }

        /// <summary>
        /// Gets the first-order temperature coefficient parameter.
        /// </summary>
        [ParameterName("tc1"), ParameterInfo("First order temperature coefficient", Units = "F/K")]
        public double TemperatureCoefficient1 { get; set; }

        /// <summary>
        /// Gets the second-order temperature coefficient parameter.
        /// </summary>
        [ParameterName("tc2"), ParameterInfo("Second order temperature coefficient", Units = "F/K^2")]
        public double TemperatureCoefficient2 { get; set; }

        /// <summary>
        /// Gets or sets the nominal temperature in degrees Celsius.
        /// </summary>
        [ParameterName("tnom"), ParameterInfo("Parameter measurement temperature", Units = "\u00b0C", Interesting = false)]
        [DerivedProperty, GreaterThan(-Constants.CelsiusKelvin)]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Constants.CelsiusKelvin;
            set => NominalTemperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the nominal temperature parameter in degrees Kelvin.
        /// </summary>
        [GreaterThan(0)]
        private GivenParameter<double> _nominalTemperature = new(Constants.ReferenceTemperature, false);
    }
}
