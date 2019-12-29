using System;
using System.Diagnostics;
using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations.Time
{
    /// <summary>
    /// Statistics for a <see cref="TimeSimulationStatistics" />.
    /// </summary>
    public class TimeSimulationStatistics
    {
        /// <summary>
        /// Gets the number of time points calculated.
        /// </summary>
        [ParameterName("ntime"), ParameterInfo("The number of time points")]
        public int TimePoints { get; set; }

        /// <summary>
        /// Gets the number of transient iterations.
        /// </summary>
        [ParameterName("ntran"), ParameterName("ntransient"), ParameterInfo("The number of transient iterations")]
        public int TransientIterations { get; set; }

        /// <summary>
        /// Gets the time spent on transient analysis.
        /// </summary>
        [ParameterName("ttran"), ParameterInfo("The time spent on doing a transient analysis")]
        public Stopwatch TransientTime { get; } = new Stopwatch();

        /// <summary>
        /// Gets the time spent on solving a transient analysis.
        /// </summary>
        [ParameterName("tsolvetran"), ParameterInfo("The time spent on solving a transient analysis")]
        public TimeSpan TransientSolveTime { get; set; }

        /// <summary>
        /// Gets the number of accepted time points.
        /// </summary>
        [ParameterName("accepted"), ParameterName("naccepted"), ParameterInfo("The number of accepted time points")]
        public int Accepted { get; set; }

        /// <summary>
        /// Gets the number of rejected time points.
        /// </summary>
        [ParameterName("rejected"), ParameterName("nrejected"), ParameterInfo("The number of rejected time points")]
        public int Rejected { get; set; }

        /// <summary>
        /// Reset the statistics.
        /// </summary>
        public void Reset()
        {
            TimePoints = 0;
            TransientIterations = 0;
            TransientTime.Reset();
            TransientSolveTime = TimeSpan.Zero;
            Accepted = 0;
            Rejected = 0;
        }
    }
}
