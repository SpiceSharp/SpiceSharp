using SpiceSharp.Attributes;

namespace SpiceSharp.Components.SwitchBehaviors
{
    /// <summary>
    /// Model parameters for a <see cref="CurrentSwitchModel" />.
    /// </summary>
    /// <seealso cref="ModelBaseParameters" />
    public class CurrentModelParameters : ModelBaseParameters
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
