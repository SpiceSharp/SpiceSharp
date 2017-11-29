using System.Collections.Generic;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Parser.Readers.Exports;
using SpiceSharp.Parser.Readers.Waveforms;
using SpiceSharp.Parser.Subcircuits;
using SpiceSharp.Parser.Readers.Collections;
using SpiceSharp.Parser.Expressions;
using SpiceSharp.Simulations;
using SpiceSharp.Circuits;

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
        public SubcircuitPath Path
        {
            get => path;
            set
            {
                path = value;
                OnPathChanged?.Invoke(this, path);
            }
        }
        private SubcircuitPath path = new SubcircuitPath();

        /// <summary>
        /// Get all subcircuit definitions
        /// </summary>
        public Dictionary<CircuitIdentifier, SubcircuitDefinition> Definitions { get; } = new Dictionary<CircuitIdentifier, SubcircuitDefinition>();

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
        /// Event called when the current path has changed
        /// </summary>
        public event NetlistPathChangedEventHandler OnPathChanged;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public Netlist(Circuit ckt)
        {
            Circuit = ckt;
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

                Simulations[i].Circuit = Circuit;
                Circuit.Simulation = Simulations[i];
                Simulations[i].SetupAndExecute();

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
        /// <list type="bullet">
        ///     <listheader>Supported components</listheader>
        ///     <item><token>R, C, L, M</token><description>Resistors, Capacitors, Inductors and Mutual inductances</description></item>
        ///     <item><token>V, I</token><description>Independent sources</description></item>
        ///     <item><token>E, F, H, G</token><description>Controlled sources</description></item>
        ///     <item><token>S, W</token><description>Switches</description></item>
        ///     <item><token>X</token><description>Subcircuit instances</description></item>
        /// </list>
        /// <list type="bullet">
        ///     <listheader>Supported control statements</listheader>
        ///     <item><token>.MODEL</token><description>Model definitions for above components</description></item>
        ///     <item><token>.SUBCKT</token><description>Subcircuit definitions</description></item>
        ///     <item><token>.IC, .NODESET</token><description>Initial conditions and nodesets</description></item>
        ///     <item><token>.SAVE</token><description>Saving exported quantities</description></item>
        /// </list>
        /// <list type="bullet">
        ///     <listheader>Additional features</listheader>
        ///     <item><description>An expression parser</description></item>
        /// </list>
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
                new OPReader(),
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
            netlist.OnPathChanged += (object sender, SubcircuitPath path) =>
            {
                e.Parameters = path.Parameters;
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

    /// <summary>
    /// Event handler used when changing the current path
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="path">Subcircuit path</param>
    public delegate void NetlistPathChangedEventHandler(object sender, SubcircuitPath path);
}
