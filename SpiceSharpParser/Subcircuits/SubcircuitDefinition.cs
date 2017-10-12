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
        public CircuitIdentifier Name { get; }

        /// <summary>
        /// The pins of the subcircuit definition
        /// </summary>
        public List<CircuitIdentifier> Pins { get; } = new List<CircuitIdentifier>();

        /// <summary>
        /// The default parameters for this subcircuit definition
        /// </summary>
        public Dictionary<CircuitIdentifier, Token> Defaults { get; } = new Dictionary<CircuitIdentifier, Token>();

        /// <summary>
        /// The subcircuit definition body
        /// </summary>
        public StatementsToken Body { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the subcircuit definition</param>
        /// <param name="body">The statements</param>
        public SubcircuitDefinition(CircuitIdentifier name, StatementsToken body)
        {
            Name = name ?? throw new ParseException("Invalid subcircuit identifier");
            Body = body ?? throw new ParseException("Invalid subcircuit body");
        }
    }
}
