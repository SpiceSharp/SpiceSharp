using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Mosfet.Level2
{
    /// <summary>
    /// Noise parameters for a <see cref="MOS2Model"/>
    /// </summary>
    public class ModelNoiseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("kf"), SpiceInfo("Flicker noise coefficient")]
        public Parameter MOS2fNcoef { get; } = new Parameter();
        [SpiceName("af"), SpiceInfo("Flicker noise exponent")]
        public Parameter MOS2fNexp { get; } = new Parameter(1);
    }
}
