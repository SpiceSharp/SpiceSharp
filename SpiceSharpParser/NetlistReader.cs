using System.IO;
using SpiceSharp.Parser;
using SpiceSharp.Parser.Readers;

namespace SpiceSharp
{
    /// <summary>
    /// Reads Spice-formatted netlists and parses in a <see cref="Netlist"/>.
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
        /// <param name="netlist">The netlist. If null, <see cref="Netlist.StandardNetlist"/> is used.</param>
        public NetlistReader(Netlist netlist)
        {
            if (netlist == null)
                Netlist = Netlist.StandardNetlist();
            else
                Netlist = netlist;
        }

        /// <summary>
        /// Parse a file
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
        /// Parse from a <see cref="Stream"/>
        /// </summary>
        /// <param name="stream">The stream</param>
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
            StatementsToken main = parser.ParseNetlist(Netlist);
            for (int i = 0; i < order.Length; i++)
            {
                foreach (var s in main.Statements(order[i]))
                    Netlist.Readers.Read(s, Netlist);
            }
        }
    }
}
