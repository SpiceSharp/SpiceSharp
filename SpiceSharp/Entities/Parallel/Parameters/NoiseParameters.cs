using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// Parameters for <see cref="INoiseBehavior"/>
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class NoiseParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets a value indicating whether noise contributions are calculated in parallel.
        /// </summary>
        /// <value>
        ///   <c>true</c> if noise contributions are calculated in parallel; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("noise.noise"), ParameterInfo("Flag indicating whether noise contributions should be calculated in parallel")]
        public bool ParallelNoise { get; set; }
    }
}
