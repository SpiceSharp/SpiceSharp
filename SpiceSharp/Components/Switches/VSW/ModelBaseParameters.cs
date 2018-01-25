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
        [SpiceName("ron"), SpiceInfo("Resistance when closed")]
        public Parameter VSWon { get; } = new Parameter(1.0);
        [SpiceName("roff"), SpiceInfo("Resistance when off")]
        public Parameter VSWoff { get; } = new Parameter(1.0e12);
        [SpiceName("vt"), SpiceInfo("Threshold voltage")]
        public Parameter VSWthresh { get; } = new Parameter();
        [SpiceName("vh"), SpiceInfo("Hysteresis voltage")]
        public Parameter VSWhyst { get; } = new Parameter();
    }
}
