using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// Base parameters for a capacitor
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("capacitance"), ParameterInfo("Device capacitance", IsPrincipal = true)]
        public Parameter Capacitance { get; } = new Parameter();
        [ParameterName("ic"), ParameterInfo("Initial capacitor voltage", Interesting = false)]
        public Parameter InitialCondition { get; } = new Parameter();
        [ParameterName("w"), ParameterInfo("Device width", Interesting = false)]
        public Parameter Width { get; } = new Parameter();
        [ParameterName("l"), ParameterInfo("Device length", Interesting = false)]
        public Parameter Length { get; } = new Parameter();

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
            Capacitance.Set(cap);
        }
    }
}
