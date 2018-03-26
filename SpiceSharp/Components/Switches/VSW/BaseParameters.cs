using SpiceSharp.Attributes;

namespace SpiceSharp.Components.VoltageSwitchBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="VoltageSwitch"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("on"), ParameterInfo("Switch initially closed")]
        public void SetZeroStateOn() { ZeroState = true; }
        [ParameterName("off"), ParameterInfo("Switch initially open")]
        public void SetZeroStateOff() { ZeroState = false; }

        /// <summary>
        /// Gets the default state
        /// </summary>
        public bool ZeroState { get; set; }
    }
}
