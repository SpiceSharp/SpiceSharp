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
        [PropertyName("area"), PropertyInfo("Area factor")]
        public Parameter Area { get; } = new Parameter(1);
        [PropertyName("off"), PropertyInfo("Initially off")]
        public bool Off { get; set; }
        [PropertyName("ic"), PropertyInfo("Initial device voltage")]
        public double InitCond { get; set; }
        [PropertyName("sens_area"), PropertyInfo("flag to request sensitivity WRT area")]
        public bool Sensitivity { get; set; }
        [PropertyName("temp"), PropertyInfo("Instance temperature")]
        public double _TEMP
        {
            get => Temperature - Circuit.CelsiusKelvin;
            set => Temperature.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter Temperature { get; } = new Parameter();
    }
}
