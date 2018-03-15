using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CurrentControlledVoltagesourceBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="CurrentControlledVoltageSource"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("gain"), ParameterInfo("Transresistance (gain)")]
        public GivenParameter Coefficient { get; } = new GivenParameter();

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseParameters() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gain"></param>
        public BaseParameters(double gain)
        {
            Coefficient.Value = gain;
        }
    }
}
