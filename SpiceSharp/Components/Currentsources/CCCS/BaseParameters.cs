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
        public GivenParameter<double> Coefficient { get; } = new GivenParameter<double>();

        /// <summary>
        /// Creates a new instance of the <see cref="BaseParameters"/> class.
        /// </summary>
        public BaseParameters()
        {
        }

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
