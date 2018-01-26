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
        [PropertyName("kf"), PropertyInfo("Flicker noise coefficient")]
        public Parameter FnCoef { get; } = new Parameter();
        [PropertyName("af"), PropertyInfo("Flicker noise exponent")]
        public Parameter FnExp { get; } = new Parameter(1);
    }
}
