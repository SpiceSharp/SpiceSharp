using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CAP
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
        public Parameter CAPcj { get; } = new Parameter();
        [PropertyName("cjsw"), PropertyInfo("Sidewall capacitance per meter")]
        public Parameter CAPcjsw { get; } = new Parameter();
        [PropertyName("defw"), PropertyInfo("Default width")]
        public Parameter CAPdefWidth { get; } = new Parameter(10.0e-6);
        [PropertyName("narrow"), PropertyInfo("Width correction factor")]
        public Parameter CAPnarrow { get; } = new Parameter();
    }
}
