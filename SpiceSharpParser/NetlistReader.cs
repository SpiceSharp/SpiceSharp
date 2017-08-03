using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
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
        public SpiceSharpParser parser { get; }

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
            parser = new SpiceSharpParser(stream);

            // Make a new netlist
            Netlist = netlist ?? StandardNetlist();
        }

        /// <summary>
        /// Create a standard netlist, which includes the following:
        /// - RLCM components and models
        /// - Voltagesource and currentsource components, with pulse and sine waveforms
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
            parser.ReInit(stream);
            parser.ParseNetlist(Netlist);

            Netlist.Parse = Netlist.ParseTypes.Component;
            stream.Seek(0, SeekOrigin.Begin);
            parser.ReInit(stream);
            parser.ParseNetlist(Netlist);
        }
    }
}
