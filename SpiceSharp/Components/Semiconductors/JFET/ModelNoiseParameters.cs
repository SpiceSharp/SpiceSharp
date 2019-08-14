using SpiceSharp.Attributes;

namespace SpiceSharp.Components.JFETBehaviors
{
    /// <summary>
    /// Noise parameters for a <see cref="JFETModel" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.ParameterSet" />
    public class ModelNoiseParameters : ParameterSet
    {
        /// <summary>
        /// Gets the flicker noise coefficient.
        /// </summary>
        [ParameterName("kf"), ParameterInfo("Flicker noise coefficient")]
        public GivenParameter<double> FnCoefficient { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the flicker noise exponent.
        /// </summary>
        [ParameterName("af"), ParameterInfo("Flicker noise exponent")]
        public GivenParameter<double> FnExponent { get; } = new GivenParameter<double>(1);
    }
}
