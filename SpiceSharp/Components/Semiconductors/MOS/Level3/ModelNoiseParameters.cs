using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Mosfet.Level3
{
    /// <summary>
    /// Noise parameters for a <see cref="MOS2Model"/>
    /// </summary>
    public class ModelNoiseParameters : Parameters
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("kf"), SpiceInfo("Flicker noise coefficient")]
        public Parameter MOS3fNcoef { get; } = new Parameter();
        [SpiceName("af"), SpiceInfo("Flicker noise exponent")]
        public Parameter MOS3fNexp { get; } = new Parameter(1);
    }
}
