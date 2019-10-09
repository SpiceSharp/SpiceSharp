using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// Parameters for <see cref="ITemperatureBehavior"/>
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class TemperatureParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets a value indicating whether temperature calculations should be done in parallel.
        /// </summary>
        /// <value>
        ///   <c>true</c> if temperature calculations are done in parallel; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("temperature.temperature"), ParameterInfo("Flag indicating whether temperature calculations should be done in parallel")]
        public bool ParallelTemperature { get; set; }
    }
}
