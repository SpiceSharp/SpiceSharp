using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// Parameters for the capacitor model
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("cj"), ParameterInfo("Bottom capacitance per area")]
        public GivenParameter<double> JunctionCap { get; } = new GivenParameter<double>();
        [ParameterName("cjsw"), ParameterInfo("Sidewall capacitance per meter")]
        public GivenParameter<double> JunctionCapSidewall { get; } = new GivenParameter<double>();
        [ParameterName("defw"), ParameterInfo("Default width")]
        public GivenParameter<double> DefaultWidth { get; } = new GivenParameter<double>(10.0e-6);
        [ParameterName("narrow"), ParameterInfo("Width correction factor")]
        public GivenParameter<double> Narrow { get; } = new GivenParameter<double>();
        [ParameterName("tc1"), ParameterInfo("First order temperature coefficient")]
        public GivenParameter<double> TemperatureCoefficient1 { get; } = new GivenParameter<double>();
        [ParameterName("tc2"), ParameterInfo("Second order temperature coefficient")]
        public GivenParameter<double> TemperatureCoefficient2 { get; } = new GivenParameter<double>();
        [ParameterName("tnom"), DerivedProperty(), ParameterInfo("Parameter measurement temperature", Interesting = false)]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Constants.CelsiusKelvin;
            set => NominalTemperature.Value = value + Constants.CelsiusKelvin;
        }
        public GivenParameter<double> NominalTemperature { get; } = new GivenParameter<double>(Constants.ReferenceTemperature);
    }
}
