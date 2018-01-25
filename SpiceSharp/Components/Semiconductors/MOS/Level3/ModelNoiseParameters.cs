using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Mosfet.Level3
{
    /// <summary>
    /// Noise parameters for a <see cref="MOS2Model"/>
    /// </summary>
    public class ModelNoiseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyNameAttribute("kf"), PropertyInfoAttribute("Flicker noise coefficient")]
        public Parameter MOS3fNcoef { get; } = new Parameter();
        [PropertyNameAttribute("af"), PropertyInfoAttribute("Flicker noise exponent")]
        public Parameter MOS3fNexp { get; } = new Parameter(1);
    }
}
