using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Parameters for <see cref="IFrequencyBehavior"/>.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class FrequencyParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets a value indicating whether parameter initialization is done in parallel.
        /// </summary>
        /// <value>
        ///   <c>true</c> if parameter initialization is done in parallel; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("frequency.initialize"), ParameterInfo("Flag indicating whether parameter initialization is done in parallel")]
        public bool ParallelInitialize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether loading is done in parallel.
        /// </summary>
        /// <value>
        ///   <c>true</c> if loading is done in parallel; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("frequency.load"), ParameterInfo("Flag indicating whether loading is done in parallel")]
        public bool ParallelLoad { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether solving is done in parallel.
        /// </summary>
        /// <value>
        ///   <c>true</c> if solving is done in parallel; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("frequency.solve"), ParameterInfo("Flag indicating whether solving is done in parallel")]
        public bool ParallelSolve { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether updates are done in parallel.
        /// </summary>
        /// <value>
        ///   <c>true</c> if update are done in parallel; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("frequency.update"), ParameterInfo("Flag indicating whether updates are done in parallel")]
        public bool ParallelUpdate { get; set; }
    }
}
