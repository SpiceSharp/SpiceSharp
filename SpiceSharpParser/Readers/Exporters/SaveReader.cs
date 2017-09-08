namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Reads saved exports (.SAVE).
    /// </summary>
    public class SaveReader : Reader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SaveReader() : base(StatementType.Control)
        {
            Identifier = "save";
        }

        /// <summary>
        /// Read
        /// This class will export 
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(string type, Statement st, Netlist netlist)
        {
            for (int i = 0; i < st.Parameters.Count; i++)
            {
                if (st.Parameters[i].kind == BRACKET)
                {
                    BracketToken bt = st.Parameters[i] as BracketToken;
                    Statement s = new Statement(StatementType.Export, bt.Name, bt.Parameters);
                    Generated = netlist.Readers.Read(s, netlist);
                }
            }
            return true;
        }
    }
}
