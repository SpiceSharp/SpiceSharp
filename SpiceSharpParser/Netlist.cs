using System.Collections.Generic;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Parser.Readers.Exports;
using SpiceSharp.Parser.Readers.Waveforms;
using SpiceSharp.Parser.Subcircuits;
using SpiceSharp.Parser.Readers.Collections;
using SpiceSharp.Parser.Expressions;
using SpiceSharp.Simulations;

namespace SpiceSharp
{
    /// <summary>
    /// Container for parsed data
    /// </summary>
    public class Netlist
    {
        /// <summary>
        /// All statement readers
        /// </summary>
        public StatementReaders Readers { get; } = new StatementReaders();

        /// <summary>
        /// Get the circuit
        /// </summary>
        public Circuit Circuit { get; }

        /// <summary>
        /// Get a list of simulations
        /// </summary>
        public List<ISimulation> Simulations { get; } = new List<ISimulation>();

        /// <summary>
        /// Get a list of exported quantities
        /// </summary>
        public List<Export> Exports { get; } = new List<Export>();

        /// <summary>
        /// Get the current subcircuit path
        /// Used for parsing subcircuit definitions and instances
        /// </summary>
        public SubcircuitPath Path { get; }

        /// <summary>
        /// Event called before a new simulation is started
        /// </summary>
        public event NetlistSimulationEventHandler BeforeSimulationInitialized;

        /// <summary>
        /// Event called when a simulation exports simulation data
        /// </summary>
        public event ExportSimulationDataEventHandler OnExportSimulationData;

        /// <summary>
        /// Event called after a simulation has finished
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
        /// Perform all simulations in the simulation queue
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
        /// A standard netlist with the following features:
        /// - Components: C, D, E, F, G, H, I, L, M, Q, R, S, X, W
        /// - Control statements: .IC, .NODESET, .OPTIONS, .PARAM, 
        /// - Exporters: V(), I(), VDB(), IDB(), VP(), IP(), VR, IR, VI, RI, @dev[par]
        /// - Subcircuit definitions
        /// - An expression parser
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
                new DictionaryReaderCollection(StatementType.Waveform),
                new DictionaryReaderCollection(StatementType.Control),
                new DictionaryReaderCollection(StatementType.Export)
                );

            // Register standard readers
            netlist.Readers.Register(
                // Subcircuit readers
                new SubcircuitReader(),
                new SubcircuitDefinitionReader(),

                // Component readers
                new RLCMReader(),
                new VoltagesourceReader(),
                new CurrentsourceReader(),
                new SwitchReader(),
                new BipolarReader(),
                new MosfetReader(),
                new DiodeReader(),

                // Control readers
                new ParamSetReader(),
                new DCReader(),
                new ACReader(),
                new TransientReader(),
                new NoiseReader(),
                new ICReader(),
                new NodesetReader(),
                new OptionReader(),
                new SaveReader(),

                // Standard export types
                new VoltageReader(),
                new CurrentReader(),
                new ParameterReader(),

                // Standard waveform types
                new PulseReader(),
                new SineReader(),
                new PwlReader(),

                // Add model types
                new RLCMModelReader(),
                new SwitchModelReader(),
                new BipolarModelReader(),
                new MosfetModelReader(),
                new DiodeModelReader());

            // Standard parser
            SpiceExpression e = new SpiceExpression();
            netlist.Readers.OnParseExpression += (object sender, ExpressionData data) =>
            {
                data.Output = e.Parse(data.Input);
            };
            netlist.Path.OnSubcircuitPathChanged += (object sender, SubcircuitPathChangedEventArgs args) =>
            {
                e.Parameters = args.Parameters;
            };

            return netlist;
        }
    }

    /// <summary>
    /// Event handler involving the current simulation
    /// </summary>
    /// <param name="sender">The netlist sending the event</param>
    /// <param name="sim">The simulation</param>
    public delegate void NetlistSimulationEventHandler(object sender, ISimulation sim);
}
