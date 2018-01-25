using SpiceSharp.Attributes;

namespace SpiceSharp.Components.VSW
{
    /// <summary>
    /// Base parameters for a <see cref="VoltageSwitchModel"/>
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyNameAttribute("ron"), PropertyInfoAttribute("Resistance when closed")]
        public Parameter VSWon { get; } = new Parameter(1.0);
        [PropertyNameAttribute("roff"), PropertyInfoAttribute("Resistance when off")]
        public Parameter VSWoff { get; } = new Parameter(1.0e12);
        [PropertyNameAttribute("vt"), PropertyInfoAttribute("Threshold voltage")]
        public Parameter VSWthresh { get; } = new Parameter();
        [PropertyNameAttribute("vh"), PropertyInfoAttribute("Hysteresis voltage")]
        public Parameter VSWhyst { get; } = new Parameter();
    }
}
