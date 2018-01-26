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
        [PropertyName("k"), PropertyName("coefficient"), PropertyInfo("Mutual inductance", IsPrincipal = true)]
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
        /// <param name="k">Mutual inductance</param>
        public BaseParameters(double k)
        {
            Coupling.Set(k);
        }
    }
}
