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
        /// <value>
        ///   <c>true</c> if the simulation should go straight to gmin stepping; otherwise, <c>false</c>.
        /// </value>
        public bool NoOperatingPointIterations { get; set; } = false;

        /// <summary>
        /// Gets or sets the minimum conductance.
        /// </summary>
        /// <value>
        /// The minimum conductance.
        /// </value>
        /// <remarks>
        /// Convergence is mainly an issue with semiconductor junctions, which often lead to exponential curves. Exponential dependencies
        /// are very harsh on convergence. A lower Gmin will cause iterations to converge faster, but to a (slightly) wrong value. By
        /// steadily relaxing this value back to 0 it is possible to progressively reach a solution without having non-convergence.
        /// </remarks>
        public double Gmin { get; set; } = 1e-12;

        /// <summary>
        /// Gets or sets the number of steps to use when using gmin stepping to improve convergence.
        /// </summary>
        /// <value>
        /// The number of gmin steps.
        /// </value>
        public int GminSteps { get; set; } = 10;

        /// <summary>
        /// Gets or sets the number of steps when using source stepping to improve convergence.
        /// </summary>
        /// <value>
        /// The number of steps.
        /// </value>
        /// <remarks>
        /// In source stepping, all sources are considered to be at 0 which has typically only one single solution (all nodes and
        /// currents are 0V and 0A). By increasing the source factor in small steps, it is possible to progressively reach a solution
        /// without having non-convergence.
        /// </remarks>
        public int SourceSteps { get; set; } = 10;

        /// <summary>
        /// Gets or sets the allowed relative tolerance.
        /// </summary>
        /// <value>
        /// The relative tolerance.
        /// </value>
        public double RelativeTolerance { get; set; } = 1e-3;

        /// <summary>
        /// Gets or sets the absolute tolerance on voltages.
        /// </summary>
        /// <value>
        /// The voltage tolerance.
        /// </value>
        public double VoltageTolerance { get; set; } = 1e-6;

        /// <summary>
        /// Gets or sets the absolute tolerance.
        /// </summary>
        /// <value>
        /// The absolute tolerance.
        /// </value>
        public double AbsoluteTolerance { get; set; } = 1e-12;

        /// <summary>
        /// Gets or sets the tolerance on charges.
        /// </summary>
        /// <value>
        /// The charge tolerance.
        /// </value>
        public double ChargeTolerance { get; set; } = 1e-14;

        /// <summary>
        /// Gets or sets the maximum number of iterations for operating point simulation.
        /// </summary>
        /// <value>
        /// The maximum amount of iterations.
        /// </value>
        public int DcMaxIterations { get; set; } = 100;
    }
}
