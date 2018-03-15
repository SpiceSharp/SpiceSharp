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
        public Parameter FlickerNoiseCoefficient { get; } = new Parameter();
        [ParameterName("af"), ParameterInfo("flicker noise exponent")]
        public Parameter FlickerNoiseExponent { get; } = new Parameter(1.0);
    }
}
