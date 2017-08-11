using System.IO;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Parser.Readers.Exports;
using SpiceSharp.Parser.Readers.Waveforms;
using System;

namespace SpiceSharp.Parser
{
    /// <summary>
    /// This class can read a netlist
    /// </summary>
    public class NetlistReader
    {
        /// <summary>
        /// The netlist
        /// </summary>
        public Netlist Netlist { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public NetlistReader()
        {
            Netlist = Netlist.StandardNetlist();
        }

        /// <summary>
        /// Constructor
        /// If netlist is null, a standard netlist is generated
        /// </summary>
        /// <param name="netlist">The netlist</param>
        public NetlistReader(Netlist netlist)
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
            SpiceSharpParser parser = new SpiceSharpParser(stream);

            // Parse the netlist for control statements and subcircuit definitions
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            Netlist.Readers.Active = StatementType.Control | StatementType.Subcircuit | StatementType.Export;
            stream.Seek(0, SeekOrigin.Begin);
            parser.ReInit(stream);
            sw.Stop();
            Console.WriteLine("Setting up stream: " + sw.ElapsedMilliseconds);
            sw.Restart();
            parser.ParseNetlist(Netlist);
            sw.Stop();
            Console.WriteLine("Parsing definitions: " + sw.ElapsedMilliseconds);

            // Parse the netlist for components while ignoring subcircuit definitions
            sw.Restart();
            Netlist.Readers.Active = StatementType.All 
                & ~StatementType.Subcircuit 
                & ~StatementType.Control
                & ~StatementType.Export;
            stream.Seek(0, SeekOrigin.Begin);
            parser.ReInit(stream);
            sw.Stop();
            Console.WriteLine("Restarting stream " + sw.ElapsedMilliseconds);
            sw.Restart();
            parser.ParseNetlist(Netlist);
            sw.Stop();
            Console.WriteLine("Parsing components: " + sw.ElapsedMilliseconds);
        }
    }
}
