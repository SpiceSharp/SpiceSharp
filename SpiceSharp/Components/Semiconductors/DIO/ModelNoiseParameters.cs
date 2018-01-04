using SpiceSharp.Attributes;

namespace SpiceSharp.Components.DIO
{
    /// <summary>
    /// Noise parameters for a <see cref="DiodeModel"/>
    /// </summary>
    public class ModelNoiseParameters : Parameters
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("kf"), SpiceInfo("flicker noise coefficient")]
        public Parameter DIOfNcoef { get; } = new Parameter();
        [SpiceName("af"), SpiceInfo("flicker noise exponent")]
        public Parameter DIOfNexp { get; } = new Parameter(1.0);
    }
}
