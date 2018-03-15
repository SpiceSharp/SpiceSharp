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
        public Parameter FlickerNoiseCoefficient { get; } = new Parameter();
        [ParameterName("af"), ParameterInfo("Flicker noise exponent")]
        public Parameter FlickerNoiseExponent { get; } = new Parameter(1);
    }
}
