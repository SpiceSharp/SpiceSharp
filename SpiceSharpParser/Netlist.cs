using System.Collections.Generic;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;

namespace SpiceSharp.Parser
{
    public class Netlist
    {
        /// <summary>
        /// The available reader classes for tokens when parsing a netlist
        /// </summary>
        public TokenReaders Readers { get; } = new TokenReaders();

        /// <summary>
        /// Gets the circuit
        /// </summary>
        public Circuit Circuit { get; }

        /// <summary>
        /// Gets the list of simulations to be performed
        /// </summary>
        public List<Simulation> Simulations { get; } = new List<Simulation>();

        /// <summary>
        /// Exports for the netlist
        /// These exports will give you the values that are specified for exporting
        /// </summary>
        public List<Export> Exports { get; } = new List<Export>();

        /// <summary>
        /// The event that is fired before a new simulation is started
        /// </summary>
        public event NetlistSimulationEventHandler BeforeSimulationInitialized;

        /// <summary>
        /// The event that is fired when new simulation data is exported
        /// </summary>
        public event ExportNetlistDataEventHandler OnExportSimulationData;

        /// <summary>
        /// The event that is fired after a simulation has finished
        /// </summary>
        public event NetlistSimulationEventHandler AfterSimulationFinished;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public Netlist(Circuit ckt)
        {
            Circuit = ckt;
        }

        /// <summary>
        /// Simulate
        /// </summary>
        public void Simulate()
        {
            for (int i = 0; i < Simulations.Count; i++)
            {
                BeforeSimulationInitialized?.Invoke(this, Simulations[i]);

                // Register our event for catching data
                Simulations[i].ExportSimulationData += ExportSimulationData;
                Circuit.Simulate(Simulations[i]);

                // Unregister
                Simulations[i].ExportSimulationData -= ExportSimulationData;

                AfterSimulationFinished?.Invoke(this, Simulations[i]);
            }
        }

        /// <summary>
        /// A wrapper for exporting data along with the netlist info
        /// </summary>
        /// <param name="sender">Simulation</param>
        /// <param name="data">Data</param>
        private void ExportSimulationData(object sender, SimulationData data)
        {
            OnExportSimulationData?.Invoke(this, data);
        }
    }

    /// <summary>
    /// An event handler for handling netlist actions
    /// </summary>
    /// <param name="sender">The netlist sending the event</param>
    /// <param name="sim">The simulation</param>
    public delegate void NetlistSimulationEventHandler(object sender, Simulation sim);

    /// <summary>
    /// An event handler for exporting simulation data through a netlist
    /// </summary>
    /// <param name="sender">The netlist sending the event</param>
    /// <param name="data">The simulation data</param>
    public delegate void ExportNetlistDataEventHandler(object sender, SimulationData data);
}
