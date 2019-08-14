using SpiceSharp.Attributes;

namespace SpiceSharp.Components.MutualInductanceBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="MutualInductance"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("k"), ParameterName("coefficient"), ParameterInfo("Mutual inductance", IsPrincipal = true)]
        public GivenParameter<double> Coupling { get; } = new GivenParameter<double>();

        /// <summary>
        /// Creates a new instance of the <see cref="BaseParameters"/> class.
        /// </summary>
        public BaseParameters()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BaseParameters"/> class.
        /// </summary>
        /// <param name="coupling">Mutual inductance</param>
        public BaseParameters(double coupling)
        {
            Coupling.Value = coupling;
        }
    }
}
