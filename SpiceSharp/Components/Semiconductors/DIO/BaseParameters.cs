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
        [ParameterName("area"), PropertyInfo("Area factor")]
        public Parameter Area { get; } = new Parameter(1);
        [ParameterName("off"), PropertyInfo("Initially off")]
        public bool Off { get; set; }
        [ParameterName("ic"), PropertyInfo("Initial device voltage")]
        public double InitCond { get; set; }
        [ParameterName("sens_area"), PropertyInfo("flag to request sensitivity WRT area")]
        public bool Sensitivity { get; set; }
        [ParameterName("temp"), PropertyInfo("Instance temperature")]
        public double TemperatureCelsius
        {
            get => Temperature - Circuit.CelsiusKelvin;
            set => Temperature.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter Temperature { get; } = new Parameter();
    }
}
