using SpiceSharp.Attributes;

namespace SpiceSharp.Components.VSW
{
    /// <summary>
    /// Base parameters for a <see cref="VoltageSwitch"/>
    /// </summary>
    public class BaseParameters : Parameters
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("on"), SpiceInfo("Switch initially closed")]
        public void SetOn() { VSWzero_state = true; }
        [SpiceName("off"), SpiceInfo("Switch initially open")]
        public void SetOff() { VSWzero_state = false; }

        /// <summary>
        /// Get the default state
        /// </summary>
        public bool VSWzero_state { get; set; } = false;
    }
}
