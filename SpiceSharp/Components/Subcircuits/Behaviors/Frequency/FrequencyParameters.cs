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
    }
}
