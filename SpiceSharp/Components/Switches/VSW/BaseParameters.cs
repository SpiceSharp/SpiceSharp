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
        [PropertyName("on"), PropertyInfo("Switch initially closed")]
        public void SetOn() { ZeroState = true; }
        [PropertyName("off"), PropertyInfo("Switch initially open")]
        public void SetOff() { ZeroState = false; }

        /// <summary>
        /// Get the default state
        /// </summary>
        public bool ZeroState { get; set; } = false;
    }
}
