using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// Parameters for <see cref="IInitialConditionBehavior"/>
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class InitialConditionParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets a value indicating whether initial conditions are applied in parallel.
        /// </summary>
        /// <value>
        ///   <c>true</c> if initial conditions are applied in parallel; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("ic.ic"), ParameterInfo("Flag indicating whether initial conditions are applied in parallel")]
        public bool ParallelInitialCondition { get; set; }
    }
}
