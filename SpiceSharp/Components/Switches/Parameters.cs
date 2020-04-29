using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Switches
{
    /// <summary>
    /// Base parameters for a switch.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class Parameters : ParameterSet
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
        /// <value>
        ///   <c>true</c> if the switch is initially on; otherwise, <c>false</c>.
        /// </value>
        public bool ZeroState { get; set; }
    }
}
