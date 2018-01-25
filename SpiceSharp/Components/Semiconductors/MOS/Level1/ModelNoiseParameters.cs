using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Mosfet.Level1
{
    /// <summary>
    /// Noise parameters for a <see cref="MOS1"/>
    /// </summary>
    public class ModelNoiseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [NameAttribute("kf"), InfoAttribute("Flicker noise coefficient")]
        public Parameter MOS1fNcoef { get; } = new Parameter();
        [NameAttribute("af"), InfoAttribute("Flicker noise exponent")]
        public Parameter MOS1fNexp { get; } = new Parameter(1);
    }
}
