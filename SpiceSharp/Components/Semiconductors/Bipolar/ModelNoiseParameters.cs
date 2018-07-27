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
        public GivenParameter<double> FlickerNoiseCoefficient { get; } = new GivenParameter<double>();
        [ParameterName("af"), ParameterInfo("Flicker Noise Exponent")]
        public GivenParameter<double> FlickerNoiseExponent { get; } = new GivenParameter<double>(1);
    }
}
