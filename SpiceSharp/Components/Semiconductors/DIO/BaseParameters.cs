using SpiceSharp.Attributes;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="Diode"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("area"), ParameterInfo("Area factor")]
        public GivenParameter Area { get; } = new GivenParameter(1);
        [ParameterName("off"), ParameterInfo("Initially off")]
        public bool Off { get; set; }
        [ParameterName("ic"), ParameterInfo("Initial device voltage")]
        public double InitCond { get; set; }
        [ParameterName("sens_area"), ParameterInfo("flag to request sensitivity WRT area")]
        public bool Sensitivity { get; set; }
        [ParameterName("temp"), ComputedProperty(), ParameterInfo("Instance temperature")]
        public double TemperatureCelsius
        {
            get => Temperature - Circuit.CelsiusKelvin;
            set => Temperature.Value = value + Circuit.CelsiusKelvin;
        }
        public GivenParameter Temperature { get; } = new GivenParameter(Circuit.ReferenceTemperature);
    }
}
