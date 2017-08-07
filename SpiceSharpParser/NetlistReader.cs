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

            // Standard component readers
            netlist.Readers.Register("component",
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
                new DiodeReader());

            // Standard control statement readers
            ModelReader mr = new ModelReader();
            netlist.Readers.Register("control",
                mr,
                new DCReader(),
                new ACReader(),
                new TransientReader(),
                new ICReader(),
                new NodesetReader(),
                new OptionReader(),
                new SaveReader());

            // Standard export types
            netlist.Readers.Register("exporter",
                new VoltageReader(),
                new CurrentReader(),
                new VoltageComplexReader(),
                new CurrentComplexReader(),
                new ParameterReader());

            // Add models
            mr.ModelReaders.Add("r", new ResistorModelReader());
            mr.ModelReaders.Add("c", new CapacitorModelReader());
            mr.ModelReaders.Add("sw", new VoltageSwitchModelReader());
            mr.ModelReaders.Add("csw", new CurrentSwitchReader());
            mr.ModelReaders.Add("npn", new BipolarModelReader(true));
            mr.ModelReaders.Add("pnp", new BipolarModelReader(false));
            mr.ModelReaders.Add("d", new DiodeModelReader());

            // Add waveforms
            netlist.Readers.Register("waveform",
                new PulseReader(),
                new SineReader());

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

            // Parse the netlist for control statements first
            Parser.ParseComponents = false;
            Parser.ParseControlStatements = true;
            stream.Seek(0, SeekOrigin.Begin);
            Parser.ReInit(stream);
            Parser.ParseNetlist(Netlist);

            // Parse the netlist for components next
            Parser.ParseComponents = true;
            Parser.ParseControlStatements = false;
            stream.Seek(0, SeekOrigin.Begin);
            Parser.ReInit(stream);
            Parser.ParseNetlist(Netlist);
        }
    }
}
