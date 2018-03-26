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
        public GivenParameter Coupling { get; } = new GivenParameter();

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseParameters()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="coupling">Mutual inductance</param>
        public BaseParameters(double coupling)
        {
            Coupling.Value = coupling;
        }
    }
}
