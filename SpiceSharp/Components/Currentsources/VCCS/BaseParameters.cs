using SpiceSharp.Attributes;

namespace SpiceSharp.Components.VCCS
{
    /// <summary>
    /// Base parameters for a <see cref="VoltageControlledCurrentsource"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [NameAttribute("gain"), InfoAttribute("Transconductance of the source (gain)")]
        public Parameter VCCScoeff { get; } = new Parameter();

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
            VCCScoeff.Set(gain);
        }
    }
}
