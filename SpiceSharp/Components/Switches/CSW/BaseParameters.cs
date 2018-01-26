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
        [PropertyName("on"), PropertyInfo("Initially closed")]
        public void SetOn() { CSWzero_state = true; }
        [PropertyName("off"), PropertyInfo("Initially open")]
        public void SetOff() { CSWzero_state = false; }

        /// <summary>
        /// The initial state
        /// </summary>
        public bool CSWzero_state { get; set; } = false;
    }
}
