using SpiceSharp.Attributes;

namespace SpiceSharp.Components.MosfetBehaviors.Common
{
    /// <summary>
    /// Noise parameters for a <see cref="Model"/>
    /// </summary>
    public class ModelNoiseParameters : ParameterSet
    {
        /// <summary>
        /// Gets the flicker-noise coefficient parameter.
        /// </summary>
        [ParameterName("kf"), ParameterInfo("Flicker noise coefficient")]
        public GivenParameter<double> FlickerNoiseCoefficient { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the flicker-noise exponent parameter.
        /// </summary>
        [ParameterName("af"), ParameterInfo("Flicker noise exponent")]
        public GivenParameter<double> FlickerNoiseExponent { get; } = new GivenParameter<double>(1);
    }
}
