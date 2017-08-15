using System.Collections.Generic;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Parser.Readers.Exports;
using SpiceSharp.Parser.Readers.Waveforms;
using SpiceSharp.Parser.Subcircuits;
using SpiceSharp.Parser.Readers.Collections;
using SpiceSharp.Simulations;

namespace SpiceSharp.Parser
{
    /// <summary>
    /// This class represents the netlist data for parsing
    /// </summary>
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
        public List<ISimulation> Simulations { get; } = new List<ISimulation>();

        /// <summary>
        /// Exports for the netlist
        /// These exports will give you the values that are specified for exporting
        /// </summary>
        public List<Export> Exports { get; } = new List<Export>();

        /// <summary>
        /// The current path
        /// </summary>
        public SubcircuitPath Path { get; }

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
            Path = new SubcircuitPath(this);
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
                Simulations[i].OnExportSimulationData += ExportSimulationData;
                Circuit.Simulate(Simulations[i]);

                // Unregister
                Simulations[i].OnExportSimulationData -= ExportSimulationData;

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

        /// <summary>
        /// Create a standard netlist, which includes the following:
        /// </summary>
        /// <returns></returns>
        public static Netlist StandardNetlist()
        {
            Netlist netlist = new Netlist(new Circuit());

            // Register standard reader collections
            netlist.Readers.Register(
                new ComponentReaderCollection(),
                new ModelReaderCollection(),
                new GenericReaderCollection(StatementType.Subcircuit),
                new GenericReaderCollection(StatementType.Waveform),
                new DictionaryReaderCollection(StatementType.Control),
                new GenericReaderCollection(StatementType.Export)
                );

            // Register standard readers
            netlist.Readers.Register(
                // Subcircuit readers
                new SubcircuitReader(),
                new SubcircuitDefinitionReader(),

                // Component readers
                new ResistorReader(),
                new CapacitorReader(),
                new InductorReader(),
                new MutualInductanceReader(),
                new VoltagesourceReader(),
                new VoltageControlledVoltagesourceReader(),
                new CurrentControlledVoltagesourceReader(),
                new CurrentsourceReader(),
                new VoltageControlledCurrentsourceReader(),
                new CurrentControlledCurrentsourceReader(),
                new VoltageSwitchReader(),
                new CurrentSwitchReader(),
                new BipolarReader(),
                new DiodeReader(),

                // Control readers
                new DCReader(),
                new ACReader(),
                new TransientReader(),
                new ICReader(),
                new NodesetReader(),
                new OptionReader(),
                new SaveReader(),

                // Standard export types
                new VoltageReader(),
                new CurrentReader(),
                new VoltageComplexReader(),
                new CurrentComplexReader(),
                new ParameterReader(),

                // Standard waveform types
                new PulseReader(),
                new SineReader(),

                // Add model types
                new ResistorModelReader(),
                new CapacitorModelReader(),
                new VoltageSwitchModelReader(),
                new CurrentSwitchModelReader(),
                new BipolarModelReader(true),
                new BipolarModelReader(false),
                new DiodeModelReader());

            return netlist;
        }
    }

    /// <summary>
    /// An event handler for handling netlist actions
    /// </summary>
    /// <param name="sender">The netlist sending the event</param>
    /// <param name="sim">The simulation</param>
    public delegate void NetlistSimulationEventHandler(object sender, ISimulation sim);

    /// <summary>
    /// An event handler for exporting simulation data through a netlist
    /// </summary>
    /// <param name="sender">The netlist sending the event</param>
    /// <param name="data">The simulation data</param>
    public delegate void ExportNetlistDataEventHandler(object sender, SimulationData data);
}
