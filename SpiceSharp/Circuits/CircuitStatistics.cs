using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharp.Circuits
{
    public class CircuitStatistics
    {
        /// <summary>
        /// Get the total number of iterations
        /// </summary>
        public int NumIter { get; set; } = 0;

        /// <summary>
        /// The total time spent solving equations
        /// </summary>
        public Stopwatch SolveTime { get; } = new Stopwatch();

        /// <summary>
        /// The total time spent loading the equation matrix
        /// </summary>
        public Stopwatch LoadTime { get; } = new Stopwatch();
    }
}
