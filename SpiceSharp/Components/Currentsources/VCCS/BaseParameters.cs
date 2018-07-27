using SpiceSharp.Attributes;

namespace SpiceSharp.Components.VoltageControlledCurrentSourceBehaviors
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
        public GivenParameter<double> Coefficient { get; } = new GivenParameter<double>();

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
