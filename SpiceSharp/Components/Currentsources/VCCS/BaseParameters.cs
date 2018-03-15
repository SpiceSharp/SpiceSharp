using SpiceSharp.Attributes;

namespace SpiceSharp.Components.VoltageControlledCurrentsourceBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="VoltageControlledCurrentSource"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("gain"), ParameterInfo("Transconductance of the source (gain)")]
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
