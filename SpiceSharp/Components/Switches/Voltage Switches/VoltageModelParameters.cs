using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components.Switches
{
    /// <summary>
    /// Model parameters for a <see cref="VoltageSwitchModel" />.
    /// </summary>
    /// <seealso cref="ModelParameters" />
    public class VoltageModelParameters : ModelParameters
    {
        /// <summary>
        /// Gets the threshold current.
        /// </summary>
        [ParameterName("vt"), ParameterInfo("Threshold current")]
        public override double Threshold { get; set; }

        /// <summary>
        /// Gets the hysteresis current.
        /// </summary>
        [ParameterName("vh"), ParameterInfo("Hysteresis current")]
        public override double Hysteresis { get; set; }
    }
}
