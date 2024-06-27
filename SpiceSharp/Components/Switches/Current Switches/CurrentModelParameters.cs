using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Switches
{
    /// <summary>
    /// Model parameters for a <see cref="CurrentSwitchModel" />.
    /// </summary>
    /// <seealso cref="ModelParameters" />
    [GeneratedParameters]
    public partial class CurrentModelParameters : ModelParameters, ICloneable<CurrentModelParameters>
    {
        private double _threshold, _hysteresis;

        /// <summary>
        /// Gets the threshold current.
        /// </summary>
        [ParameterName("it"), ParameterInfo("Threshold current")]
        [Finite]
        public override double Threshold
        {
            get => _threshold;
            set
            {
                Utility.Finite(value, nameof(Threshold));
                _threshold = value;
            }
        }

        /// <summary>
        /// Gets the hysteresis current.
        /// </summary>
        [ParameterName("ih"), ParameterInfo("Hysteresis current")]
        [Finite]
        public override double Hysteresis
        {
            get => _hysteresis;
            set
            {
                Utility.Finite(value, nameof(Hysteresis));
                _hysteresis = value;
            }
        }

        /// <inheritdoc/>
        CurrentModelParameters ICloneable<CurrentModelParameters>.Clone()
            => (CurrentModelParameters)Clone();
    }
}
