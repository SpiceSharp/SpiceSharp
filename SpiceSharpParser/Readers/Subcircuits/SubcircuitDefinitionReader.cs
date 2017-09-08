using SpiceSharp.Parser.Subcircuits;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Reads a <see cref="SubcircuitDefinition"/>.
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
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(string type, Statement st, Netlist netlist)
        {
            if (st.Parameters.Count < 2)
                throw new ParseException(st.Name, "Subcircuit name expected", false);

            // Create the subcircuit definition
            string name = st.Parameters[0].image.ToLower();
            StatementsToken body = st.Parameters[st.Parameters.Count - 1] as StatementsToken;
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
                    if (st.Parameters[i].image.ToLower() == "params:")
                        mode = false;

                    // Parameters have started, so we will keep reading parameters
                    else if (st.Parameters[i].kind == ASSIGNMENT)
                    {
                        mode = false;
                        AssignmentToken at = st.Parameters[i] as AssignmentToken;
                        definition.Defaults.Add(at.Name.image.ToLower(), at.Value);
                    }

                    // Still reading nodes
                    else if (ReaderExtension.IsNode(st.Parameters[i]))
                        definition.Pins.Add(st.Parameters[i].image.ToLower());
                }
                else if (st.Parameters[i].kind == ASSIGNMENT)
                {
                    AssignmentToken at = st.Parameters[i] as AssignmentToken;
                    definition.Defaults.Add(at.Name.image.ToLower(), at.Value);
                }
            }

            // Create a new subcircuit definition
            netlist.Path.AddDefinition(definition);
            netlist.Path.Descend(null, definition, null);
            foreach (var s in definition.Body.Statements(StatementType.Subcircuit))
                netlist.Readers.Read(s, netlist);
            netlist.Path.Ascend();

            Generated = definition;
            return true;
        }
    }
}
