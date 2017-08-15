using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Parser.Subcircuits;
using static SpiceSharp.Parser.SpiceSharpParserConstants;
using SpiceSharp.Parser.Readers.Extensions;

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
        protected override ICircuitObject Generate(string name, List<Token> parameters, Netlist netlist)
        {
            List<string> pins = new List<string>();
            Dictionary<string, Token> pars = new Dictionary<string, Token>();
            string subcktname = null;

            // Format: <NAME> <NODES>* <SUBCKT> <PAR1>=<VAL1> ...
            bool mode = true; // true = nodes, false = parameters
            for (int i = 0; i < parameters.Count; i++)
            {
                if (mode)
                {
                    if (ReaderExtension.IsNode(parameters[i]))
                        pins.Add(subcktname = parameters[i].image.ToLower());
                    else
                    {
                        pins.RemoveAt(pins.Count - 1);
                        mode = false;
                    }
                }

                // Reading parameters
                if (!mode)
                {
                    if (parameters[i].kind == TokenConstants.ASSIGNMENT)
                    {
                        AssignmentToken at = parameters[i] as AssignmentToken;
                        switch (at.Name.kind)
                        {
                            case WORD:
                            case IDENTIFIER:
                                pars.Add(at.Name.image.ToLower(), at.Value);
                                break;

                            default:
                                throw new ParseException(at.Name, "Parameter name expected");
                        }
                    }
                }
            }
            if (mode)
                pins.RemoveAt(pins.Count - 1);

            // Find the subcircuit definition
            SubcircuitDefinition definition = netlist.Path.FindDefinition(subcktname);
            if (definition == null)
                throw new ParseException(parameters[parameters.Count - 1], "Cannot find subcircuit");

            // Create the subcircuit
            Subcircuit subckt = new Subcircuit(name, definition.Pins.ToArray());
            subckt.Connect(pins.ToArray());

            // Apply models and components
            netlist.Path.Descend(subckt, definition, pars);
            var oldactive = netlist.Readers.Active;
            netlist.Readers.Active = StatementType.All;
            definition.Read(StatementType.Model, netlist);
            definition.Read(StatementType.Component, netlist);
            netlist.Readers.Active = oldactive;
            netlist.Path.Ascend();

            // Return the subcircuit
            return subckt;
        }
    }
}
