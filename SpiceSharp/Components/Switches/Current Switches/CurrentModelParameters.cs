using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components.Switches
{
    /// <summary>
    /// Model parameters for a <see cref="CurrentSwitchModel" />.
    /// </summary>
    /// <seealso cref="ModelParameters" />
    public class CurrentModelParameters : ModelParameters
    {
        /// <summary>
        /// Gets the threshold current.
        /// </summary>
        [ParameterName("it"), ParameterInfo("Threshold current")]
        public override double Threshold { get; set; }

        /// <summary>
        /// Gets the hysteresis current.
        /// </summary>
        [ParameterName("ih"), ParameterInfo("Hysteresis current")]
        public override double Hysteresis { get; set; }
    }
}
