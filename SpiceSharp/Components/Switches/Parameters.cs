using SpiceSharp.ParameterSets;

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
        /// <param name="flag">If <c>true</c>, the switch will be initially on.</param>
        [ParameterName("on"), ParameterInfo("Initially closed")]
        public void SetZeroStateOn(bool flag)
        {
            if (flag)
                ZeroState = true;
        }

        /// <summary>
        /// Set the switch initially to non-conducting.
        /// </summary>
        /// <param name="flag">If <c>true</c>, the switch will be initially off.</param>
        [ParameterName("off"), ParameterInfo("Initially open")]
        public void SetZeroStateOff(bool flag)
        {
            if (flag)
                ZeroState = false;
        }

        /// <summary>
        /// Gets or sets initial state.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the switch is initially on; otherwise, <c>false</c>.
        /// </value>
        public bool ZeroState { get; set; }
    }
}
