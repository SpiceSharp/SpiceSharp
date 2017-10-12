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
        public CircuitIdentifier Name { get; }

        /// <summary>
        /// Gets the list of pins for the subcircuit
        /// </summary>
        public List<CircuitIdentifier> Pins { get; } = new List<CircuitIdentifier>();

        /// <summary>
        /// Gets the parameters for this instance
        /// </summary>
        public Dictionary<CircuitIdentifier, Token> Parameters { get; } = new Dictionary<CircuitIdentifier, Token>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="definition">The definition</param>
        /// <param name="name">Name identifier</param>
        public Subcircuit(SubcircuitDefinition definition, CircuitIdentifier name, List<CircuitIdentifier> pins, Dictionary<CircuitIdentifier, Token> parameters = null)
        {
            Definition = definition;
            Name = name;
            Pins = pins;
            Parameters = parameters;
        }
    }
}
