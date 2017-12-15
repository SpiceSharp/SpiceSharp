using SpiceSharp.Circuits;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Reads parameter definitions (.PARAM)
    /// </summary>
    public class ParamSetReader : Reader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ParamSetReader()
            : base(StatementType.Control)
        {
            Identifier = "param";
        }

        /// <summary>
        /// Read the parameter
        /// </summary>
        /// <param name="st"></param>
        /// <param name="netlist"></param>
        /// <returns></returns>
        public override bool Read(string type, Statement st, Netlist netlist)
        {
            // Read all assignments
            for (int i = 0; i < st.Parameters.Count; i++)
            {
                switch (st.Parameters[i].kind)
                {
                    case ASSIGNMENT:
                        AssignmentToken at = st.Parameters[i] as AssignmentToken;
                        switch (at.Name.kind)
                        {
                            case WORD:
                                netlist.Path.Parameters.Add(new Identifier(at.Name.image), netlist.ParseDouble(at.Value));
                                break;

                            default:
                                throw new ParseException(at.Name, "Parameter expected");
                        }
                        break;

                    default:
                        throw new ParseException(st.Parameters[i], "Assignment expected");
                }
            }

            Generated = null;
            return true;
        }
    }
}
