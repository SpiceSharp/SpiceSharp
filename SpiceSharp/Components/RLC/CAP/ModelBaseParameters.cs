using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// Parameters for the capacitor model
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets the bottom junction capacitance parameter.
        /// </summary>
        [ParameterName("cj"), ParameterInfo("Bottom capacitance per area")]
        public GivenParameter<double> JunctionCap { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the junction sidewall capacitance parameter.
        /// </summary>
        [ParameterName("cjsw"), ParameterInfo("Sidewall capacitance per meter")]
        public GivenParameter<double> JunctionCapSidewall { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the default width parameter.
        /// </summary>
        [ParameterName("defw"), ParameterInfo("Default width")]
        public GivenParameter<double> DefaultWidth { get; } = new GivenParameter<double>(10.0e-6);

        /// <summary>
        /// Gets the width correction factor parameter.
        /// </summary>
        [ParameterName("narrow"), ParameterInfo("Width correction factor")]
        public GivenParameter<double> Narrow { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the first-order temperature coefficient parameter.
        /// </summary>
        [ParameterName("tc1"), ParameterInfo("First order temperature coefficient")]
        public GivenParameter<double> TemperatureCoefficient1 { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the second-order temperature coefficient parameter.
        /// </summary>
        [ParameterName("tc2"), ParameterInfo("Second order temperature coefficient")]
        public GivenParameter<double> TemperatureCoefficient2 { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets or sets the nominal temperature in degrees Celsius.
        /// </summary>
        [ParameterName("tnom"), DerivedProperty(), ParameterInfo("Parameter measurement temperature", Interesting = false)]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Constants.CelsiusKelvin;
            set => NominalTemperature.Value = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the nominal temperature parameter in degrees Kelvin.
        /// </summary>
        public GivenParameter<double> NominalTemperature { get; } = new GivenParameter<double>(Constants.ReferenceTemperature);
    }
}
