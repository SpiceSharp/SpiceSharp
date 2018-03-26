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
        public GivenParameter Resistance { get; } = new GivenParameter();
        [ParameterName("temp"), ParameterInfo("Instance operating temperature", Interesting = false)]
        public double TemperatureCelsius
        {
            get => Temperature - Circuit.CelsiusKelvin;
            set => Temperature.Value = value + Circuit.CelsiusKelvin;
        }
        public GivenParameter Temperature { get; } = new GivenParameter(300.15);
        [ParameterName("w"), ParameterInfo("Width", Interesting = false)]
        public GivenParameter Width { get; } = new GivenParameter();
        [ParameterName("l"), ParameterInfo("Length", Interesting = false)]
        public GivenParameter Length { get; } = new GivenParameter();

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
