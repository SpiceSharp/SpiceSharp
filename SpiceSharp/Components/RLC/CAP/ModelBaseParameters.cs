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
        [PropertyNameAttribute("cj"), PropertyInfoAttribute("Bottom capacitance per area")]
        public Parameter CAPcj { get; } = new Parameter();
        [PropertyNameAttribute("cjsw"), PropertyInfoAttribute("Sidewall capacitance per meter")]
        public Parameter CAPcjsw { get; } = new Parameter();
        [PropertyNameAttribute("defw"), PropertyInfoAttribute("Default width")]
        public Parameter CAPdefWidth { get; } = new Parameter(10.0e-6);
        [PropertyNameAttribute("narrow"), PropertyInfoAttribute("Width correction factor")]
        public Parameter CAPnarrow { get; } = new Parameter();
    }
}
