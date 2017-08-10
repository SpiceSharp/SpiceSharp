using SpiceSharp.Parser.Readers.Extensions;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read initial conditions
    /// </summary>
    public class ICReader : Reader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ICReader() : base(StatementType.Control) { }

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(Statement st, Netlist netlist)
        {
            if (!st.Name.TryReadLiteral("ic"))
                return false;

            // Only assignments are possible
            for (int i = 0; i < st.Parameters.Count; i++)
            {
                // .IC st.Parameters are of the form V(node) = value
                st.Parameters[i].ReadAssignment(out object pname, out object pvalue);
                if (pname.TryReadBracket(out BracketToken bt) && bt.Name.TryReadLiteral("v"))
                {
                    if (bt.Parameters.Count != 1)
                        throw new ParseException(pname, "One node expected");
                    string node = bt.Parameters[0].ReadIdentifier();
                    string value = pvalue.ReadValue();

                    // Convert to a double
                    double d = (double)Parameters.SpiceMember.ConvertType(this, value, typeof(double));
                    netlist.Circuit.Nodes.IC.Add(node, d);
                }
                else
                    throw new ParseException(pname, "V() expected");
            }

            return true;
        }
    }
}
