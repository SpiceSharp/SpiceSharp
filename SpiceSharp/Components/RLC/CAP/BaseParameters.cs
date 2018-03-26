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
        public GivenParameter Capacitance { get; } = new GivenParameter();
        [ParameterName("ic"), ParameterInfo("Initial capacitor voltage", Interesting = false)]
        public GivenParameter InitialCondition { get; } = new GivenParameter();
        [ParameterName("w"), ParameterInfo("Device width", Interesting = false)]
        public GivenParameter Width { get; } = new GivenParameter();
        [ParameterName("l"), ParameterInfo("Device length", Interesting = false)]
        public GivenParameter Length { get; } = new GivenParameter();

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
            Capacitance.Value = cap;
        }
    }
}
