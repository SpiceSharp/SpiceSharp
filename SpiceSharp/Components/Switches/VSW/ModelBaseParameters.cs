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
        [ParameterName("ron"), ParameterInfo("Resistance when closed")]
        public Parameter OnResistance { get; } = new Parameter(1.0);
        [ParameterName("roff"), ParameterInfo("Resistance when off")]
        public Parameter OffResistance { get; } = new Parameter(1.0e12);
        [ParameterName("vt"), ParameterInfo("Threshold voltage")]
        public Parameter Threshold { get; } = new Parameter();
        [ParameterName("vh"), ParameterInfo("Hysteresis voltage")]
        public Parameter Hysteresis { get; } = new Parameter();
    }
}
