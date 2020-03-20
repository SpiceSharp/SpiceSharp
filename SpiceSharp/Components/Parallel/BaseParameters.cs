using SpiceSharp.Attributes;
using SpiceSharp.Entities;

namespace SpiceSharp.Components.ParallelBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="ParallelComponents"/>.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the entities that should be run in parallel.
        /// </summary>
        /// <value>
        /// The entities.
        /// </value>
        public IEntityCollection Entities { get; set; }

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
        [ParameterName("biasing.load"), ParameterInfo("Work distributor for loading the Y-matrix and Rhs-vector")]
        public IWorkDistributor LoadDistributor { get; set; }

        /// <summary>
        /// Gets or sets the work distributor for testing convergence.
        /// </summary>
        /// <value>
        /// The convergence distributor.
        /// </value>
        [ParameterName("biasing.convergence"), ParameterInfo("Work distributor for testing convergence")]
        public IWorkDistributor<bool> ConvergenceDistributor { get; set; }

        /// <summary>
        /// Gets or sets the biasing update distributor.
        /// </summary>
        /// <value>
        /// The biasing update distributor.
        /// </value>
        [ParameterName("biasing.update"), ParameterInfo("Work distributor for updating a behavior after solving")]
        public IWorkDistributor UpdateDistributor { get; set; }

        /// <summary>
        /// Gets or sets the ac load distributor.
        /// </summary>
        /// <value>
        /// The ac load distributor.
        /// </value>
        [ParameterName("frequency.load"), ParameterInfo("Work distributor for loading the small-signal components")]
        public IWorkDistributor AcLoadDistributor { get; set; }

        /// <summary>
        /// Gets or sets the ac initialize distributor.
        /// </summary>
        /// <value>
        /// The ac initialize distributor.
        /// </value>
        [ParameterName("frequency.init"), ParameterName("frequency.initialize"), ParameterInfo("Work distributor for initializing small-signal parameters")]
        public IWorkDistributor AcInitDistributor { get; set; }

        /// <summary>
        /// Gets or sets the time initialize distributor.
        /// </summary>
        /// <value>
        /// The time initialize distributor.
        /// </value>
        [ParameterName("time.init"), ParameterName("time.initialize"), ParameterInfo("Work distributor for initializing time-dependent quantities")]
        public IWorkDistributor TimeInitDistributor { get; set; }

        /// <summary>
        /// Gets or sets the probe distributor.
        /// </summary>
        /// <value>
        /// The probe distributor.
        /// </value>
        [ParameterName("time.probe"), ParameterInfo("Work distributor for probing a new timepoint")]
        public IWorkDistributor ProbeDistributor { get; set; }

        /// <summary>
        /// Gets or sets the accept distributor.
        /// </summary>
        /// <value>
        /// The accept distributor.
        /// </value>
        [ParameterName("time.accept"), ParameterInfo("Work distributor for accepting a timepoint")]
        public IWorkDistributor AcceptDistributor { get; set; }

        /// <summary>
        /// Gets or sets the noise distributor.
        /// </summary>
        /// <value>
        /// The noise distributor.
        /// </value>
        [ParameterName("noise.noise"), ParameterInfo("Work distributor for calculating noise contributions")]
        public IWorkDistributor NoiseDistributor { get; set; }
    }
}
