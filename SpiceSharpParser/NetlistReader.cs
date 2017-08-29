using System.IO;
using SpiceSharp.Parser;
using SpiceSharp.Parser.Readers;

namespace SpiceSharp
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

            // The order of reading
            StatementType[] order = new StatementType[]
            {
                StatementType.Subcircuit,
                StatementType.Control,
                StatementType.Model,
                StatementType.Component
            };

            // Parse the netlist and read the statements
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            StatementsToken main = parser.ParseNetlist(Netlist);
            sw.Stop();
            System.Console.WriteLine("Time for parsing: " + sw.ElapsedMilliseconds + " ms");

            sw.Restart();
            for (int i = 0; i < order.Length; i++)
            {
                foreach (var s in main.Statements(order[i]))
                    Netlist.Readers.Read(s, Netlist);
            }
        }
    }
}
