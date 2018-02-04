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
        [PropertyName("capacitance"), PropertyInfo("Device capacitance", IsPrincipal = true)]
        public Parameter Capacitance { get; } = new Parameter();
        [PropertyName("ic"), PropertyInfo("Initial capacitor voltage", Interesting = false)]
        public Parameter InitialCondition { get; } = new Parameter();
        [PropertyName("w"), PropertyInfo("Device width", Interesting = false)]
        public Parameter Width { get; } = new Parameter();
        [PropertyName("l"), PropertyInfo("Device length", Interesting = false)]
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
