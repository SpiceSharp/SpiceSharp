using SpiceSharp.Attributes;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Noise parameters for a <see cref="BJTModel"/>
    /// </summary>
    public class ModelNoiseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyName("kf"), PropertyInfo("Flicker Noise Coefficient")]
        public Parameter FnCoefficient { get; } = new Parameter();
        [PropertyName("af"), PropertyInfo("Flicker Noise Exponent")]
        public Parameter FnExp { get; } = new Parameter(1);
    }
}
