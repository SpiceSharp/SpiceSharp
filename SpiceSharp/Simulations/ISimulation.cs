using SpiceSharp.Parameters;

namespace SpiceSharp.Simulations
{
    public interface ISimulation : IParameterized
    {
        /// <summary>
        /// Event that is called for initializing simulation data exports
        /// </summary>
        event InitializeSimulationExportEventHandler InitializeSimulationExport;

        /// <summary>
        /// Event that is called when new simulation data is available
        /// </summary>
        event ExportSimulationDataEventHandler OnExportSimulationData;

        /// <summary>
        /// Event that is called for finalizing simulation data exports
        /// </summary>
        event FinalizeSimulationExportEventHandler FinalizeSimulationExport;

        /// <summary>
        /// Get the configuration for the simulation
        /// </summary>
        SimulationConfiguration Config { get; }

        /// <summary>
        /// Execute the simulation
        /// </summary>
        /// <param name="ckt"></param>
        void Execute(Circuit ckt);
    }
}
