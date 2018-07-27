using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CurrentControlledVoltageSourceBehaviors
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
        public GivenParameter<double> Coefficient { get; } = new GivenParameter<double>();

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
