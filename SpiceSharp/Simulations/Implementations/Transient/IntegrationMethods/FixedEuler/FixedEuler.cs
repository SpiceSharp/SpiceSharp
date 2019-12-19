using SpiceSharp.Attributes;
using System.Collections.Generic;

namespace SpiceSharp.Simulations.IntegrationMethods
{
    /// <summary>
    /// Implements backward Euler integration with a fixed timesep. This is the
    /// fastest way, but also the least accurate. Any changes to the timestep are
    /// ignored.
    /// </summary>
    /// <seealso cref="IIntegrationMethodDescription" />
    public partial class FixedEuler : ParameterSet, IIntegrationMethodDescription
    {
        /// <summary>
        /// Gets or sets the maximum timestep.
        /// </summary>
        /// <value>
        /// The maximum timestep.
        /// </value>
        [ParameterName("tmax"), ParameterInfo("The maximum timestep.")]
        public double MaxStep
        {
            get
            {
                if (_maxStep <= 0.0)
                    return (StopTime - StartTime) / 50.0;
                return _maxStep;
            }
            set => _maxStep = value;
        }
        private double _maxStep = 0.0;

        /// <summary>
        /// Gets or sets the expansion factor for timesteps.
        /// </summary>
        /// <value>
        /// The expansion factor.
        /// </value>
        [ParameterName("expansion"), ParameterInfo("The maximum expansion factor for consecutive timesteps.")]
        public double Expansion { get; set; } = 2.0;

        /// <summary>
        /// Gets or sets the minimum timestep.
        /// </summary>
        /// <value>
        /// The minimum timestep.
        /// </value>
        [ParameterName("tmin"), ParameterInfo("The minimum timestep.")]
        public double MinStep => 1e-9 * MaxStep;

        /// <summary>
        /// Gets or sets the initial timestep.
        /// </summary>
        /// <value>
        /// The initial timestep.
        /// </value>
        [ParameterName("step"), ParameterInfo("The initial timestep.")]
        public double InitialStep { get; set; }

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>
        /// The start time.
        /// </value>
        [ParameterName("tstart"), ParameterName("start"), ParameterInfo("The time to start exporting data.")]
        public double StartTime { get; set; }

        /// <summary>
        /// Gets or sets the stop time.
        /// </summary>
        /// <value>
        /// The stop time.
        /// </value>
        [ParameterName("tstop"), ParameterName("stop"), ParameterInfo("The time to stop exporting data.")]
        public double StopTime { get; set; }

        /// <summary>
        /// Gets or sets the absolute tolerance.
        /// </summary>
        /// <value>
        /// The absolute tolerance.
        /// </value>
        [ParameterName("abstol"), ParameterInfo("The absolute tolerance.")]
        public double AbsTol { get; set; } = 1e-12;

        /// <summary>
        /// The tolerance on charges.
        /// </summary>
        /// <value>
        /// The charge tolerance.
        /// </value>
        [ParameterName("chgtol"), ParameterInfo("The charge tolerance.")]
        public double ChgTol { get; set; } = 1e-14;

        /// <summary>
        /// Gets or sets the relative tolerance.
        /// </summary>
        /// <value>
        /// The relative tolerance.
        /// </value>
        [ParameterName("reltol"), ParameterInfo("The relative tolerance.")]
        public double RelTol { get; set; } = 1e-3;

        /// <summary>
        /// Gets or sets the transient tolerance factor.
        /// </summary>
        /// <value>
        /// The transient tolerance factor.
        /// </value>
        [ParameterName("trtol"), ParameterInfo("The transient tolerance factor.")]
        public double TrTol { get; set; } = 7.0;

        /// <summary>
        /// The local truncation error relative tolerance.
        /// </summary>
        /// <value>
        /// The relative tolerance.
        /// </value>
        [ParameterName("ltereltol"), ParameterInfo("The local truncation error relative tolerance.")]
        public double LteRelTol { get; set; } = 1e-3;

        /// <summary>
        /// The local truncation error absolute tolerance.
        /// </summary>
        /// <value>
        /// The aboslute tolerance.
        /// </value>
        [ParameterName("lteabstol"), ParameterInfo("The local truncation error absolute tolerance.")]
        public double LteAbsTol { get; set; } = 1e-6;

        /// <summary>
        /// Gets or sets a value indicating whether initial conditions should be used.
        /// </summary>
        /// <value>
        ///   <c>true</c> if initial conditions should be used; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("useic"), ParameterInfo("Flag indicating whether initial conditions should be determined by the components.")]
        public bool UseIc { get; set; }
        // TODO: This shouldn't really be a part of integration methods...

        /// <summary>
        /// Gets or sets the maximum transient iterations.
        /// </summary>
        /// <value>
        /// The maximum transient iterations.
        /// </value>
        public int TranMaxIterations { get; set; } = 10;

        /// <summary>
        /// Gets the initial conditions.
        /// </summary>
        /// <value>
        /// The initial conditions.
        /// </value>
        public Dictionary<string, double> InitialConditions { get; } = new Dictionary<string, double>();

        /// <summary>
        /// Creates an instance of the integration method for an associated <see cref="IBiasingSimulationState" />.
        /// </summary>
        /// <param name="simulation">The simulation that provides the biasing state.</param>
        /// <returns>
        /// The integration method.
        /// </returns>
        public IIntegrationMethod Create(IStateful<IBiasingSimulationState> simulation)
            => new Instance(this, simulation);
    }
}
