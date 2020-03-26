using SpiceSharp.Attributes;
using SpiceSharp.Simulations;

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
        [ParameterName("capacitance"), ParameterInfo("Device capacitance", Units = "F", IsPrincipal = true)]
        public GivenParameter<double> Capacitance { get; set; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the initial voltage parameter.
        /// </summary>
        [ParameterName("ic"), ParameterInfo("Initial capacitor voltage", Units = "V", Interesting = false)]
        public double InitialCondition { get; set; }

        /// <summary>
        /// Gets the width parameter.
        /// </summary>
        [ParameterName("w"), ParameterInfo("Device width", Units = "m", Interesting = false)]
        public GivenParameter<double> Width { get; set; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the length parameter.
        /// </summary>
        [ParameterName("l"), ParameterInfo("Device length", Units = "m", Interesting = false)]
        public GivenParameter<double> Length { get; set; } = new GivenParameter<double>();

        /// <summary>
        /// Gets or sets the parallel multiplier.
        /// </summary>
        [ParameterName("m"), ParameterInfo("Parallel multiplier")]
        public double ParallelMultiplier { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the temperature in degrees Celsius.
        /// </summary>
        [ParameterName("temp"), DerivedProperty(), ParameterInfo("Instance operating temperature", Units = "\u00b0C", Interesting = false)]
        public double TemperatureCelsius
        {
            get => Temperature - Constants.CelsiusKelvin;
            set => Temperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the temperature parameter (in degrees Kelvin).
        /// </summary>
        public GivenParameter<double> Temperature { get; set; } = new GivenParameter<double>(Constants.ReferenceTemperature, false);

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseParameters"/> class.
        /// </summary>
        public BaseParameters()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseParameters"/> class.
        /// </summary>
        /// <param name="cap">Capacitance</param>
        public BaseParameters(double cap)
        {
            Capacitance = cap;
        }
    }
}
