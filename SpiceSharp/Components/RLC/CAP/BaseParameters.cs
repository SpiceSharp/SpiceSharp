using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CAP
{
    /// <summary>
    /// Base parameters for a capacitor
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyNameAttribute("capacitance"), PropertyInfoAttribute("Device capacitance", IsPrincipal = true)]
        public Parameter CAPcapac { get; } = new Parameter();
        [PropertyNameAttribute("ic"), PropertyInfoAttribute("Initial capacitor voltage", Interesting = false)]
        public Parameter CAPinitCond { get; } = new Parameter();
        [PropertyNameAttribute("w"), PropertyInfoAttribute("Device width", Interesting = false)]
        public Parameter CAPwidth { get; } = new Parameter();
        [PropertyNameAttribute("l"), PropertyInfoAttribute("Device length", Interesting = false)]
        public Parameter CAPlength { get; } = new Parameter();

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseParameters()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cap">Capacitance</param>
        public BaseParameters(double cap)
        {
            CAPcapac.Set(cap);
        }
    }
}
