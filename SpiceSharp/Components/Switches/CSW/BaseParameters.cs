using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CurrentSwitchBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="CurrentSwitch"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("on"), ParameterInfo("Initially closed")]
        public void SetZeroStateOn() { ZeroState = true; }
        [ParameterName("off"), ParameterInfo("Initially open")]
        public void SetZeroStateOff() { ZeroState = false; }

        /// <summary>
        /// The initial state
        /// </summary>
        public bool ZeroState { get; set; }
    }
}
