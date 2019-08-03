using SpiceSharp.Attributes;

namespace SpiceSharp.Components.SwitchBehaviors
{
    /// <summary>
    /// Model parameters for a <see cref="CurrentSwitchModel" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.SwitchBehaviors.ModelBaseParameters" />
    public class CurrentModelParameters : ModelBaseParameters
    {
        /// <summary>
        /// Gets the threshold current.
        /// </summary>
        [ParameterName("it"), ParameterInfo("Threshold current")]
        public override GivenParameter<double> Threshold { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the hysteresis current.
        /// </summary>
        [ParameterName("ih"), ParameterInfo("Hysteresis current")]
        public override GivenParameter<double> Hysteresis { get; } = new GivenParameter<double>();
    }
}
