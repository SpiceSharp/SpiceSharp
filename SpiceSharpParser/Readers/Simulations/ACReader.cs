using SpiceSharp.Simulations;
using SpiceSharp.Parser.Readers.Extensions;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class that can read AC analysis
    /// </summary>
    public class ACReader : Reader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ACReader() : base(StatementType.Control) { }

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="st">Statement</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(Statement st, Netlist netlist)
        {
            if (!st.Name.TryReadLiteral("ac"))
                return false;

            AC ac = new AC("AC " + (netlist.Simulations.Count + 1));
            switch (st.Parameters.Count)
            {
                case 0: throw new ParseException(st.Name, "LIN, DEC or OCT expected");
                case 1: throw new ParseException(st.Parameters[0], "Number of points expected");
                case 2: throw new ParseException(st.Parameters[1], "Starting frequency expected");
                case 3: throw new ParseException(st.Parameters[2], "Stopping frequency expected");
            }

            // Standard st.Parameters
            string type = st.Parameters[0].ReadWord();
            switch (type)
            {
                case "lin":
                case "oct":
                case "dec":
                    ac.Set("type", type);
                    break;
                default:
                    throw new ParseException(st.Parameters[0], "LIN, DEC or OCT expected");
            }
            ac.Set("steps", st.Parameters[1].ReadValue());
            ac.Set("start", st.Parameters[2].ReadValue());
            ac.Set("stop", st.Parameters[3].ReadValue());

            Generated = ac;
            netlist.Simulations.Add(ac);
            return true;
        }
    }
}
