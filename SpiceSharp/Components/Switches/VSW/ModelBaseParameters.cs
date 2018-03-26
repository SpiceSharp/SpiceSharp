using SpiceSharp.Attributes;

namespace SpiceSharp.Components.VoltageSwitchBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="VoltageSwitchModel"/>
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("ron"), ParameterInfo("Resistance when closed")]
        public GivenParameter OnResistance { get; } = new GivenParameter(1.0);
        [ParameterName("roff"), ParameterInfo("Resistance when off")]
        public GivenParameter OffResistance { get; } = new GivenParameter(1.0e12);
        [ParameterName("vt"), ParameterInfo("Threshold voltage")]
        public GivenParameter Threshold { get; } = new GivenParameter();
        [ParameterName("vh"), ParameterInfo("Hysteresis voltage")]
        public GivenParameter Hysteresis { get; } = new GivenParameter();
    }
}
