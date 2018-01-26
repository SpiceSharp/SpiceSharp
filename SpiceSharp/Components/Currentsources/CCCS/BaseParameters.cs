using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CurrentControlledCurrentSourceBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="CurrentControlledCurrentsource"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        // TODO: Correct spelling for device parameters
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
