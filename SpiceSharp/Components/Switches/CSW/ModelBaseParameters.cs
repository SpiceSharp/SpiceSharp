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
        [PropertyName("ron"), PropertyInfo("Closed resistance")]
        public Parameter OnResistance { get; } = new Parameter(1.0);
        [PropertyName("roff"), PropertyInfo("Open resistance")]
        public Parameter OffResistance { get; } = new Parameter(1.0e12);
        [PropertyName("it"), PropertyInfo("Threshold current")]
        public Parameter Threshold { get; } = new Parameter();
        [PropertyName("ih"), PropertyInfo("Hysteresis current")]
        public Parameter Hysteresis { get; } = new Parameter();
    }
}
