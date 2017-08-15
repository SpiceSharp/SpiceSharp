using SpiceSharp.Parser.Readers.Extensions;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read nodesets
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
        public override bool Read(Statement st, Netlist netlist)
        {
            // Only assignments are possible
            for (int i = 0; i < st.Parameters.Count; i++)
            {
                switch (st.Parameters[i].kind)
                {
                    case TokenConstants.ASSIGNMENT:
                        AssignmentToken at = st.Parameters[i] as AssignmentToken;
                        switch (at.Name.kind)
                        {
                            case TokenConstants.BRACKET:
                                BracketToken bt = at.Name as BracketToken;
                                if (bt.Name.image.ToLower() == "v" && bt.Parameters.Length == 1 && ReaderExtension.IsNode(bt.Parameters[0]))
                                    netlist.Circuit.Nodes.Nodeset.Add(bt.Parameters[0].image.ToLower(), netlist.ParseDouble(at.Value));
                                else
                                    throw new ParseException(st.Parameters[i], "Invalid format, v(<node>)=<ic> expected");
                                break;
                            default:
                                if (ReaderExtension.IsNode(at.Name))
                                    netlist.Circuit.Nodes.Nodeset.Add(at.Name.image.ToLower(), netlist.ParseDouble(at.Value));
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
