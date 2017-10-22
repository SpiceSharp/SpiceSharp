using SpiceSharp.Parameters;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Provides methods for performing a simulation using a <see cref="Circuit"/>.
    /// </summary>
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

        Circuit Circuit { get; set; }

        void SetupAndExecute();
    }
}
