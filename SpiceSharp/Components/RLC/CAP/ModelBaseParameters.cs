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
        [NameAttribute("cj"), InfoAttribute("Bottom capacitance per area")]
        public Parameter CAPcj { get; } = new Parameter();
        [NameAttribute("cjsw"), InfoAttribute("Sidewall capacitance per meter")]
        public Parameter CAPcjsw { get; } = new Parameter();
        [NameAttribute("defw"), InfoAttribute("Default width")]
        public Parameter CAPdefWidth { get; } = new Parameter(10.0e-6);
        [NameAttribute("narrow"), InfoAttribute("Width correction factor")]
        public Parameter CAPnarrow { get; } = new Parameter();
    }
}
