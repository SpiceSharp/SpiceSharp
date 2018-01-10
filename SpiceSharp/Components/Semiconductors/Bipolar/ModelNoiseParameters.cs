using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Bipolar
{
    /// <summary>
    /// Noise parameters for a <see cref="BJTModel"/>
    /// </summary>
    public class ModelNoiseParameters : Parameters
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("kf"), SpiceInfo("Flicker Noise Coefficient")]
        public Parameter BJTfNcoef { get; } = new Parameter();
        [SpiceName("af"), SpiceInfo("Flicker Noise Exponent")]
        public Parameter BJTfNexp { get; } = new Parameter(1);
    }
}
