using SpiceSharp.Attributes;

namespace SpiceSharp.Components.MosfetBehaviors.Level1
{
    /// <summary>
    /// Noise parameters for a <see cref="MOS1"/>
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
