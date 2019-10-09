using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// Parameter set for configuring parallel computation of <see cref="IBiasingBehavior"/>.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class BiasingParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets a value indicating whether loading should be done in parallel.
        /// </summary>
        /// <value>
        ///   <c>true</c> if loading is done in parallel; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("biasing.load"), ParameterInfo("Flag indicating whether loading should be done in parallel")]
        public bool ParallelLoad { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether convergence should be tested in parallel.
        /// </summary>
        /// <value>
        ///   <c>true</c> if convergence is tested in parallel; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("biasing.convergence"), ParameterInfo("Flag indicating whether convergence testing should be done in parallel")]
        public bool ParallelConvergences { get; set; }
    }
}
