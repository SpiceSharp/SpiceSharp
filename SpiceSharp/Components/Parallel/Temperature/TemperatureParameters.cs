using SpiceSharp.Attributes;

namespace SpiceSharp.Components.ParallelBehaviors
{
    /// <summary>
    /// Parameters for temperature-dependent calculations of a <see cref="ParallelComponents"/>.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class TemperatureParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the temperature work distributor.
        /// </summary>
        /// <value>
        /// The temperature work distributor.
        /// </value>
        [ParameterName("temperature.distributor"), ParameterInfo("The work distributor for temperature dependent calculations.")]
        public IWorkDistributor TemperatureDistributor { get; set; }
    }
}
