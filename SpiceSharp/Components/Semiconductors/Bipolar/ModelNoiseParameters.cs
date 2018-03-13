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
        [ParameterName("kf"), PropertyInfo("Flicker Noise Coefficient")]
        public Parameter FlickerNoiseCoefficient { get; } = new Parameter();
        [ParameterName("af"), PropertyInfo("Flicker Noise Exponent")]
        public Parameter FlickerNoiseExponent { get; } = new Parameter(1);
    }
}
