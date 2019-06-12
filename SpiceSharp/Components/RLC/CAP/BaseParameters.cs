using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// Base parameters for a capacitor
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("capacitance"), ParameterInfo("Device capacitance", IsPrincipal = true)]
        public GivenParameter<double> Capacitance { get; } = new GivenParameter<double>();
        [ParameterName("ic"), ParameterInfo("Initial capacitor voltage", Interesting = false)]
        public GivenParameter<double> InitialCondition { get; } = new GivenParameter<double>();
        [ParameterName("w"), ParameterInfo("Device width", Interesting = false)]
        public GivenParameter<double> Width { get; } = new GivenParameter<double>();
        [ParameterName("l"), ParameterInfo("Device length", Interesting = false)]
        public GivenParameter<double> Length { get; } = new GivenParameter<double>();

        [ParameterName("temp"), DerivedProperty(), ParameterInfo("Instance operating temperature", Interesting = false)]
        public double TemperatureCelsius
        {
            get => Temperature - Constants.CelsiusKelvin;
            set => Temperature.Value = value + Constants.CelsiusKelvin;
        }
        public GivenParameter<double> Temperature { get; } = new GivenParameter<double>(Constants.ReferenceTemperature);

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseParameters()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cap">Capacitance</param>
        public BaseParameters(double cap)
        {
            Capacitance.Value = cap;
        }
    }
}
