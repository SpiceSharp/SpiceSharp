using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Subcircuits
{
    /// <summary>
    /// A class for keeping track of subcircuits
    /// </summary>
    public class SubcircuitPath
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private Netlist netlist;
        private Stack<Subcircuit> csubckt = new Stack<Subcircuit>();
        private Stack<SubcircuitDefinition> csubcktdef = new Stack<SubcircuitDefinition>();
        private Dictionary<string, SubcircuitDefinition> definitions = new Dictionary<string, SubcircuitDefinition>();

        /// <summary>
        /// Get the current components
        /// </summary>
        public CircuitComponents Components { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="netlist">The netlist for the path</param>
        public SubcircuitPath(Netlist netlist)
        {
            this.netlist = netlist;
            Components = netlist.Circuit.Components;
        }

        /// <summary>
        /// Add a definition to the root path
        /// </summary>
        /// <param name="def">The definition</param>
        public void AddDefinition(SubcircuitDefinition def)
        {
            definitions.Add(def.Name, def);
        }

        /// <summary>
        /// Descend
        /// </summary>
        /// <param name="subckt">The subcircuit</param>
        /// <param name="def">The subcircuit definition</param>
        public void Descend(Subcircuit subckt, SubcircuitDefinition def)
        {
            // Push
            csubckt.Push(subckt);
            csubcktdef.Push(def);

            // Update the currently active components
            Components = subckt.Components;
        }

        /// <summary>
        /// Ascend
        /// </summary>
        public void Ascend()
        {
            // Pop
            csubckt.Pop();
            csubcktdef.Pop();

            // Update the currently active components
            if (csubckt.Count > 0)
                Components = csubckt.Peek().Components;
            else
                Components = netlist.Circuit.Components;
        }

        /// <summary>
        /// Find a model
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public CircuitModel FindModel(string name)
        {
            // Find the model in any of the components
            foreach (Subcircuit subckt in csubckt)
            {
                // Try to find it in this subcircuit
                if (subckt.Components.Contains(name))
                {
                    CircuitComponent c = subckt.Components[name];
                    if (c is CircuitModel)
                        return c as CircuitModel;
                }
            }

            // Finally try to find it in the circuit
            if (netlist.Circuit.Components.Contains(name))
            {
                CircuitComponent c = netlist.Circuit.Components[name];
                if (c is CircuitModel)
                    return c as CircuitModel;
            }

            throw new ParseException($"Cannot find model \"{name}\"");
        }

        /// <summary>
        /// Find a subcircuit definition
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns></returns>
        public SubcircuitDefinition FindDefinition(string name)
        {
            // Find the definition in any of the subcircuit definitions
            foreach (SubcircuitDefinition def in csubcktdef)
            {
                // try to find it in this definition
                if (def.ContainsDefinition(name))
                    return def.GetDefinition(name);
            }

            // Finally try to find it in the current definitions
            if (definitions.ContainsKey(name))
                return definitions[name];
            throw new ParseException($"Cannot find subcircuit \"{name}\"");
        }
    }
}
