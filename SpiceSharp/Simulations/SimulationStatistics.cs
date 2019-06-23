using System.Diagnostics;
using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Statistics for a <see cref="Simulation" />.
    /// </summary>
    public class SimulationStatistics : Statistics
    {
        /// <summary>
        /// Gets the time spent during setup.
        /// </summary>
        [ParameterName("tsetup"), ParameterInfo("The time spent during setup")]
        public Stopwatch SetupTime { get; } = new Stopwatch();

        /// <summary>
        /// Gets the time spent during execution.
        /// </summary>
        [ParameterName("texecution"), ParameterName("texec"), ParameterInfo("Time spent during execution")]
        public Stopwatch ExecutionTime { get; } = new Stopwatch();

        /// <summary>
        /// Gets the time spent during unsetup.
        /// </summary>
        [ParameterName("tunsetup"), ParameterInfo("Time spent during unsetup")]
        public Stopwatch UnsetupTime { get; } = new Stopwatch();

        /// <summary>
        /// Gets the time spent creating behaviors.
        /// </summary>
        [ParameterName("tbehavior"), ParameterInfo("Time spent creating behaviors")]
        public Stopwatch BehaviorCreationTime { get; } = new Stopwatch();

        /// <summary>
        /// Clear all statistics
        /// </summary>
        public override void Reset()
        {
            SetupTime.Reset();
            ExecutionTime.Reset();
            UnsetupTime.Reset();
            BehaviorCreationTime.Reset();
        }
    }
}
