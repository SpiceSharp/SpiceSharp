using SpiceSharp.Attributes;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Noise parameters for a <see cref="BipolarJunctionTransistorModel"/>
    /// </summary>
    public class ModelNoiseParameters : ParameterSet
    {
        /// <summary>
        /// Gets the flicker noise coefficient parameter.
        /// </summary>
        [ParameterName("kf"), ParameterInfo("Flicker Noise Coefficient")]
        public GivenParameter<double> FlickerNoiseCoefficient { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the flicker noise exponent parameter.
        /// </summary>
        [ParameterName("af"), ParameterInfo("Flicker Noise Exponent")]
        public GivenParameter<double> FlickerNoiseExponent { get; } = new GivenParameter<double>(1);
    }
}
