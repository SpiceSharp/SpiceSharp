using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Parser.Readers;

namespace SpiceSharp.Parser.Subcircuits
{
    /// <summary>
    /// This class represents a subcircuit definition
    /// </summary>
    public class SubcircuitDefinition
    {
        /// <summary>
        /// The name of the definition
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The pins of the subcircuit definition
        /// </summary>
        public List<string> Pins { get; } = new List<string>();

        /// <summary>
        /// Private variables
        /// </summary>
        private Dictionary<StatementType, List<Statement>> statements { get; } = new Dictionary<StatementType, List<Statement>>();
        private Dictionary<string, SubcircuitDefinition> definitions { get; } = new Dictionary<string, SubcircuitDefinition>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name</param>
        public SubcircuitDefinition(Statement st)
        {
            // Parse the name
            if (st == null || st.Parameters == null)
            {
                Name = null;
                return;
            }
            if (st.Parameters.Count > 0)
                Name = st.Parameters[0].ReadIdentifier();

            statements.Add(StatementType.Model, new List<Statement>());
            statements.Add(StatementType.Control, new List<Statement>());

            // Get the body
            if (st.Parameters.Count > 1)
            {
                object body = st.Parameters.Last();
                if (!(body is List<Statement>))
                    throw new ParseException(st.Name, "Invalid subcircuit definition");
                List<Statement> stbody = body as List<Statement>;

                // Parse nodes
                for (int i = 1; i < st.Parameters.Count - 1; i++)
                    Pins.Add(st.Parameters[i].ReadIdentifier());

                // Parse the body statements
                foreach (Statement s in stbody)
                    AddStatement(s);
            }
        }

        /// <summary>
        /// Add a statement to the definition
        /// </summary>
        /// <param name="st">The statement</param>
        private void AddStatement(Statement st)
        {
            if (st.Type == StatementType.Subcircuit)
            {
                SubcircuitDefinition ndef = new SubcircuitDefinition(st);
                definitions.Add(ndef.Name, ndef);
            }
            else
            {
                if (!statements.ContainsKey(st.Type))
                    statements.Add(st.Type, new List<Statement>());
                statements[st.Type].Add(st);
            }
        }

        /// <summary>
        /// Read all the statements of the subcircuit definition
        /// Only models and components are read!
        /// </summary>
        /// <param name="r">The token readers that are active</param>
        /// <param name="netlist">The netlist</param>
        public void ReadStatements(Netlist netlist)
        {
            // Read all models
            foreach (var s in statements[StatementType.Model])
                netlist.Readers.Read(s, netlist);

            // Read all components
            foreach (var s in statements[StatementType.Component])
                netlist.Readers.Read(s, netlist);
        }

        /// <summary>
        /// Add a definition
        /// </summary>
        /// <param name="def">The subcircuit definition</param>
        public void AddDefinition(SubcircuitDefinition def) => definitions.Add(def.Name, def);

        /// <summary>
        /// Check if a subcircuit definition is in the definitions
        /// </summary>
        /// <param name="name">The name</param>
        /// <returns></returns>
        public bool ContainsDefinition(string name) => definitions.ContainsKey(name);

        /// <summary>
        /// Get a definition by name
        /// </summary>
        /// <param name="name">The name</param>
        /// <returns></returns>
        public SubcircuitDefinition GetDefinition(string name) => definitions[name];
    }
}
