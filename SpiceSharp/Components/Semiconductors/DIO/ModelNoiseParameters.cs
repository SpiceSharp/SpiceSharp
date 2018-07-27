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
        public GivenParameter<double> FlickerNoiseCoefficient { get; } = new GivenParameter<double>();
        [ParameterName("af"), ParameterInfo("flicker noise exponent")]
        public GivenParameter<double> FlickerNoiseExponent { get; } = new GivenParameter<double>(1.0);
    }
}
