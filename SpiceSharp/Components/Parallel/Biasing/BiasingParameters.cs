using SpiceSharp.Attributes;

namespace SpiceSharp.Components.ParallelBehaviors
{
    /// <summary>
    /// Biasing parameters for parallel loading.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class BiasingParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets a value indicating whether loading should be done in parallel.
        /// </summary>
        /// <value>
        ///   <c>true</c> if loading should be done in parallel; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("biasing.load"), ParameterInfo("Flag indicating whether loading should be done in parallel for biasing behaviors.")]
        public IWorkDistributor LoadDistributor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether convergence should be tested in parallel.
        /// </summary>
        /// <value>
        ///   <c>true</c> if convergence should be tested in parallel; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("biasing.convergent"), ParameterInfo("Flag indicating whether convergence should be tested in parallel for biasing behaviors.")]
        public IWorkDistributor<bool> ConvergenceDistributor { get; set; }
    }
}
