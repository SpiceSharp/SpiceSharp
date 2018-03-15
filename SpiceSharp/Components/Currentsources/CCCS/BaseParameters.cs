using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CurrentControlledCurrentSourceBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="CurrentControlledCurrentSource"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("gain"), ParameterInfo("Gain of the source")]
        public GivenParameter Coefficient { get; } = new GivenParameter();

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
            Coefficient.Value = gain;
        }
    }
}
