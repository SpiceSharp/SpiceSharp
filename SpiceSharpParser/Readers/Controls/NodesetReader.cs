using System.Collections.Generic;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read nodesets
    /// </summary>
    public class NodesetReader : IReader
    {
        /// <summary>
        /// The last generated object
        /// </summary>
        public object Generated { get; private set; } = null;

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public bool Read(Token name, List<object> parameters, Netlist netlist)
        {
            if (!name.TryReadLiteral("nodeset"))
                return false;

            // Only assignments are possible
            for (int i = 0; i < parameters.Count; i++)
            {
                // .NODESET parameters are of the form V(name) = value
                parameters[i].ReadAssignment(out object pname, out object pvalue);
                if (pname.TryReadBracket(out BracketToken bt) && bt.Name.TryReadLiteral("v"))
                {
                    if (bt.Parameters.Count != 1)
                        throw new ParseException(pname, "One node expected");
                    string node = bt.Parameters[0].ReadIdentifier();
                    string value = pvalue.ReadValue();

                    // Convert to a double
                    double d = (double)Parameters.SpiceMember.ConvertType(this, value, typeof(double));
                    netlist.Circuit.Nodes.Nodeset.Add(node, d);
                }
                else
                    throw new ParseException(pname, "V() expected");
            }

            return true;
        }
    }
}
