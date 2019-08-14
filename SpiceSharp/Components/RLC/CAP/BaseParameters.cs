using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// Base parameters for a capacitor
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets the capacitance parameter.
        /// </summary>
        [ParameterName("capacitance"), ParameterInfo("Device capacitance", IsPrincipal = true)]
        public GivenParameter<double> Capacitance { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the initial voltage parameter.
        /// </summary>
        [ParameterName("ic"), ParameterInfo("Initial capacitor voltage", Interesting = false)]
        public GivenParameter<double> InitialCondition { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the width parameter.
        /// </summary>
        [ParameterName("w"), ParameterInfo("Device width", Interesting = false)]
        public GivenParameter<double> Width { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the length parameter.
        /// </summary>
        [ParameterName("l"), ParameterInfo("Device length", Interesting = false)]
        public GivenParameter<double> Length { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets or sets the temperature in degrees Celsius.
        /// </summary>
        [ParameterName("temp"), DerivedProperty(), ParameterInfo("Instance operating temperature", Interesting = false)]
        public double TemperatureCelsius
        {
            get => Temperature - Constants.CelsiusKelvin;
            set => Temperature.Value = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the temperature parameter (in degrees Kelvin).
        /// </summary>
        public GivenParameter<double> Temperature { get; } = new GivenParameter<double>(Constants.ReferenceTemperature);

        /// <summary>
        /// Creates a new instance of the <see cref="BaseParameters"/> class.
        /// </summary>
        public BaseParameters()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BaseParameters"/> class.
        /// </summary>
        /// <param name="cap">Capacitance</param>
        public BaseParameters(double cap)
        {
            Capacitance.Value = cap;
        }
    }
}
