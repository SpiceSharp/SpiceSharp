using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CurrentSwitchBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="CurrentSwitchModel"/>
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("ron"), ParameterInfo("Closed resistance")]
        public GivenParameter OnResistance { get; } = new GivenParameter(1.0);
        [ParameterName("roff"), ParameterInfo("Open resistance")]
        public GivenParameter OffResistance { get; } = new GivenParameter(1.0e12);
        [ParameterName("it"), ParameterInfo("Threshold current")]
        public GivenParameter Threshold { get; } = new GivenParameter();
        [ParameterName("ih"), ParameterInfo("Hysteresis current")]
        public GivenParameter Hysteresis { get; } = new GivenParameter();
    }
}
