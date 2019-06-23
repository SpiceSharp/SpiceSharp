using System.Diagnostics;
using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Statistics for a <see cref="BaseSimulationStatistics" />.
    /// </summary>
    public class BaseSimulationStatistics : Statistics
    {
        /// <summary>
        /// Gets the total number of iterations.
        /// </summary>
        [ParameterName("iterations"), ParameterName("niter"), ParameterInfo("The total number of iterations")]
        public int Iterations { get; set; }

        /// <summary>
        /// Gets a stopwatch that keeps the total time spent solving equations.
        /// </summary>
        [ParameterName("tsolve"), ParameterInfo("The time spent solving equations")]
        public Stopwatch SolveTime { get; } = new Stopwatch();

        /// <summary>
        /// Gets a stopwatch that keeps the total time spent loading the equation matrix.
        /// </summary>
        [ParameterName("tload"), ParameterInfo("The time spent loading the equation matrix")]
        public Stopwatch LoadTime { get; } = new Stopwatch();

        /// <summary>
        /// Gets a stopwatch that keeps the total time spent reordering the equation matrix.
        /// </summary>
        [ParameterName("treorder"), ParameterInfo("The time spent reordering the equation matrix")]
        public Stopwatch ReorderTime { get; } = new Stopwatch();

        /// <summary>
        /// Gets a stopwatch that keeps the total time spent on decomposition of the matrix.
        /// </summary>
        [ParameterName("tdecomposition"), ParameterInfo("The time spent on equation matrix decomposition")]
        public Stopwatch DecompositionTime { get; } = new Stopwatch();

        /// <summary>
        /// Reset simulation statistics.
        /// </summary>
        public override void Reset()
        {
            Iterations = 0;
            SolveTime.Reset();
            LoadTime.Reset();
            ReorderTime.Reset();
            DecompositionTime.Reset();
        }
    }
}
