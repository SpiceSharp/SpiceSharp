using System.IO;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Parser.Readers.Exports;
using SpiceSharp.Parser.Readers.Waveforms;

namespace SpiceSharp.Parser
{
    /// <summary>
    /// This class can read a netlist
    /// </summary>
    public class NetlistReader
    {
        /// <summary>
        /// Private variables
        /// </summary>
        public SpiceSharpParser Parser { get; private set; } = null;

        /// <summary>
        /// The netlist
        /// </summary>
        public Netlist Netlist { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stream">The stream</param>
        public NetlistReader(Netlist netlist = null)
        {
            Netlist = netlist;
        }

        /// <summary>
        /// Create a standard netlist, which includes the following:
        /// </summary>
        /// <returns></returns>
        public Netlist StandardNetlist()
        {
            Netlist netlist = new Netlist(new Circuit());

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
                new CurrentSwitchReader(),
                new BipolarModelReader(true),
                new BipolarModelReader(false),
                new DiodeModelReader());

            return netlist;
        }

        /// <summary>
        /// Parse the netlist
        /// </summary>
        public void Parse(string filename)
        {
            using (FileStream stream = new FileStream(filename, FileMode.Open))
            {
                // Parse the stream
                Parse(stream);
            }
        }

        /// <summary>
        /// Parse a stream
        /// Seeking must be possible, as the netlist needs to be parsed in multiple passes.
        /// </summary>
        /// <param name="stream">The stream</param>
        /// <param name="source">The source</param>
        public void Parse(Stream stream)
        {
            if (Netlist == null)
                Netlist = StandardNetlist();
            Parser = new SpiceSharpParser(stream);

            // Parse the netlist for control statements and subcircuit definitions
            Netlist.Readers.Active = StatementType.Control | StatementType.Subcircuit | StatementType.Export;
            stream.Seek(0, SeekOrigin.Begin);
            Parser.ReInit(stream);
            Parser.ParseNetlist(Netlist);

            // Parse the netlist for components while ignoring subcircuit definitions
            Netlist.Readers.Active = StatementType.All 
                & ~StatementType.Subcircuit 
                & ~StatementType.Control
                & ~StatementType.Export;
            stream.Seek(0, SeekOrigin.Begin);
            Parser.ReInit(stream);
            Parser.ParseNetlist(Netlist);
        }
    }
}
