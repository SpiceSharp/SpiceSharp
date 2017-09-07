using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Configuration for simulations
    /// </summary>
    public class SimulationConfiguration
    {
        /// <summary>
        /// Get the default simulation configuration
        /// </summary>
        public static SimulationConfiguration Default { get; } = new SimulationConfiguration();

        /// <summary>
        /// If true, the operating point calculation immediately skips to the GMIN stepping phase
        /// </summary>
        public bool NoOpIter = false;

        /// <summary>
        /// The GMIN parameter
        /// This is an extra conductance added in parallel to PN junction to improve convergence
        /// </summary>
        public double Gmin = 1e-12;

        /// <summary>
        /// The number of steps when using GMIN stepping to improve convergence
        /// </summary>
        public int NumGminSteps = 10;

        /// <summary>
        /// The number of steps when using SOURCE stepping to improve convergence
        /// </summary>
        public int NumSrcSteps = 10;

        /// <summary>
        /// Relative tolerance
        /// </summary>
        public double RelTol = 1e-3;

        /// <summary>
        /// Absolute tolerance on voltages
        /// </summary>
        public double VoltTol = 1e-6;

        /// <summary>
        /// Absolute tolerance
        /// </summary>
        public double AbsTol = 1e-12;

        /// <summary>
        /// Number of iterations for DC simulation
        /// </summary>
        public int DcMaxIterations = 50;

        /// <summary>
        /// Number of iterations for DC sweeps
        /// </summary>
        public int SweepMaxIterations = 20;

        /// <summary>
        /// Maximum number of iterations for each time point
        /// </summary>
        public int TranMaxIterations = 10;

        /// <summary>
        /// Use initial conditions
        /// </summary>
        public bool UseIC = false;

        /// <summary>
        /// Keep operating point information
        /// </summary>
        public bool KeepOpInfo = false;

        /// <summary>
        /// Integration method
        /// </summary>
        public IntegrationMethod Method = new IntegrationMethods.Trapezoidal();

        /// <summary>
        /// Simulation configuration
        /// </summary>
        /// <param name="b"></param>
        public SimulationConfiguration()
        {
        }
    }
}
