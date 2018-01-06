using SpiceSharp.Attributes;

namespace SpiceSharp.Components.VCVS
{
    /// <summary>
    /// Base parameters for a <see cref="VoltageControlledVoltagesource"/>
    /// </summary>
    public class BaseParameters : Parameters
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("gain"), SpiceInfo("Voltage gain")]
        public Parameter VCVScoeff { get; } = new Parameter();

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseParameters() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gain">Gain</param>
        public BaseParameters(double gain)
        {
            VCVScoeff.Set(gain);
        }
    }
}
