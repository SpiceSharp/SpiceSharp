using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CSW
{
    /// <summary>
    /// Base parameters for a <see cref="CurrentSwitch"/>
    /// </summary>
    public class BaseParameters : Parameters
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("on"), SpiceInfo("Initially closed")]
        public void SetOn() { CSWzero_state = true; }
        [SpiceName("off"), SpiceInfo("Initially open")]
        public void SetOff() { CSWzero_state = false; }

        /// <summary>
        /// The initial state
        /// </summary>
        public bool CSWzero_state { get; protected set; } = false;
    }
}
