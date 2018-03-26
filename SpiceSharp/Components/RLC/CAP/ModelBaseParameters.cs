using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// Parameters for the capacitor model
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("cj"), ParameterInfo("Bottom capacitance per area")]
        public GivenParameter JunctionCap { get; } = new GivenParameter();
        [ParameterName("cjsw"), ParameterInfo("Sidewall capacitance per meter")]
        public GivenParameter JunctionCapSidewall { get; } = new GivenParameter();
        [ParameterName("defw"), ParameterInfo("Default width")]
        public GivenParameter DefaultWidth { get; } = new GivenParameter(10.0e-6);
        [ParameterName("narrow"), ParameterInfo("Width correction factor")]
        public GivenParameter Narrow { get; } = new GivenParameter();
    }
}
