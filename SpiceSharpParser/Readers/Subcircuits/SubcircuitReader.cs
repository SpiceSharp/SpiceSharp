using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Parser.Subcircuits;
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
        protected override CircuitComponent Generate(string name, List<object> parameters, Netlist netlist)
        {
            List<string> pins = new List<string>();
            Dictionary<string, string> pars = new Dictionary<string, string>();
            string subcktname = null;

            // Format: <NAME> <NODES>* <SUBCKT> <PAR1>=<VAL1> ...
            bool mode = true; // true = nodes, false = parameters
            for (int i = 0; i < parameters.Count; i++)
            {
                if (mode)
                {
                    if (parameters[i].TryReadIdentifier(out subcktname))
                        pins.Add(subcktname);
                    else
                    {
                        // Parameter found, which means our last pin was actually our subcircuit name
                        pins.RemoveAt(pins.Count - 1);
                        mode = false;
                    }
                }

                // Reading parameters
                if (!mode)
                {
                    parameters[i].ReadAssignment(out string pname, out string pvalue);
                    pars.Add(pname, pvalue);
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
            definition.Read(StatementType.Model, netlist);
            definition.Read(StatementType.Component, netlist);
            netlist.Path.Ascend();

            // Return the subcircuit
            return subckt;
        }
    }
}
