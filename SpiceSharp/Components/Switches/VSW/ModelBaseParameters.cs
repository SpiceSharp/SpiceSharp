using SpiceSharp.Attributes;

namespace SpiceSharp.Components.VoltageSwitchBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="VoltageSwitchModel"/>
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("ron"), PropertyInfo("Resistance when closed")]
        public Parameter OnResistance { get; } = new Parameter(1.0);
        [ParameterName("roff"), PropertyInfo("Resistance when off")]
        public Parameter OffResistance { get; } = new Parameter(1.0e12);
        [ParameterName("vt"), PropertyInfo("Threshold voltage")]
        public Parameter Threshold { get; } = new Parameter();
        [ParameterName("vh"), PropertyInfo("Hysteresis voltage")]
        public Parameter Hysteresis { get; } = new Parameter();
    }
}
