using SpiceSharp.Attributes;

namespace SpiceSharp.Components.VSW
{
    /// <summary>
    /// Base parameters for a <see cref="VoltageSwitch"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [NameAttribute("on"), InfoAttribute("Switch initially closed")]
        public void SetOn() { VSWzero_state = true; }
        [NameAttribute("off"), InfoAttribute("Switch initially open")]
        public void SetOff() { VSWzero_state = false; }

        /// <summary>
        /// Get the default state
        /// </summary>
        public bool VSWzero_state { get; set; } = false;
    }
}
