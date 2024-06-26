using System.Diagnostics;
using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Statistics for a <see cref="Simulation" />.
    /// </summary>
    public class SimulationStatistics
    {
        /// <summary>
        /// Gets the time spent during setup.
        /// </summary>
        /// <value>
        /// The setup time.
        /// </value>
        [ParameterName("tsetup"), ParameterInfo("The time spent during setup")]
        public Stopwatch SetupTime { get; } = new Stopwatch();

        /// <summary>
        /// Gets the time spent during validation.
        /// </summary>
        /// <value>
        /// The validation time.
        /// </value>
        [ParameterName("tvalidation"), ParameterInfo("The time spent validating the input")]
        public Stopwatch ValidationTime { get; } = new Stopwatch();

        /// <summary>
        /// Gets the time spent during execution.
        /// </summary>
        /// <value>
        /// The execution time.
        /// </value>
        [ParameterName("texecution"), ParameterName("texec"), ParameterInfo("Time spent during execution")]
        public Stopwatch ExecutionTime { get; } = new Stopwatch();

        /// <summary>
        /// Gets the time spent during unsetup.
        /// </summary>
        /// <value>
        /// The unsetup time.
        /// </value>
        [ParameterName("tfinish"), ParameterInfo("Time spent finishing up")]
        public Stopwatch FinishTime { get; } = new Stopwatch();

        /// <summary>
        /// Gets the time spent creating behaviors.
        /// </summary>
        /// <value>
        /// The behavior creation time.
        /// </value>
        [ParameterName("tbehavior"), ParameterInfo("Time spent creating behaviors")]
        public Stopwatch BehaviorCreationTime { get; } = new Stopwatch();

        /// <summary>
        /// Clear all statistics
        /// </summary>
        public void Reset()
        {
            SetupTime.Reset();
            ValidationTime.Reset();
            ExecutionTime.Reset();
            FinishTime.Reset();
            BehaviorCreationTime.Reset();
        }
    }
}
