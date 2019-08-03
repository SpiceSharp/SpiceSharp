using SpiceSharp.Attributes;

namespace SpiceSharp.Components.VoltageControlledVoltageSourceBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="VoltageControlledVoltageSource"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("gain"), ParameterInfo("Voltage gain")]
        public GivenParameter<double> Coefficient { get; } = new GivenParameter<double>();

        /// <summary>
        /// Creates a new instance of the <see cref="BaseParameters"/> class.
        /// </summary>
        public BaseParameters() { }

        /// <summary>
        /// Creates a new instance of the <see cref="BaseParameters"/> class.
        /// </summary>
        /// <param name="gain">Gain</param>
        public BaseParameters(double gain)
        {
            Coefficient.Value = gain;
        }
    }
}
