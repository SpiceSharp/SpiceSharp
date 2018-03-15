using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CurrentSwitchBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="CurrentSwitchModel"/>
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("ron"), ParameterInfo("Closed resistance")]
        public Parameter OnResistance { get; } = new Parameter(1.0);
        [ParameterName("roff"), ParameterInfo("Open resistance")]
        public Parameter OffResistance { get; } = new Parameter(1.0e12);
        [ParameterName("it"), ParameterInfo("Threshold current")]
        public Parameter Threshold { get; } = new Parameter();
        [ParameterName("ih"), ParameterInfo("Hysteresis current")]
        public Parameter Hysteresis { get; } = new Parameter();
    }
}
