using SpiceSharp.Parser.Readers.Extensions;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// An exporter that can read .save statements
    /// </summary>
    public class SaveReader : Reader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SaveReader() : base(StatementType.Control) { }

        /// <summary>
        /// Read
        /// This class will export 
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(Statement st, Netlist netlist)
        {
            if (!st.Name.TryReadLiteral("save"))
                return false;

            for (int i = 0; i < st.Parameters.Count; i++)
            {
                if (st.Parameters[i].TryReadBracket(out BracketToken bt, '?'))
                {
                    if (!(bt.Name is Token))
                        throw new ParseException(bt, "Export type expected");
                    Statement s = new Statement(StatementType.Export, bt.Name as Token, bt.Parameters);
                    Generated = netlist.Readers.Read(s, netlist);
                }
            }
            return true;
        }
    }
}
