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
        [PropertyNameAttribute("on"), PropertyInfoAttribute("Initially closed")]
        public void SetOn() { CSWzero_state = true; }
        [PropertyNameAttribute("off"), PropertyInfoAttribute("Initially open")]
        public void SetOff() { CSWzero_state = false; }

        /// <summary>
        /// The initial state
        /// </summary>
        public bool CSWzero_state { get; set; } = false;
    }
}
