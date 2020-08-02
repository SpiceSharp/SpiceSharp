using SpiceSharp.ParameterSets;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A configuration for a <see cref="ITimeSimulation"/> with all the necessary parameters to do a transient analysis.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public abstract class TimeParameters : ParameterSet
    {
        private int _transientMaxIterations = 10;
        private double _stopTime;
        private double _startTime;

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>
        /// The start time.
        /// </value>
        /// <exception cref="ArgumentException">Thrown if the timepoint is negative.</exception>
        [ParameterName("tstart"), ParameterName("t0"), ParameterInfo("The initial timepoint to start exporting data.")]
        [GreaterThanOrEquals(0)]
        public double StartTime
        {
            get => _startTime;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(StartTime), 0);
                _startTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the stop time.
        /// </summary>
        /// <value>
        /// The stop time.
        /// </value>
        /// <exception cref="ArgumentException">Thrown if the timepoint is negative.</exception>
        [ParameterName("tstop"), ParameterInfo("The final timepoint.")]
        [GreaterThanOrEquals(0)]
        public double StopTime
        {
            get => _stopTime;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(StopTime), 0);
                _stopTime = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether initial conditions should be set by the entities.
        /// </summary>
        /// <value>
        ///   <c>true</c> if initial conditions; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("uic"), ParameterInfo("A flag indicating that entities should set their own initial conditions.")]
        public bool UseIc { get; set; }

        /// <summary>
        /// Gets or sets the transient maximum iterations.
        /// </summary>
        /// <value>
        /// The transient maximum iterations.
        /// </value>
        [ParameterName("itl4"), ParameterInfo("The maximum number of transient timepoint iterations.")]
        [GreaterThan(0)]
        public int TransientMaxIterations
        {
            get => _transientMaxIterations;
            set
            {
                Utility.GreaterThan(value, nameof(TransientMaxIterations), 0);
                _transientMaxIterations = value;
            }
        }

        /// <summary>
        /// Gets the initial conditions.
        /// </summary>
        /// <value>
        /// The initial conditions.
        /// </value>
        public Dictionary<string, double> InitialConditions { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the simulation should be validated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the simulation should be validated; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("time.validate"), ParameterInfo("Flag indicating whether the simulation should validate the circuit before executing")]
        public bool Validate { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeParameters"/> class.
        /// </summary>
        protected TimeParameters()
        {
            InitialConditions = new Dictionary<string, double>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeParameters"/> class.
        /// </summary>
        /// <param name="ic">The initial conditions.</param>
        protected TimeParameters(Dictionary<string, double> ic)
        {
            InitialConditions = ic.ThrowIfNull(nameof(ic));
        }

        /// <summary>
        /// Creates an instance of the integration method.
        /// </summary>
        /// <param name="state">The biasing simulation state that will be used as a base.</param>
        /// <returns>
        /// The integration method.
        /// </returns>
        public abstract IIntegrationMethod Create(IBiasingSimulationState state);
    }
}
