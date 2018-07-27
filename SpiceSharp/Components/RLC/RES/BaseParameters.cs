using SpiceSharp.Attributes;

namespace SpiceSharp.Components.ResistorBehaviors
{
    /// <summary>
    /// Base set of parameters for a <see cref="Resistor"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("resistance"), ParameterInfo("Resistance", IsPrincipal = true)]
        public GivenParameter<double> Resistance { get; } = new GivenParameter<double>();
        [ParameterName("temp"), DerivedProperty(), ParameterInfo("Instance operating temperature", Interesting = false)]
        public double TemperatureCelsius
        {
            get => Temperature - Circuit.CelsiusKelvin;
            set => Temperature.Value = value + Circuit.CelsiusKelvin;
        }
        public GivenParameter<double> Temperature { get; } = new GivenParameter<double>(Circuit.ReferenceTemperature);
        [ParameterName("w"), ParameterInfo("Width", Interesting = false)]
        public GivenParameter<double> Width { get; } = new GivenParameter<double>();
        [ParameterName("l"), ParameterInfo("Length", Interesting = false)]
        public GivenParameter<double> Length { get; } = new GivenParameter<double>();

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseParameters()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="res">Resistor</param>
        public BaseParameters(double res)
        {
            Resistance.Value = res;
        }
    }
}
