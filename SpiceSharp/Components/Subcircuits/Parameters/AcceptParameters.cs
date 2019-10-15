using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Parameters for <see cref="IAcceptBehavior"/>.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class AcceptParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets a value indicating whether accepting a timepoint should be done in parallel.
        /// </summary>
        /// <value>
        ///   <c>true</c> if timepoints are accepted in parallel; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("accept.accept"), ParameterInfo("Flag indicating whether accepting a timepoint is done in parallel")]
        public bool ParallelAccept { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether probing a timepoint should be done in parallel.
        /// </summary>
        /// <value>
        ///   <c>true</c> if timepoints are probed in parallel; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("accept.probe"), ParameterInfo("Flag indicating whether probing a timepoint is done in parallel")]
        public bool ParallelProbe { get; set; }
    }
}
