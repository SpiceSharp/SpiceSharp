using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.SubcircuitBehaviors
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
        /// Gets or sets a value indicating whether solving should be done in parallel.
        /// </summary>
        /// <value>
        ///   <c>true</c> if solving is done in parallel; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("biasing.solve"), ParameterInfo("Flag indicating whether solving should be done in parallel")]
        public bool ParallelSolve { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether convergence should be tested in parallel.
        /// </summary>
        /// <value>
        ///   <c>true</c> if convergence is tested in parallel; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("biasing.convergence"), ParameterInfo("Flag indicating whether convergence testing should be done in parallel")]
        public bool ParallelConvergences { get; set; }

        /// <summary>
        /// Gets or sets the absolute tolerance.
        /// </summary>
        /// <value>
        /// The absolute tolerance.
        /// </value>
        [ParameterName("biasing.abstol"), ParameterInfo("The absolute tolerance when solving in parallel")]
        public double AbsoluteTolerance { get; set; } = 1e-12;

        /// <summary>
        /// Gets or sets the relative tolerance.
        /// </summary>
        /// <value>
        /// The relative tolerance.
        /// </value>
        [ParameterName("biasing.reltol"), ParameterInfo("The relative tolerance when solving in parallel")]
        public double RelativeTolerance { get; set; } = 1e-3;
    }
}
