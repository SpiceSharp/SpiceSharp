using System;
using System.Diagnostics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Keeps track of simulation statistics.
    /// </summary>
    public class Statistics
    {
        /// <summary>
        /// Gets the total number of iterations
        /// </summary>
        public int Iterations { get; set; } = 0;

        /// <summary>
        /// The total time spent solving equations
        /// </summary>
        public Stopwatch SolveTime { get; } = new Stopwatch();

        /// <summary>
        /// The total time spent loading the equation matrix
        /// </summary>
        public Stopwatch LoadTime { get; } = new Stopwatch();

        /// <summary>
        /// The total time spent reordering the equation matrix
        /// </summary>
        public Stopwatch ReorderTime { get; } = new Stopwatch();

        /// <summary>
        /// The total time spent on decomposition of the matrix
        /// </summary>
        public Stopwatch DecompositionTime { get; } = new Stopwatch();

        /// <summary>
        /// The total time spent on creating behaviors for each device
        /// </summary>
        public Stopwatch BehaviorCreationTime { get; } = new Stopwatch();

        /// <summary>
        /// Gets or sets the number of timepoints calculated
        /// </summary>
        public int TimePoints { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total number of transient iterations
        /// </summary>
        public int TransientIterations { get; set; } = 0;

        /// <summary>
        /// The total time spent executing transient simulations
        /// </summary>
        public Stopwatch TransientTime { get; } = new Stopwatch();

        /// <summary>
        /// Gets or sets the total solving time during transient simulations
        /// </summary>
        public TimeSpan TransientSolveTime { get; set; } = new TimeSpan();

        /// <summary>
        /// Gets the number of accepted timepoints
        /// </summary>
        public int Accepted { get; set; } = 0;

        /// <summary>
        /// Gets the number of rejected timepoints
        /// </summary>
        public int Rejected { get; set; } = 0;

        /// <summary>
        /// Clear the statistics
        /// </summary>
        public void Clear()
        {
            Iterations = 0;
            SolveTime.Reset();
            LoadTime.Reset();
            TimePoints = 0;
            TransientIterations = 0;
            TransientTime.Reset();
            TransientSolveTime = TimeSpan.Zero;
            Accepted = 0;
            Rejected = 0;
        }
    }
}
