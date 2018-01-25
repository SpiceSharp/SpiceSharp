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
        [NameAttribute("kf"), InfoAttribute("Flicker Noise Coefficient")]
        public Parameter BJTfNcoef { get; } = new Parameter();
        [NameAttribute("af"), InfoAttribute("Flicker Noise Exponent")]
        public Parameter BJTfNexp { get; } = new Parameter(1);
    }
}
