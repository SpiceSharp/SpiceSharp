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
        [NameAttribute("ron"), InfoAttribute("Resistance when closed")]
        public Parameter VSWon { get; } = new Parameter(1.0);
        [NameAttribute("roff"), InfoAttribute("Resistance when off")]
        public Parameter VSWoff { get; } = new Parameter(1.0e12);
        [NameAttribute("vt"), InfoAttribute("Threshold voltage")]
        public Parameter VSWthresh { get; } = new Parameter();
        [NameAttribute("vh"), InfoAttribute("Hysteresis voltage")]
        public Parameter VSWhyst { get; } = new Parameter();
    }
}
