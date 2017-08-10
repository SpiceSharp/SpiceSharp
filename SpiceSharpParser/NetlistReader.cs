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
            if (netlist == null)
                Netlist = Netlist.StandardNetlist();
            else
                Netlist = netlist;
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
                throw new ParseException("No netlist specified");
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
