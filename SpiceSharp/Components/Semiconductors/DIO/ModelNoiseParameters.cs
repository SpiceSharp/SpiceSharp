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
        [PropertyNameAttribute("kf"), PropertyInfoAttribute("flicker noise coefficient")]
        public Parameter DIOfNcoef { get; } = new Parameter();
        [PropertyNameAttribute("af"), PropertyInfoAttribute("flicker noise exponent")]
        public Parameter DIOfNexp { get; } = new Parameter(1.0);
    }
}
