using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CCVS
{
    /// <summary>
    /// Base parameters for a <see cref="CurrentControlledVoltagesource"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("gain"), SpiceInfo("Transresistance (gain)")]
        public Parameter CCVScoeff { get; } = new Parameter();

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
            CCVScoeff.Set(gain);
        }
    }
}
