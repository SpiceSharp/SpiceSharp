using SpiceSharp.Circuits;
using System.Collections.Generic;

namespace SpiceSharp.Parser.Subcircuits
{
    /// <summary>
    /// Represents a subcircuit definition
    /// </summary>
    public class SubcircuitDefinition
    {
        /// <summary>
        /// The name of the definition
        /// </summary>
        public Identifier Name { get; }

        /// <summary>
        /// The pins of the subcircuit definition
        /// </summary>
        public List<Identifier> Pins { get; } = new List<Identifier>();

        /// <summary>
        /// The default parameters for this subcircuit definition
        /// </summary>
        public Dictionary<Identifier, Token> Defaults { get; } = new Dictionary<Identifier, Token>();

        /// <summary>
        /// The subcircuit definition body
        /// </summary>
        public StatementsToken Body { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the subcircuit definition</param>
        /// <param name="body">The statements</param>
        public SubcircuitDefinition(Identifier name, StatementsToken body)
        {
            Name = name ?? throw new ParseException("Invalid subcircuit identifier");
            Body = body ?? throw new ParseException("Invalid subcircuit body");
        }
    }
}
