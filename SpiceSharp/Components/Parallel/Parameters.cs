using SpiceSharp.Attributes;
using SpiceSharp.Entities;

namespace SpiceSharp.Components.ParallelComponents
{
    /// <summary>
    /// Base parameters for a <see cref="ParallelComponents"/>.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class Parameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the entities that should be run in parallel.
        /// </summary>
        /// <value>
        /// The entities.
        /// </value>
        public IEntityCollection Entities { get; set; }

        /// <summary>
        /// Gets or sets the workload distributor for temperature-dependent calculations.
        /// </summary>
        /// <value>
        /// The workload distributor.
        /// </value>
        [ParameterName("temperature"), ParameterInfo("Workload distributor for temperature-dependent calculations.")]
        public IWorkDistributor TemperatureDistributor { get; set; }

        /// <summary>
        /// Gets or sets the workload distributor for loading in biasing simulations.
        /// </summary>
        /// <value>
        /// The workload distributor.
        /// </value>
        [ParameterName("biasing.load"), ParameterInfo("Work distributor for loading the Y-matrix and Rhs-vector")]
        public IWorkDistributor BiasLoadDistributor { get; set; }

        /// <summary>
        /// Gets or sets the workload distributor for testing convergence in biasing simulations.
        /// </summary>
        /// <value>
        /// The workload distributor.
        /// </value>
        [ParameterName("biasing.convergence"), ParameterInfo("Workload distributor for testing convergence")]
        public IWorkDistributor<bool> BiasConvergenceDistributor { get; set; }

        /// <summary>
        /// Gets or sets the workload distributor for updating a behavior after solving.
        /// </summary>
        /// <value>
        /// The biasing update distributor.
        /// </value>
        [ParameterName("biasing.update"), ParameterInfo("Workload distributor for updating a behavior after solving")]
        public IWorkDistributor BiasUpdateDistributor { get; set; }

        /// <summary>
        /// Gets or sets the workload distributor for loading the small-signal contributions.
        /// </summary>
        /// <value>
        /// The workload distributor.
        /// </value>
        [ParameterName("frequency.load"), ParameterInfo("Work distributor for loading the small-signal contributions")]
        public IWorkDistributor AcLoadDistributor { get; set; }

        /// <summary>
        /// Gets or sets the workload distributor for initializing small-signal parameters.
        /// </summary>
        /// <value>
        /// The workload distributor.
        /// </value>
        [ParameterName("frequency.init"), ParameterName("frequency.initialize"), ParameterInfo("Work distributor for initializing small-signal parameters")]
        public IWorkDistributor AcInitDistributor { get; set; }

        /// <summary>
        /// Gets or sets workload distributor for initializing time-dependent parameters.
        /// </summary>
        /// <value>
        /// The workload distributor.
        /// </value>
        [ParameterName("time.init"), ParameterName("time.initialize"), ParameterInfo("Work distributor for initializing time-dependent quantities")]
        public IWorkDistributor TimeInitDistributor { get; set; }

        /// <summary>
        /// Gets or sets the workload distributor for probing a new timepoint.
        /// </summary>
        /// <value>
        /// The workload distributor.
        /// </value>
        [ParameterName("time.probe"), ParameterInfo("Work distributor for probing a new timepoint")]
        public IWorkDistributor ProbeDistributor { get; set; }

        /// <summary>
        /// Gets or sets the workload distributor for accepting a timepoint.
        /// </summary>
        /// <value>
        /// The workload distributor.
        /// </value>
        [ParameterName("time.accept"), ParameterInfo("Work distributor for accepting a timepoint")]
        public IWorkDistributor AcceptDistributor { get; set; }

        /// <summary>
        /// Gets or sets the workload distributor for noise computations.
        /// </summary>
        /// <value>
        /// The workload distributor for noise computations.
        /// </value>
        [ParameterName("noise.compute"), ParameterInfo("Work distributor for computing noise contributions")]
        public IWorkDistributor NoiseComputeDistributor { get; set; }

        /// <summary>
        /// Gets or sets the workload distributor for noise initialization.
        /// </summary>
        /// <value>
        /// The workload distributor for noise initialization.
        /// </value>
        [ParameterName("noise.initialize"), ParameterInfo("Work distributor for initializing noise sources")]
        public IWorkDistributor NoiseInitializeDistributor { get; set; }
    }
}
