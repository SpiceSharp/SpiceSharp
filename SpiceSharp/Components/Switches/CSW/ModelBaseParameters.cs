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
        public GivenParameter<double> OnResistance { get; } = new GivenParameter<double>(1.0);
        [ParameterName("roff"), ParameterInfo("Open resistance")]
        public GivenParameter<double> OffResistance { get; } = new GivenParameter<double>(1.0e12);
        [ParameterName("it"), ParameterInfo("Threshold current")]
        public GivenParameter<double> Threshold { get; } = new GivenParameter<double>();
        [ParameterName("ih"), ParameterInfo("Hysteresis current")]
        public GivenParameter<double> Hysteresis { get; } = new GivenParameter<double>();
    }
}
