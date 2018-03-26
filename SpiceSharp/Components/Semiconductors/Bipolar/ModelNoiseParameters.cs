using SpiceSharp.Attributes;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Noise parameters for a <see cref="BipolarJunctionTransistorModel"/>
    /// </summary>
    public class ModelNoiseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("kf"), ParameterInfo("Flicker Noise Coefficient")]
        public GivenParameter FlickerNoiseCoefficient { get; } = new GivenParameter();
        [ParameterName("af"), ParameterInfo("Flicker Noise Exponent")]
        public GivenParameter FlickerNoiseExponent { get; } = new GivenParameter(1);
    }
}
