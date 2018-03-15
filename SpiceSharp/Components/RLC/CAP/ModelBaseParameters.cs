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
        public Parameter JunctionCap { get; } = new Parameter();
        [ParameterName("cjsw"), ParameterInfo("Sidewall capacitance per meter")]
        public Parameter JunctionCapSidewall { get; } = new Parameter();
        [ParameterName("defw"), ParameterInfo("Default width")]
        public Parameter DefaultWidth { get; } = new Parameter(10.0e-6);
        [ParameterName("narrow"), ParameterInfo("Width correction factor")]
        public Parameter Narrow { get; } = new Parameter();
    }
}
