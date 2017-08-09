using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Parser.Subcircuits;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read subcircuit instances
    /// </summary>
    public class SubcircuitReader : ComponentReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SubcircuitReader()
            : base('x')
        {
        }

        /// <summary>
        /// Generate
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected override CircuitComponent Generate(string name, List<object> parameters, Netlist netlist)
        {
            // First get the number of terminals
            string[] pins = new string[parameters.Count - 1];
            for (int i = 0; i < parameters.Count - 1; i++)
                pins[i] = parameters[i].ReadIdentifier();
            string subcktname = parameters[parameters.Count - 1].ReadIdentifier();

            // Find the subcircuit definition
            SubcircuitDefinition definition = netlist.Path.FindDefinition(subcktname);
            if (definition == null)
                throw new ParseException(parameters[parameters.Count - 1], "Cannot find subcircuit");

            // Create the subcircuit
            Subcircuit subckt = new Subcircuit(name, definition.Pins.ToArray());
            subckt.Connect(pins);

            // Read the subcircuit definition in the subcircuit
            netlist.Path.Descend(subckt, definition);
            definition.ReadStatements(netlist);
            netlist.Path.Ascend();

            // Return the subcircuit
            return subckt;
        }
    }
}
