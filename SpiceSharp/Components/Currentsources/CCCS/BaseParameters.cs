using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CCCS
{
    /// <summary>
    /// Base parameters for a <see cref="CurrentControlledCurrentsource"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyName("gain"), PropertyInfo("Gain of the source")]
        public Parameter CCCScoeff { get; } = new Parameter();

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseParameters()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gain">Gain</param>
        public BaseParameters(double gain)
        {
            CCCScoeff.Set(gain);
        }
    }
}
