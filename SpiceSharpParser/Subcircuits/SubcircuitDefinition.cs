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
        /// The default parameters for this subcircuit definition
        /// </summary>
        public Dictionary<string, Token> Defaults { get; } = new Dictionary<string, Token>();

        /// <summary>
        /// Private variables
        /// </summary>
        private Dictionary<StatementType, List<Statement>> statements { get; } = new Dictionary<StatementType, List<Statement>>();
        private Dictionary<string, SubcircuitDefinition> definitions { get; } = new Dictionary<string, SubcircuitDefinition>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the subcircuit definition</param>
        /// <param name="body">The statements</param>
        public SubcircuitDefinition(string name, IEnumerable<Statement> body)
        {
            Name = name;

            // Basic statements so later checks aren't needed
            statements.Add(StatementType.Model, new List<Statement>());
            statements.Add(StatementType.Control, new List<Statement>());

            // Parse the body statements
            foreach (Statement s in body)
                AddStatement(s);
        }

        /// <summary>
        /// Add a statement to the definition
        /// </summary>
        /// <param name="st">The statement</param>
        private void AddStatement(Statement st)
        {
            if (!statements.ContainsKey(st.Type))
                statements.Add(st.Type, new List<Statement>());
            statements[st.Type].Add(st);
        }

        /// <summary>
        /// Read the statements in the subcircuit definition
        /// </summary>
        /// <param name="type">The type of the statements</param>
        /// <param name="netlist">The netlist</param>
        public void Read(StatementType type, Netlist netlist)
        {
            if (statements.ContainsKey(type))
            {
                foreach (var st in statements[type])
                    netlist.Readers.Read(st, netlist);
            }
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
