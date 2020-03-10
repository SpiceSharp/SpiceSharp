using SpiceSharp.Attributes;

namespace SpiceSharp.Components.ParallelBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="ParallelComponents"/>.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the temperature distributor.
        /// </summary>
        /// <value>
        /// The temperature distributor.
        /// </value>
        [ParameterName("temperature"), ParameterInfo("Work distributor for temperature-dependent calculations.")]
        public IWorkDistributor TemperatureDistributor { get; set; }

        /// <summary>
        /// Gets or sets the work distributor for loading in biasing simulations.
        /// </summary>
        /// <value>
        /// The load distributor.
        /// </value>
        [ParameterName("biasing.load"), ParameterInfo("Work distributor for loading the Y-matrix and Rhs-vector.")]
        public IWorkDistributor LoadDistributor { get; set; }

        /// <summary>
        /// Gets or sets the work distributor for testing convergence.
        /// </summary>
        /// <value>
        /// The convergence distributor.
        /// </value>
        [ParameterName("biasing.isconvergent"), ParameterInfo("Work distributor for testing convergence.")]
        public IWorkDistributor<bool> ConvergenceDistributor { get; set; }

        /// <summary>
        /// Gets or sets the ac load distributor.
        /// </summary>
        /// <value>
        /// The ac load distributor.
        /// </value>
        [ParameterName("frequency.load"), ParameterInfo("Work distributor for loading the small-signal components.")]
        public IWorkDistributor AcLoadDistributor { get; set; }

        /// <summary>
        /// Gets or sets the ac initialize distributor.
        /// </summary>
        /// <value>
        /// The ac initialize distributor.
        /// </value>
        [ParameterName("frequency.init"), ParameterName("frequency.initialize"), ParameterInfo("Work distributor for initializing small-signal parameters.")]
        public IWorkDistributor AcInitDistributor { get; set; }
    }
}
