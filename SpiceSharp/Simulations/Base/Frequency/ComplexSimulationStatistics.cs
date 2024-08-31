using System.Diagnostics;
using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations.Frequency
{
    /// <summary>
    /// Statistics for a <see cref="FrequencySimulation"/>.
    /// </summary>
    public class ComplexSimulationStatistics
    {
        /// <summary>
        /// Gets or sets the number of frequency points calculated.
        /// </summary>
        /// <value>
        /// The number of calculated complex points.
        /// </value>
        [ParameterName("ncomplex"), ParameterInfo("Gets the number of calculated frequency points")]
        public int ComplexPoints { get; set; }

        /// <summary>
        /// Gets the time spent on frequency analysis.
        /// </summary>
        /// <value>
        /// The time spent on small-signal analysis.
        /// </value>
        [ParameterName("tcomplex"), ParameterInfo("Gets the time spent on frequency analysis")]
        public Stopwatch ComplexTime { get; } = new Stopwatch();

        /// <summary>
        /// Gets the time spent on solving the complex equation matrix.
        /// </summary>
        /// <value>
        /// The time spent solving complex equations.
        /// </value>
        [ParameterName("tcomplexsolve"), ParameterInfo("Gets the time spent on solving complex equations")]
        public Stopwatch ComplexSolveTime { get; } = new Stopwatch();

        /// <summary>
        /// Gets the time spent on loading the complex equation matrix.
        /// </summary>
        /// <value>
        /// The time spent loading the complex matrix and right hand side vector.
        /// </value>
        [ParameterName("tcomplexload"), ParameterInfo("Gets the time spent on loading the complex equation matrix")]
        public Stopwatch ComplexLoadTime { get; } = new Stopwatch();

        /// <summary>
        /// Gets the time spent on reordering the complex equation matrix.
        /// </summary>
        /// <value>
        /// The time spent reordering the complex matrix.
        /// </value>
        [ParameterName("tcomplexreorder"), ParameterInfo("Gets the time spent on reordering the complex equation matrix")]
        public Stopwatch ComplexReorderTime { get; } = new Stopwatch();

        /// <summary>
        /// Gets the time spent on factoring the complex equation matrix.
        /// </summary>
        /// <value>
        /// The time spent on factoring the complex equation matrix.
        /// </value>
        [ParameterName("tcomplexfactor"), ParameterInfo("Gets the time spent on factoring the complex equation matrix")]
        public Stopwatch ComplexDecompositionTime { get; } = new Stopwatch();

        /// <summary>
        /// Reset the statistics.
        /// </summary>
        public void Reset()
        {
            ComplexPoints = 0;
            ComplexTime.Reset();
            ComplexSolveTime.Reset();
            ComplexLoadTime.Reset();
            ComplexReorderTime.Reset();
            ComplexDecompositionTime.Reset();
        }
    }
}
