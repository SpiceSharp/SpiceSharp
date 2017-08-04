using SpiceSharp.Simulations;
using SpiceSharp.Parameters;

namespace SpiceSharp
{
    /// <summary>
    /// A class with everything needed to do a simulation
    /// </summary>
    public abstract class Simulation : Parameterized
    {
        /// <summary>
        /// The configuration
        /// </summary>
        public SimulationConfiguration Config { get; set; } = null;

        /// <summary>
        /// Event that is called when new simulation data is available
        /// </summary>
        public event ExportSimulationDataEventHandler ExportSimulationData;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">The configuration for this simulation</param>
        public Simulation(string name, SimulationConfiguration config = null)
            : base(name)
        {
            Config = config;
        }

        /// <summary>
        /// Execute the simulation
        /// </summary>
        /// <param name="ckt">The circuit to be used</param>
        /// <param name="reset">Restart the simulation when true</param>
        public abstract void Execute(Circuit ckt);

        /// <summary>
        /// Export the data
        /// </summary>
        /// <param name="ckt"></param>
        public virtual void Export(Circuit ckt)
        {
            SimulationData data = new SimulationData(ckt);
            ExportSimulationData?.Invoke(this, data);
        }
    }
}
