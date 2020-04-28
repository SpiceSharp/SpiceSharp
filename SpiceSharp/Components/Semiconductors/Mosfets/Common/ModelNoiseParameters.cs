using SpiceSharp.Attributes;

namespace SpiceSharp.Components.MosfetBehaviors
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
        public double FlickerNoiseCoefficient { get; set; }

        /// <summary>
        /// Gets the flicker-noise exponent parameter.
        /// </summary>
        [ParameterName("af"), ParameterInfo("Flicker noise exponent")]
        public double FlickerNoiseExponent { get; set; } = 1;
    }
}
