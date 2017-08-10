using System.Collections.Generic;
using SpiceSharp.Parser.Subcircuits;
using SpiceSharp.Parser.Readers.Extensions;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read subcircuit definitions
    /// </summary>
    public class SubcircuitDefinitionReader : Reader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SubcircuitDefinitionReader() : base(StatementType.Subcircuit) { }

        /// <summary>
        /// Read subcircuit definitions
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parameters"></param>
        /// <param name="netlist"></param>
        /// <returns></returns>
        public override bool Read(Statement st, Netlist netlist)
        {
            if (st.Parameters.Count < 2)
                throw new ParseException(st.Name, "Subcircuit name expected", false);

            // Create the subcircuit definition
            string name = st.Parameters[0].ReadIdentifier();
            List<Statement> body = st.Parameters[st.Parameters.Count - 1] as List<Statement>;
            if (body == null)
                throw new ParseException(st.Name, "Invalid subcircuit body passed to method");
            SubcircuitDefinition definition = new SubcircuitDefinition(name, body);

            // Parse nodes and parameters
            bool mode = true; // true = nodes, false = parameters
            for (int i = 1; i < st.Parameters.Count - 1; i++)
            {
                if (mode)
                {
                    // After this, only parameters will follow
                    if (st.Parameters[i].TryReadLiteral("params:"))
                        mode = false;

                    // Parameters have started, so we will keep reading parameters
                    else if (st.Parameters[i].TryReadAssignment(out string pname, out string pvalue))
                    {
                        mode = false;
                        definition.Defaults.Add(pname, pvalue);
                    }

                    // Still reading nodes
                    else
                        definition.Pins.Add(st.Parameters[i].ReadIdentifier());
                }
                else
                {
                    st.Parameters[i].ReadAssignment(out string pname, out string pvalue);
                    definition.Defaults.Add(pname, pvalue);
                }
            }

            // Create a new subcircuit definition
            netlist.Path.AddDefinition(definition);
            netlist.Path.Descend(null, definition, null);
            definition.Read(StatementType.Subcircuit, netlist);
            netlist.Path.Ascend();

            Generated = definition;
            return true;
        }
    }
}
