using SpiceSharp.Attributes;

namespace SpiceSharp.Components.MosfetBehaviors.Level3
{
    /// <summary>
    /// Noise parameters for a <see cref="Model"/>
    /// </summary>
    public class ModelNoiseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("kf"), ParameterInfo("Flicker noise coefficient")]
        public GivenParameter<double> FlickerNoiseCoefficient { get; } = new GivenParameter<double>();
        [ParameterName("af"), ParameterInfo("Flicker noise exponent")]
        public GivenParameter<double> FlickerNoiseExponent { get; } = new GivenParameter<double>(1);
    }
}
