using System.Diagnostics;
using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations.Biasing
{
    /// <summary>
    /// Statistics for a <see cref="BiasingSimulationStatistics" />.
    /// </summary>
    public class BiasingSimulationStatistics
    {
        /// <summary>
        /// Gets the total number of iterations.
        /// </summary>
        /// <value>
        /// The total number of iterations.
        /// </value>
        [ParameterName("iterations"), ParameterName("niter"), ParameterInfo("The total number of iterations")]
        public int Iterations { get; set; }

        /// <summary>
        /// Gets a stopwatch that keeps the total time spent solving equations.
        /// </summary>
        /// <value>
        /// The time spent solving the equations.
        /// </value>
        [ParameterName("tsolve"), ParameterInfo("The time spent solving equations")]
        public Stopwatch SolveTime { get; } = new Stopwatch();

        /// <summary>
        /// Gets a stopwatch that keeps the total time spent loading the equation matrix.
        /// </summary>
        /// <value>
        /// The time spent computing contributions and loading the matrix and right hand side vector.
        /// </value>
        [ParameterName("tload"), ParameterInfo("The time spent loading the equation matrix")]
        public Stopwatch LoadTime { get; } = new Stopwatch();

        /// <summary>
        /// Gets a stopwatch that keeps the total time spent reordering the equation matrix.
        /// </summary>
        /// <value>
        /// The time spent reordering the solver.
        /// </value>
        [ParameterName("treorder"), ParameterInfo("The time spent reordering the equation matrix")]
        public Stopwatch ReorderTime { get; } = new Stopwatch();

        /// <summary>
        /// Gets a stopwatch that keeps the total time spent on decomposition of the matrix.
        /// </summary>
        /// <value>
        /// The time spent factoring the matrix.
        /// </value>
        [ParameterName("tfactor"), ParameterInfo("The time spent on factoring the equation matrix")]
        public Stopwatch FactoringTime { get; } = new Stopwatch();

        /// <summary>
        /// Reset simulation statistics.
        /// </summary>
        public void Reset()
        {
            Iterations = 0;
            SolveTime.Reset();
            LoadTime.Reset();
            ReorderTime.Reset();
            FactoringTime.Reset();
        }
    }
}
