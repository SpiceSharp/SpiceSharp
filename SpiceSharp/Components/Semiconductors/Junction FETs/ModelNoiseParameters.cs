using SpiceSharp.Attributes;

namespace SpiceSharp.Components.JFETBehaviors
{
    /// <summary>
    /// Noise parameters for a <see cref="JFETModel" />.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class ModelNoiseParameters : ParameterSet
    {
        /// <summary>
        /// Gets the flicker noise coefficient.
        /// </summary>
        [ParameterName("kf"), ParameterInfo("Flicker noise coefficient")]
        public double FnCoefficient { get; set; }

        /// <summary>
        /// Gets the flicker noise exponent.
        /// </summary>
        [ParameterName("af"), ParameterInfo("Flicker noise exponent")]
        public double FnExponent { get; set; } = 1;
    }
}
