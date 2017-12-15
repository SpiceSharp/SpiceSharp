using System.Collections.Generic;
using SpiceSharp.Circuits;

namespace SpiceSharp.Parser.Subcircuits
{
    /// <summary>
    /// Represent a parsed subcircuit instance
    /// </summary>
    public class Subcircuit
    {
        /// <summary>
        /// Gets the definition this subcircuit is used for
        /// </summary>
        public SubcircuitDefinition Definition { get; }

        /// <summary>
        /// Gets the name of the subcircuit
        /// </summary>
        public Identifier Name { get; }

        /// <summary>
        /// Gets the list of pins for the subcircuit
        /// </summary>
        public List<Identifier> Pins { get; } = new List<Identifier>();

        /// <summary>
        /// Gets the parameters for this instance
        /// </summary>
        public Dictionary<Identifier, Token> Parameters { get; } = new Dictionary<Identifier, Token>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="definition">The definition</param>
        /// <param name="name">Name identifier</param>
        public Subcircuit(SubcircuitDefinition definition, Identifier name, List<Identifier> pins, Dictionary<Identifier, Token> parameters = null)
        {
            Definition = definition;
            Name = name;
            Pins = pins;
            Parameters = parameters;
        }
    }
}
