using SpiceSharp.Attributes;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="Diode"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets the area parameter.
        /// </summary>
        [ParameterName("area"), ParameterInfo("Area factor")]
        public GivenParameter<double> Area { get; } = new GivenParameter<double>(1);

        /// <summary>
        /// Gets or sets whether or not the diode is initially off (non-conducting).
        /// </summary>
        [ParameterName("off"), ParameterInfo("Initially off")]
        public bool Off { get; set; }

        /// <summary>
        /// Gets or sets the initial condition.
        /// </summary>
        [ParameterName("ic"), ParameterInfo("Initial device voltage")]
        public double InitCond { get; set; }
        
        /// <summary>
        /// Gets or sets the temperature in degrees Celsius.
        /// </summary>
        [ParameterName("temp"), DerivedProperty(), ParameterInfo("Instance temperature")]
        public double TemperatureCelsius
        {
            get => Temperature - Constants.CelsiusKelvin;
            set => Temperature.Value = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the temperature parameter in degrees Kelvin.
        /// </summary>
        public GivenParameter<double> Temperature { get; } = new GivenParameter<double>(Constants.ReferenceTemperature);
    }
}
