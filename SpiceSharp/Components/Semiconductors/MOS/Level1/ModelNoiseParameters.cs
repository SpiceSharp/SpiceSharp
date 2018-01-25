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
        [PropertyNameAttribute("kf"), PropertyInfoAttribute("Flicker noise coefficient")]
        public Parameter MOS1fNcoef { get; } = new Parameter();
        [PropertyNameAttribute("af"), PropertyInfoAttribute("Flicker noise exponent")]
        public Parameter MOS1fNexp { get; } = new Parameter(1);
    }
}
