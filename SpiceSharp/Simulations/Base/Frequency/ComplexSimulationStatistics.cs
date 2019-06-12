using System.Diagnostics;
using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Statistics for a <see cref="FrequencySimulation"/>.
    /// </summary>
    public class ComplexSimulationStatistics : Statistics
    {
        /// <summary>
        /// Gets or sets the number of frequency points calculated.
        /// </summary>
        [ParameterName("ncomplex"), ParameterInfo("Gets the number of calculated frequency points")]
        public int ComplexPoints { get; set; }

        /// <summary>
        /// Gets the time spent on frequency analysis.
        /// </summary>
        [ParameterName("tcomplex"), ParameterInfo("Gets the time spent on frequency analysis")]
        public Stopwatch ComplexTime { get; } = new Stopwatch();

        /// <summary>
        /// Gets the time spent on solving the complex equation matrix.
        /// </summary>
        [ParameterName("tcomplexsolve"), ParameterInfo("Gets the time spent on solving complex equations")]
        public Stopwatch ComplexSolveTime { get; } = new Stopwatch();

        /// <summary>
        /// Gets the time spent on loading the complex equation matrix.
        /// </summary>
        [ParameterName("tcomplexload"), ParameterInfo("Gets the time spent on loading the complex equation matrix")]
        public Stopwatch ComplexLoadTime { get; } = new Stopwatch();

        /// <summary>
        /// Gets the time spent on reordering the complex equation matrix.
        /// </summary>
        [ParameterName("tcomplexreorder"), ParameterInfo("Gets the time spent on reordering the complex equation matrix")]
        public Stopwatch ComplexReorderTime { get; } = new Stopwatch();

        /// <summary>
        /// Gets the time spent on decomposition of the complex equation matrix.
        /// </summary>
        [ParameterName("tcomplexdecomp"), ParameterInfo("Gets the time spent on decomposition of the complex equation matrix")]
        public Stopwatch ComplexDecompositionTime { get; } = new Stopwatch();

        /// <summary>
        /// Reset the statistics.
        /// </summary>
        public override void Reset()
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
