using SpiceSharp.Attributes;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A configuration for a <see cref="ITimeSimulation"/> with all the necessary parameters to do a transient analysis.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public abstract class TimeConfiguration : ParameterSet
    {
        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>
        /// The start time.
        /// </value>
        /// <exception cref="ArgumentException">Thrown if the timepoint is negative.</exception>
        [ParameterName("tstart"), ParameterName("t0"), ParameterInfo("The initial timepoint to start exporting data.")]
        public double StartTime
        {
            get => _start;
            set
            {
                if (value < 0.0)
                    throw new ArgumentException(Properties.Resources.Simulations_Time_TimeTooSmall);
                _start = value;
            }
        }
        private double _start;

        /// <summary>
        /// Gets or sets the stop time.
        /// </summary>
        /// <value>
        /// The stop time.
        /// </value>
        /// <exception cref="ArgumentException">Thrown if the timepoint is negative.</exception>
        [ParameterName("tstop"), ParameterInfo("The final timepoint.")]
        public double StopTime
        {
            get => _stop;
            set
            {
                if (value < 0.0)
                    throw new ArgumentException(Properties.Resources.Simulations_Time_TimeTooSmall);
                _stop = value;
            }
        }
        private double _stop;

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
        public int TransientMaxIterations
        {
            get => _maxIterations;
            set
            {
                if (value < 1)
                    throw new ArgumentException(Properties.Resources.Simulations_IterationsTooSmall);
                _maxIterations = value;
            }
        }
        private int _maxIterations = 10;

        /// <summary>
        /// Gets the initial conditions.
        /// </summary>
        /// <value>
        /// The initial conditions.
        /// </value>
        public Dictionary<string, double> InitialConditions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeConfiguration"/> class.
        /// </summary>
        public TimeConfiguration()
        {
            InitialConditions = new Dictionary<string, double>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeConfiguration"/> class.
        /// </summary>
        /// <param name="ic">The initial conditions.</param>
        public TimeConfiguration(Dictionary<string, double> ic)
        {
            InitialConditions = ic.ThrowIfNull(nameof(ic));
        }

        /// <summary>
        /// Creates an instance of the integration method.
        /// </summary>
        /// <param name="simulation">The simulation that provides the biasing state.</param>
        /// <returns>
        /// The integration method.
        /// </returns>
        public abstract IIntegrationMethod Create(IStateful<IBiasingSimulationState> simulation);
    }
}
