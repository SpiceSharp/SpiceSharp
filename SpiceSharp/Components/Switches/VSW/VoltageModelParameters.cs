using SpiceSharp.Attributes;

namespace SpiceSharp.Components.SwitchBehaviors
{
    /// <summary>
    /// Model parameters for a <see cref="VoltageSwitchModel" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.SwitchBehaviors.ModelBaseParameters" />
    public class VoltageModelParameters : ModelBaseParameters
    {
        /// <summary>
        /// Gets the threshold current.
        /// </summary>
        [ParameterName("vt"), ParameterInfo("Threshold current")]
        public override GivenParameter<double> Threshold { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the hysteresis current.
        /// </summary>
        [ParameterName("vh"), ParameterInfo("Hysteresis current")]
        public override GivenParameter<double> Hysteresis { get; } = new GivenParameter<double>();
    }
}
