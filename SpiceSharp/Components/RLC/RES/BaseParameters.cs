using SpiceSharp.Attributes;

namespace SpiceSharp.Components.ResistorBehaviors
{
    /// <summary>
    /// Base set of parameters for a <see cref="Resistor"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets the resistance parameter.
        /// </summary>
        [ParameterName("resistance"), ParameterInfo("Resistance", IsPrincipal = true)]
        public GivenParameter<double> Resistance { get; } = new GivenParameter<double>();

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
        /// Gets the temperature parameter in degrees Kelvin.
        /// </summary>
        public GivenParameter<double> Temperature { get; } = new GivenParameter<double>(Constants.ReferenceTemperature);

        /// <summary>
        /// Gets the width parameter of the resistor.
        /// </summary>
        [ParameterName("w"), ParameterInfo("Width", Interesting = false)]
        public GivenParameter<double> Width { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the length parameter of the resistor.
        /// </summary>
        [ParameterName("l"), ParameterInfo("Length", Interesting = false)]
        public GivenParameter<double> Length { get; } = new GivenParameter<double>();

        /// <summary>
        /// Creates a new instance of the <see cref="BaseParameters"/> class.
        /// </summary>
        public BaseParameters()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BaseParameters"/> class.
        /// </summary>
        /// <param name="res">Resistor</param>
        public BaseParameters(double res)
        {
            Resistance.Value = res;
        }
    }
}
