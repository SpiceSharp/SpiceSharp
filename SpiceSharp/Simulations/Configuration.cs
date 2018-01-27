using SpiceSharp.IntegrationMethods;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Configuration for simulations
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Get the default simulation configuration
        /// </summary>
        public static Configuration Default { get; } = new Configuration();

        /// <summary>
        /// The GMIN parameter
        /// This is an extra conductance added in parallel to PN junction to improve convergence
        /// </summary>
        public double Gmin { get; set; } = 1e-12;

        /// <summary>
        /// The number of steps when using GMIN stepping to improve convergence
        /// </summary>
        public int NumGminSteps { get; set; } = 10;

        /// <summary>
        /// The number of steps when using SOURCE stepping to improve convergence
        /// </summary>
        public int NumSrcSteps { get; set; } = 10;

        /// <summary>
        /// Relative tolerance
        /// </summary>
        public double RelTol { get; set; } = 1e-3;

        /// <summary>
        /// Absolute tolerance on voltages
        /// </summary>
        public double VoltTol { get; set; } = 1e-6;

        /// <summary>
        /// Absolute tolerance
        /// </summary>
        public double AbsTol { get; set; } = 1e-12;

        /// <summary>
        /// Charge tolerance
        /// </summary>
        public double ChgTol { get; set; } = 1e-14;

        /// <summary>
        /// Number of iterations for DC simulation
        /// </summary>
        public int DcMaxIterations { get; set; } = 100;

        /// <summary>
        /// Number of iterations for DC sweeps
        /// </summary>
        public int SweepMaxIterations { get; set; } = 20;

        /// <summary>
        /// Maximum number of iterations for each time point
        /// </summary>
        public int TranMaxIterations { get; set; } = 10;

        /// <summary>
        /// Use initial conditions
        /// </summary>
        public bool UseIC { get; set; } = false;

        /// <summary>
        /// Keep operating point information
        /// </summary>
        public bool KeepOpInfo { get; set; } = false;

        /// <summary>
        /// Integration method
        /// </summary>
        public IntegrationMethod Method { get; set; } = new Trapezoidal();
    }
}
