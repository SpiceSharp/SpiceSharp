using System.IO;
using SpiceSharp.Parser.Readers;
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
        public SpiceSharpParser Parser { get; }

        /// <summary>
        /// The netlist
        /// </summary>
        public Netlist Netlist { get; set; }

        /// <summary>
        /// Private variables
        /// </summary>
        private Stream stream;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stream">The stream</param>
        public NetlistReader(Stream stream, Netlist netlist = null)
        {
            this.stream = stream;
            Parser = new SpiceSharpParser(stream);

            // Make a new netlist
            Netlist = netlist ?? StandardNetlist();
        }

        /// <summary>
        /// Create a standard netlist, which includes the following:
        /// </summary>
        /// <returns></returns>
        public Netlist StandardNetlist()
        {
            Netlist netlist = new Netlist(new Circuit());

            // Standard component readers
            netlist.ComponentReaders.Add(new ResistorReader());
            netlist.ComponentReaders.Add(new CapacitorReader());
            netlist.ComponentReaders.Add(new InductorReader());
            netlist.ComponentReaders.Add(new MutualInductanceReader());

            netlist.ComponentReaders.Add(new VoltagesourceReader());
            netlist.ComponentReaders.Add(new VoltageControlledVoltagesourceReader());
            netlist.ComponentReaders.Add(new CurrentControlledVoltagesourceReader());

            netlist.ComponentReaders.Add(new CurrentsourceReader());
            netlist.ComponentReaders.Add(new VoltageControlledCurrentsourceReader());
            netlist.ComponentReaders.Add(new CurrentControlledCurrentsourceReader());

            netlist.ComponentReaders.Add(new VoltageSwitchReader());
            netlist.ComponentReaders.Add(new CurrentSwitchReader());

            netlist.ComponentReaders.Add(new BipolarReader());
            netlist.ComponentReaders.Add(new DiodeReader());

            // Standard control statement readers
            ModelReader mr = new ModelReader();
            netlist.ControlReaders.Add(mr);

            netlist.ControlReaders.Add(new DCReader());
            netlist.ControlReaders.Add(new ACReader());
            netlist.ControlReaders.Add(new TransientReader());

            netlist.ControlReaders.Add(new ICReader());
            netlist.ControlReaders.Add(new NodesetReader());
            netlist.ControlReaders.Add(new OptionReader());

            // Add models
            mr.ModelReaders.Add("r", new ResistorModelReader());
            mr.ModelReaders.Add("c", new CapacitorModelReader());
            mr.ModelReaders.Add("sw", new VoltageSwitchModelReader());
            mr.ModelReaders.Add("csw", new CurrentSwitchReader());
            mr.ModelReaders.Add("npn", new BipolarModelReader(true));
            mr.ModelReaders.Add("pnp", new BipolarModelReader(false));
            mr.ModelReaders.Add("d", new DiodeModelReader());

            // Add waveforms
            netlist.WaveformReaders.Add(new PulseReader());
            netlist.WaveformReaders.Add(new SineReader());

            return netlist;
        }

        /// <summary>
        /// Parse the netlist
        /// </summary>
        public void Parse()
        {
            // Initialize if necessary
            if (Netlist == null)
                Netlist = StandardNetlist();

            // Parse the netlist for control statements first
            Netlist.Parse = Netlist.ParseTypes.Control;
            stream.Seek(0, SeekOrigin.Begin);
            Parser.ReInit(stream);
            Parser.ParseNetlist(Netlist);

            Netlist.Parse = Netlist.ParseTypes.Component;
            stream.Seek(0, SeekOrigin.Begin);
            Parser.ReInit(stream);
            Parser.ParseNetlist(Netlist);
        }
    }
}
