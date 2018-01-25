using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CSW
{
    /// <summary>
    /// Base parameters for a <see cref="CurrentSwitch"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [NameAttribute("on"), InfoAttribute("Initially closed")]
        public void SetOn() { CSWzero_state = true; }
        [NameAttribute("off"), InfoAttribute("Initially open")]
        public void SetOff() { CSWzero_state = false; }

        /// <summary>
        /// The initial state
        /// </summary>
        public bool CSWzero_state { get; set; } = false;
    }
}
