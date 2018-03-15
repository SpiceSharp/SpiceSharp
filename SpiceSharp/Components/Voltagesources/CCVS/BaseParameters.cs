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
        public Parameter Coefficient { get; } = new Parameter();

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
            Coefficient.Set(gain);
        }
    }
}
