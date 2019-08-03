using System.Collections.Generic;
using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A configuration for a <see cref="BaseSimulation" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.ParameterSet" />
    public class BaseConfiguration : ParameterSet
    {
        /// <summary>
        /// Gets or sets a value indicating whether the simulation should go straight to gmin stepping.
        /// </summary>
        [ParameterName("noopiter"), ParameterInfo("Skip immediately to gmin stepping.")]
        public bool NoOperatingPointIterations { get; set; } = false;

        /// <summary>
        /// Gets or sets the minimum conductance.
        /// </summary>
        /// <remarks>
        /// Convergence is mainly an issue with semiconductor junctions, which often lead to exponential curves. Exponential dependencies
        /// are very harsh on convergence. A lower Gmin will cause iterations to converge faster, but to a (slightly) wrong value. By
        /// steadily relaxing this value back to 0 it is possible to progressively reach a solution without having non-convergence.
        /// </remarks>
        [ParameterName("gmin"), ParameterInfo("A minimum conductance for helping convergence.")]
        public double Gmin { get; set; } = 1e-12;

        /// <summary>
        /// Gets or sets the number of steps to use when using gmin stepping to improve convergence.
        /// </summary>
        [ParameterName("gminsteps"), ParameterInfo("The number of steps used for gmin stepping.")]
        public int GminSteps { get; set; } = 10;

        /// <summary>
        /// Gets or sets the number of steps when using source stepping to improve convergence.
        /// </summary>
        /// <remarks>
        /// In source stepping, all sources are considered to be at 0 which has typically only one single solution (all nodes and
        /// currents are 0V and 0A). By increasing the source factor in small steps, it is possible to progressively reach a solution
        /// without having non-convergence.
        /// </remarks>
        [ParameterName("sourcesteps"), ParameterInfo("The number of steps used for source stepping.")]
        public int SourceSteps { get; set; } = 10;

        /// <summary>
        /// Gets or sets the allowed relative tolerance.
        /// </summary>
        [ParameterName("reltol"), ParameterInfo("The relative error tolerance.")]
        public double RelativeTolerance { get; set; } = 1e-3;

        /// <summary>
        /// Gets or sets the absolute tolerance on voltages.
        /// </summary>
        [ParameterName("vntol"), ParameterInfo("The absolute voltage error tolerance.")]
        public double VoltageTolerance { get; set; } = 1e-6;

        /// <summary>
        /// Gets or sets the absolute tolerance.
        /// </summary>
        [ParameterName("abstol"), ParameterInfo("The absolute error tolerance.")]
        public double AbsoluteTolerance { get; set; } = 1e-12;

        /// <summary>
        /// Gets or sets the tolerance on charges.
        /// </summary>
        [ParameterName("chgtol"), ParameterInfo("The absolute charge error tolerance.")]
        public double ChargeTolerance { get; set; } = 1e-14;

        /// <summary>
        /// Gets or sets the absolute threshold for choosing pivots.
        /// </summary>
        public double AbsolutePivotThreshold { get; set; }

        /// <summary>
        /// Gets or sets the relative threshold for choosing pivots.
        /// </summary>
        public double RelativePivotThreshold { get; set; } = 1e-3;

        /// <summary>
        /// Gets or sets the maximum number of iterations for operating point simulation.
        /// </summary>
        [ParameterName("itl1"), ParameterName("dciter"), ParameterInfo("The DC iteration limit.")]
        public int DcMaxIterations { get; set; } = 100;

        /// <summary>
        /// Gets the nodesets.
        /// </summary>
        public Dictionary<string, double> Nodesets { get; } = new Dictionary<string, double>();
    }
}
