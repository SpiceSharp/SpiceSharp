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
