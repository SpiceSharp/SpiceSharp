using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Bipolar
{
    /// <summary>
    /// Noise parameters for a <see cref="BJTModel"/>
    /// </summary>
    public class ModelNoiseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyNameAttribute("kf"), PropertyInfoAttribute("Flicker Noise Coefficient")]
        public Parameter BJTfNcoef { get; } = new Parameter();
        [PropertyNameAttribute("af"), PropertyInfoAttribute("Flicker Noise Exponent")]
        public Parameter BJTfNexp { get; } = new Parameter(1);
    }
}
