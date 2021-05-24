using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Switches
{
    /// <summary>
    /// Base parameters for a switch.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public partial class Parameters : ParameterSet<Parameters>
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
        /// Sets the switch initially to non-conducting.
        /// </summary>
        /// <param name="flag">If <c>true</c>, the switch will be initially off.</param>
        [ParameterName("off"), ParameterInfo("Initially open")]
        public void SetZeroStateOff(bool flag)
        {
            if (flag)
                ZeroState = false;
        }

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        [ParameterName("m"), ParameterInfo("The parallel multiplier")]
        [GreaterThan(0.0), Finite]
        private double _parallelMultiplier = 1.0;

        /// <summary>
        /// Gets or sets initial state.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the switch is initially on; otherwise, <c>false</c>.
        /// </value>
        public bool ZeroState { get; set; }
    }
}
