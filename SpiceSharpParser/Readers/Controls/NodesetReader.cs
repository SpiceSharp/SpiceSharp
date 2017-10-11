using SpiceSharp.Circuits;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Reads nodesets (.NODESET)
    /// </summary>
    public class NodesetReader : Reader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public NodesetReader() : base(StatementType.Control)
        {
            Identifier = "nodeset";
        }

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(string type, Statement st, Netlist netlist)
        {
            // Only assignments are possible
            for (int i = 0; i < st.Parameters.Count; i++)
            {
                switch (st.Parameters[i].kind)
                {
                    case ASSIGNMENT:
                        AssignmentToken at = st.Parameters[i] as AssignmentToken;
                        switch (at.Name.kind)
                        {
                            case BRACKET:
                                BracketToken bt = at.Name as BracketToken;
                                if (bt.Name.image.ToLower() == "v" && bt.Parameters.Length == 1 && ReaderExtension.IsNode(bt.Parameters[0]))
                                    netlist.Circuit.Nodes.Nodeset.Add(new CircuitIdentifier(bt.Parameters[0].image), netlist.ParseDouble(at.Value));
                                else
                                    throw new ParseException(st.Parameters[i], "Invalid format, v(<node>)=<ic> expected");
                                break;

                            default:
                                if (ReaderExtension.IsNode(at.Name))
                                    netlist.Circuit.Nodes.Nodeset.Add(new CircuitIdentifier(at.Name.image), netlist.ParseDouble(at.Value));
                                else
                                    throw new ParseException(st.Parameters[i], "Invalid format, <node>=<ic> expected");
                                break;
                        }
                        break;
                }
            }
            return true;
        }
    }
}
