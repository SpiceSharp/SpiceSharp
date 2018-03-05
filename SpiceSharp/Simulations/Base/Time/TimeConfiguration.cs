using SpiceSharp.Attributes;
using SpiceSharp.IntegrationMethods;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Configuration for a <see cref="TimeConfiguration"/>
    /// </summary>
    public class TimeConfiguration : ParameterSet
    {
        /// <summary>
        /// Gets or sets the integration method
        /// </summary>
        public IntegrationMethod Method { get; set; } = new Trapezoidal();

        /// <summary>
        /// Gets or sets the initial timepoint that should be exported
        /// </summary>
        [PropertyName("init"), PropertyName("start"), PropertyInfo("The starting timepoint")]
        public double InitTime { get; set; } = 0.0;

        /// <summary>
        /// Gets or sets the final simulation timepoint
        /// </summary>
        [PropertyName("final"), PropertyName("stop"), PropertyInfo("The final timepoint")]
        public double FinalTime { get; set; } = double.NaN;

        /// <summary>
        /// Gets or sets the step
        /// </summary>
        [PropertyName("step"), PropertyInfo("The timestep")]
        public double Step { get; set; } = double.NaN;

        /// <summary>
        /// Gets or sets the maximum timestep
        /// </summary>
        [PropertyName("maxstep"), PropertyInfo("The maximum allowed timestep")]
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
        /// Gets the minimum timestep allowed
        /// </summary>
        [PropertyName("deltamin"), PropertyInfo("The minimum delta for breakpoints")]
        public double DeltaMin => 1e-13 * MaxStep;

        /// <summary>
        /// Maximum number of iterations for each time point
        /// </summary>
        public int TranMaxIterations { get; set; } = 10;

        /// <summary>
        /// Use initial conditions
        /// </summary>
        public bool UseIc { get; set; } = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public TimeConfiguration()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="step"></param>
        /// <param name="stop"></param>
        public TimeConfiguration(double step, double stop)
        {
            Step = step;
            FinalTime = stop;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="step">Step</param>
        /// <param name="stop">Stop</param>
        /// <param name="max">Maximum timestep</param>
        public TimeConfiguration(double step, double stop, double max)
        {
            Step = step;
            FinalTime = stop;
            MaxStep = max;
        }
    }
}
