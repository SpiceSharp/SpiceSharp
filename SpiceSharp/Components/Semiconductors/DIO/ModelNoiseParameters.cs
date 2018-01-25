using SpiceSharp.Attributes;

namespace SpiceSharp.Components.DIO
{
    /// <summary>
    /// Noise parameters for a <see cref="DiodeModel"/>
    /// </summary>
    public class ModelNoiseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyName("kf"), PropertyInfo("flicker noise coefficient")]
        public Parameter DIOfNcoef { get; } = new Parameter();
        [PropertyName("af"), PropertyInfo("flicker noise exponent")]
        public Parameter DIOfNexp { get; } = new Parameter(1.0);
    }
}
