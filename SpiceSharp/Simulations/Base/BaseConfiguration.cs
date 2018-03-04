namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Configuration for a <see cref="BaseSimulation"/>
    /// </summary>
    public class BaseConfiguration : ParameterSet
    {
        /// <summary>
        /// If true, the operating point calculation immediately skips to the GMIN stepping phase
        /// </summary>
        public bool NoOperatingPointIterations { get; set; } = false;

        /// <summary>
        /// The GMIN parameter
        /// This is an extra conductance added in parallel to PN junction to improve convergence
        /// </summary>
        public double Gmin { get; set; } = 1e-12;

        /// <summary>
        /// The number of steps when using GMIN stepping to improve convergence
        /// </summary>
        public int GminSteps { get; set; } = 10;

        /// <summary>
        /// The number of steps when using SOURCE stepping to improve convergence
        /// </summary>
        public int SourceSteps { get; set; } = 10;

        /// <summary>
        /// Relative tolerance
        /// </summary>
        public double RelativeTolerance { get; set; } = 1e-3;

        /// <summary>
        /// Absolute tolerance on voltages
        /// </summary>
        public double VoltageTolerance { get; set; } = 1e-6;

        /// <summary>
        /// Absolute tolerance
        /// </summary>
        public double AbsoluteTolerance { get; set; } = 1e-12;

        /// <summary>
        /// Charge tolerance
        /// </summary>
        public double ChargeTolerance { get; set; } = 1e-14;

        /// <summary>
        /// Number of iterations for DC simulation
        /// </summary>
        public int DcMaxIterations { get; set; } = 100;
    }
}
