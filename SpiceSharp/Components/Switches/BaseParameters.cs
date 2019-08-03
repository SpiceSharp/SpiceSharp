using SpiceSharp.Attributes;

namespace SpiceSharp.Components.SwitchBehaviors
{
    /// <summary>
    /// Base parameters for a switch.
    /// </summary>
    /// <seealso cref="SpiceSharp.ParameterSet" />
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Sets the switch initially to conducting.
        /// </summary>
        [ParameterName("on"), ParameterInfo("Initially closed")]
        public void SetZeroStateOn() { ZeroState = true; }

        /// <summary>
        /// Set the switch initially to non-conducting.
        /// </summary>
        [ParameterName("off"), ParameterInfo("Initially open")]
        public void SetZeroStateOff() { ZeroState = false; }

        /// <summary>
        /// Gets or sets initial state.
        /// </summary>
        public bool ZeroState { get; set; }
    }
}
