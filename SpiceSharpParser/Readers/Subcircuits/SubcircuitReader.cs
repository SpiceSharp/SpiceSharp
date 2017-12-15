using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Parser.Subcircuits;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Reads a <see cref="Subcircuit"/> component.
    /// </summary>
    public class SubcircuitReader : Reader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SubcircuitReader() : base(StatementType.Component)
        {
            Identifier = "x";
        }

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="st">Statement</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(string type, Statement st, Netlist netlist)
        {
            // Initialize
            List<Identifier> instancepins = new List<Identifier>();
            Dictionary<Identifier, Token> instanceparameters = new Dictionary<Identifier, Token>();
            Identifier definition = null;

            // Get the name
            Identifier name;
            if (netlist.Path.InstancePath != null)
                name = netlist.Path.InstancePath.Grow(st.Name.image);
            else
                name = new Identifier(st.Name.image);

            // Format: <NAME> <NODES>* <SUBCKT> <PAR1>=<VAL1> ...
            // Or: <NAME> <NODES>* <SUBCKT> params: <PAR1>=<VAL1> ...
            bool mode = true; // true = nodes, false = parameters
            for (int i = 0; i < st.Parameters.Count; i++)
            {
                // Reading nodes
                if (mode)
                {
                    if (ReaderExtension.IsNode(st.Parameters[i]))
                        instancepins.Add(definition = new Identifier(st.Parameters[i].image));
                    else
                    {
                        instancepins.RemoveAt(instancepins.Count - 1);
                        mode = false;
                    }
                }

                // Reading parameters
                // Don't use ELSE! We still need to read the last parameter
                if (!mode)
                {
                    if (st.Parameters[i].kind == ASSIGNMENT)
                    {
                        AssignmentToken at = st.Parameters[i] as AssignmentToken;
                        switch (at.Name.kind)
                        {
                            case WORD:
                            case IDENTIFIER:
                                instanceparameters.Add(new Identifier(at.Name.image), at.Value);
                                break;

                            default:
                                throw new ParseException(at.Name, "Parameter name expected");
                        }
                    }
                }
            }

            // If there are only node-like tokens, then the last one is the definition by default
            if (mode)
                instancepins.RemoveAt(instancepins.Count - 1);

            // Modify the instancepins to be local or use the nodemap
            for (int i = 0; i < instancepins.Count; i++)
            {
                if (netlist.Path.NodeMap.TryGetValue(instancepins[i], out Identifier node))
                    instancepins[i] = node;
                else if (netlist.Path.InstancePath != null)
                    instancepins[i] = netlist.Path.InstancePath.Grow(instancepins[i].Name);
            }

            // Find the subcircuit definition
            if (netlist.Path.DefinitionPath != null)
                definition = netlist.Path.DefinitionPath.Grow(definition);
            SubcircuitDefinition subcktdef = netlist.Path.FindDefinition(netlist.Definitions, definition) ?? 
                throw new ParseException(st.Parameters[st.Parameters.Count - 1], "Cannot find subcircuit definition");
            Subcircuit subckt = new Subcircuit(subcktdef, name, instancepins, instanceparameters);

            SubcircuitPath orig = netlist.Path;
            netlist.Path = new SubcircuitPath(netlist, orig, subckt);

            // Read all control statements
            foreach (var s in subcktdef.Body.Statements(StatementType.Control))
                netlist.Readers.Read(s, netlist);

            // Read all model statements
            foreach (var s in subcktdef.Body.Statements(StatementType.Model))
                netlist.Readers.Read(s, netlist);

            // Read all component statements
            foreach (var s in subcktdef.Body.Statements(StatementType.Component))
                netlist.Readers.Read(s, netlist);

            // Restore
            netlist.Path = orig;
            Generated = subckt;
            return true;
        }
    }
}
