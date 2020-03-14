using SpiceSharp.Attributes;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Noise parameters for a <see cref="DiodeModel"/>
    /// </summary>
    public class ModelNoiseParameters : ParameterSet
    {
        /// <summary>
        /// Gets the flicker noise coefficient parameter.
        /// </summary>
        [ParameterName("kf"), ParameterInfo("flicker noise coefficient")]
        public double FlickerNoiseCoefficient { get; set; }

        /// <summary>
        /// Gets the flicker noise exponent parameter.
        /// </summary>
        [ParameterName("af"), ParameterInfo("flicker noise exponent")]
        public double FlickerNoiseExponent { get; set; } = 1;
    }
}
