using SpiceSharp.Attributes;

namespace SpiceSharp.Components.MUT
{
    /// <summary>
    /// Base parameters for a <see cref="MutualInductance"/>
    /// </summary>
    public class BaseParameters : Parameters
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("k"), SpiceName("coefficient"), SpiceInfo("Mutual inductance", IsPrincipal = true)]
        public Parameter MUTcoupling { get; } = new Parameter();

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
            MUTcoupling.Set(k);
        }
    }
}
