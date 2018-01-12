using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Mosfet.Level1
{
    /// <summary>
    /// Noise parameters for a <see cref="MOS1"/>
    /// </summary>
    public class ModelNoiseParameters : Parameters
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("kf"), SpiceInfo("Flicker noise coefficient")]
        public Parameter MOS1fNcoef { get; } = new Parameter();
        [SpiceName("af"), SpiceInfo("Flicker noise exponent")]
        public Parameter MOS1fNexp { get; } = new Parameter(1);
    }
}
