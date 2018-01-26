using SpiceSharp.Attributes;

namespace SpiceSharp.Components.MosfetBehaviors.Level2
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
        public Parameter FNcoef { get; } = new Parameter();
        [PropertyName("af"), PropertyInfo("Flicker noise exponent")]
        public Parameter FNexp { get; } = new Parameter(1);
    }
}
