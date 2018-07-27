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
        public GivenParameter<double> JunctionCap { get; } = new GivenParameter<double>();
        [ParameterName("cjsw"), ParameterInfo("Sidewall capacitance per meter")]
        public GivenParameter<double> JunctionCapSidewall { get; } = new GivenParameter<double>();
        [ParameterName("defw"), ParameterInfo("Default width")]
        public GivenParameter<double> DefaultWidth { get; } = new GivenParameter<double>(10.0e-6);
        [ParameterName("narrow"), ParameterInfo("Width correction factor")]
        public GivenParameter<double> Narrow { get; } = new GivenParameter<double>();
    }
}
