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
        [PropertyName("cj"), PropertyInfo("Bottom capacitance per area")]
        public Parameter Cj { get; } = new Parameter();
        [PropertyName("cjsw"), PropertyInfo("Sidewall capacitance per meter")]
        public Parameter Cjsw { get; } = new Parameter();
        [PropertyName("defw"), PropertyInfo("Default width")]
        public Parameter DefWidth { get; } = new Parameter(10.0e-6);
        [PropertyName("narrow"), PropertyInfo("Width correction factor")]
        public Parameter Narrow { get; } = new Parameter();
    }
}
