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
        /// The subcircuit definition body
        /// </summary>
        public StatementsToken Body { get; }

        /// <summary>
        /// Private variables
        /// </summary>
        private Dictionary<string, SubcircuitDefinition> definitions { get; } = new Dictionary<string, SubcircuitDefinition>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the subcircuit definition</param>
        /// <param name="body">The statements</param>
        public SubcircuitDefinition(string name, StatementsToken body)
        {
            Name = name;
            Body = body;
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
