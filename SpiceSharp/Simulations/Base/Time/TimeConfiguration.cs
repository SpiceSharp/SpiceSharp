using System.Collections.Generic;
using SpiceSharp.Attributes;
using SpiceSharp.IntegrationMethods;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Configuration for a <see cref="TimeConfiguration" />
    /// </summary>
    /// <seealso cref="SpiceSharp.ParameterSet" />
    public class TimeConfiguration : ParameterSet
    {
        /// <summary>
        /// Gets or sets the integration method that needs to be used.
        /// </summary>
        public IntegrationMethod Method { get; set; } = new Trapezoidal();

        /// <summary>
        /// Gets or sets the initial timepoint that should be exported.
        /// </summary>
        [ParameterName("init"), ParameterName("start"), ParameterInfo("The starting timepoint")]
        public double InitTime { get; set; } = 0.0;

        /// <summary>
        /// Gets or sets the final simulation timepoint.
        /// </summary>
        [ParameterName("final"), ParameterName("stop"), ParameterInfo("The final timepoint")]
        public double FinalTime { get; set; } = double.NaN;

        /// <summary>
        /// Gets or sets the step size.
        /// </summary>
        [ParameterName("step"), ParameterInfo("The timestep")]
        public double Step { get; set; } = double.NaN;

        /// <summary>
        /// Gets or sets the maximum timestep.
        /// </summary>
        [ParameterName("maxstep"), ParameterInfo("The maximum allowed timestep")]
        public double MaxStep
        {
            get
            {
                if (double.IsNaN(_maxstep))
                    return (FinalTime - InitTime) / 50.0;
                return _maxstep;
            }
            set => _maxstep = value;
        }

        private double _maxstep = double.NaN;

        /// <summary>
        /// Gets the minimum timestep allowed.
        /// </summary>
        [ParameterName("deltamin"), ParameterInfo("The minimum delta for breakpoints")]
        public double DeltaMin => 1e-9 * MaxStep;

        /// <summary>
        /// Gets or sets the maximum number of iterations allowed for each time point.
        /// </summary>
        public int TranMaxIterations { get; set; } = 10;

        /// <summary>
        /// Use initial conditions.
        /// </summary>
        public bool UseIc { get; set; } = false;

        /// <summary>
        /// Gets the initial conditions.
        /// </summary>
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
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing initial condition node names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        public TimeConfiguration(IEqualityComparer<string> comparer)
        {
            InitialConditions = new Dictionary<string, double>(comparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeConfiguration"/> class.
        /// </summary>
        /// <param name="step">The step size.</param>
        /// <param name="final">The final time.</param>
        public TimeConfiguration(double step, double final)
            : this()
        {
            Step = step;
            FinalTime = final;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeConfiguration"/> class.
        /// </summary>
        /// <param name="step">The step size.</param>
        /// <param name="final">The final time.</param>
        /// <param name="max">The maximum timestep.</param>
        public TimeConfiguration(double step, double final, double max)
            : this()
        {
            Step = step;
            FinalTime = final;
            MaxStep = max;
        }
    }
}
