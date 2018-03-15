using SpiceSharp.Attributes;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Noise parameters for a <see cref="DiodeModel"/>
    /// </summary>
    public class ModelNoiseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("kf"), ParameterInfo("flicker noise coefficient")]
        public GivenParameter FlickerNoiseCoefficient { get; } = new GivenParameter();
        [ParameterName("af"), ParameterInfo("flicker noise exponent")]
        public GivenParameter FlickerNoiseExponent { get; } = new GivenParameter(1.0);
    }
}
