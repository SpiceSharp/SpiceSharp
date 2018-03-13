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
        [ParameterName("k"), ParameterName("coefficient"), PropertyInfo("Mutual inductance", IsPrincipal = true)]
        public Parameter Coupling { get; } = new Parameter();

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
            Coupling.Set(coupling);
        }
    }
}
