using SpiceSharp.Behaviors;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// Parameters for <see cref="ITimeBehavior"/>
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class TimeParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets a value indicating whether states should be initialized in parallel.
        /// </summary>
        /// <value>
        ///   <c>true</c> if states are initialized in parallel; otherwise, <c>false</c>.
        /// </value>
        public bool ParallelInitialize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether loading should be done in parallel.
        /// </summary>
        /// <value>
        ///   <c>true</c> if loading is done in parallel; otherwise, <c>false</c>.
        /// </value>
        public bool ParallelLoad { get; set; }
    }
}
