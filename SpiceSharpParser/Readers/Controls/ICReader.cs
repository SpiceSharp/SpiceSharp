using System.Collections.Generic;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read initial conditions
    /// </summary>
    public class ICReader : IReader
    {
        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public bool Read(Token name, List<object> parameters, Netlist netlist)
        {
            if (!name.TryReadLiteral("ic"))
                return false;

            // Only assignments are possible
            for (int i = 0; i < parameters.Count; i++)
            {
                object pname, pvalue;
                parameters[i].ReadAssignment(out pname, out pvalue);

                // The first needs to be a bracketed
                if (!(pname is BracketToken))
                    throw new ParseException(pname, "V() expected");
                var bt = pname as BracketToken;

                if (bt.Name.TryReadLiteral("v"))
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
