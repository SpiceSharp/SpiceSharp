using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components.Capacitors
{
    /// <summary>
    /// Parameters for a <see cref="CapacitorModel"/>.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public class ModelParameters : ParameterSet
    {
        private GivenParameter<double> _nominalTemperature = new GivenParameter<double>(Constants.ReferenceTemperature, false);
        private double _defaultWidth = 10e-6;
        private double _junctionCapSidewall;
        private double _junctionCap;

        /// <summary>
        /// Gets the bottom junction capacitance parameter.
        /// </summary>
        [ParameterName("cj"), ParameterInfo("Bottom capacitance per area", Units = "F/m^2")]
        [GreaterThanOrEquals(0)]
        public double JunctionCap
        {
            get => _junctionCap;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(JunctionCap), 0);
                _junctionCap = value;
            }
        }

        /// <summary>
        /// Gets the junction sidewall capacitance parameter.
        /// </summary>
        [ParameterName("cjsw"), ParameterInfo("Sidewall capacitance per meter", Units = "F/m")]
        [GreaterThanOrEquals(0)]
        public double JunctionCapSidewall
        {
            get => _junctionCapSidewall;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(JunctionCapSidewall), 0);
                _junctionCapSidewall = value;
            }
        }

        /// <summary>
        /// Gets the default width parameter.
        /// </summary>
        [ParameterName("defw"), ParameterInfo("Default width", Units = "m")]
        [GreaterThanOrEquals(0)]
        public double DefaultWidth
        {
            get => _defaultWidth;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(DefaultWidth), 0);
                _defaultWidth = value;
            }
        }

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
        [ParameterName("tnom"), DerivedProperty(), ParameterInfo("Parameter measurement temperature", Units = "\u00b0C", Interesting = false)]
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
        public GivenParameter<double> NominalTemperature
        {
            get => _nominalTemperature;
            set
            {
                Utility.GreaterThan(value, nameof(NominalTemperature), 0);
                _nominalTemperature = value;
            }
        }
    }
}
