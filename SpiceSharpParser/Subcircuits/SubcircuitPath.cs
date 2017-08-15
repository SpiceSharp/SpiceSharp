using System;
using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Parser.Readers.Extensions;

namespace SpiceSharp.Parser.Subcircuits
{
    /// <summary>
    /// A class for keeping track of subcircuits
    /// </summary>
    public class SubcircuitPath
    {
        /// <summary>
        /// Enumeration for scope rules
        /// </summary>
        public enum ScopeRule
        {
            // Variables and models are passed down to each subcircuit
            Descend,

            // Only the strictly local and global variables/models are visible
            GlobalLocal
        }

        /// <summary>
        /// The scope rules for parameters
        /// </summary>
        public ScopeRule ParameterScope { get; set; } = ScopeRule.Descend;

        /// <summary>
        /// The scope rules for models
        /// </summary>
        public ScopeRule ModelScope { get; set; } = ScopeRule.Descend;

        /// <summary>
        /// The scope rules for subcircuit definitions
        /// </summary>
        public ScopeRule DefinitionScope { get; set; } = ScopeRule.Descend;

        /// <summary>
        /// Private variables
        /// </summary>
        private Netlist netlist;
        private Stack<Subcircuit> csubckt = new Stack<Subcircuit>();
        private Stack<SubcircuitDefinition> csubcktdef = new Stack<SubcircuitDefinition>();
        private Stack<Dictionary<string, double>> cparams = new Stack<Dictionary<string, double>>();
        private Dictionary<string, SubcircuitDefinition> definitions = new Dictionary<string, SubcircuitDefinition>();
        private Dictionary<string, double> globalparameters = new Dictionary<string, double>();

        /// <summary>
        /// Event that is triggered when the netlist descends into a subcircuit
        /// </summary>
        public event SubcircuitPathChangedEventHandler OnSubcircuitPathChanged;

        /// <summary>
        /// Get the current components
        /// </summary>
        public CircuitObjects Objects { get; private set; }

        /// <summary>
        /// The global parameters
        /// </summary>
        public Dictionary<string, double> Parameters { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="netlist">The netlist for the path</param>
        public SubcircuitPath(Netlist netlist)
        {
            this.netlist = netlist;
            Objects = netlist.Circuit.Objects;
            Parameters = globalparameters;
        }

        /// <summary>
        /// Add a definition to the root path
        /// </summary>
        /// <param name="def">The definition</param>
        public void AddDefinition(SubcircuitDefinition def, SubcircuitDefinition parent = null)
        {
            if (parent == null)
                definitions.Add(def.Name, def);
            else
                parent.AddDefinition(def);
        }

        /// <summary>
        /// Descend into a subcircuit instance
        /// </summary>
        /// <param name="subckt">The subcircuit</param>
        /// <param name="def">Its matching definition</param>
        /// <param name="pars">Parameters passed to the subcircuit</param>
        public void Descend(Subcircuit subckt, SubcircuitDefinition def, Dictionary<string, Token> parameters)
        {
            var nparameters = parameters != null ? MergeParameters(def, parameters) : null;

            // Push
            csubckt.Push(subckt);
            csubcktdef.Push(def);
            cparams.Push(nparameters);

            // Update the currently active components
            if (subckt != null)
                Objects = subckt.Objects;
            if (parameters != null)
            Parameters = nparameters;

            // Call the event
            SubcircuitPathChangedEventArgs args = new SubcircuitPathChangedEventArgs(subckt, def, SubcircuitPathChangedEventArgs.ChangeType.Descend, nparameters);
            OnSubcircuitPathChanged?.Invoke(this, args);
        }

        /// <summary>
        /// Ascend
        /// </summary>
        public void Ascend()
        {
            // Pop
            csubckt.Pop();
            csubcktdef.Pop();
            cparams.Pop();

            // Restore the previous state
            if (csubckt.Count > 0 && csubckt.Peek() != null)
                Objects = csubckt.Peek().Objects;
            else
                Objects = netlist.Circuit.Objects;
            if (cparams.Count > 0 && cparams.Peek() != null)
                Parameters = cparams.Peek();
            else
                Parameters = globalparameters;

            // Event arguments
            Subcircuit subckt = csubckt.Count > 0 ? csubckt.Peek() : null;
            SubcircuitDefinition def = csubcktdef.Count > 0 ? csubcktdef.Peek() : null;
            Dictionary<string, double> pars = cparams.Count > 0 ? cparams.Peek() : Parameters;

            // Call the event
            SubcircuitPathChangedEventArgs args = new SubcircuitPathChangedEventArgs(subckt, def, SubcircuitPathChangedEventArgs.ChangeType.Ascend, pars);
            OnSubcircuitPathChanged?.Invoke(this, args);
        }

        /// <summary>
        /// Find a model
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public T FindModel<T>(string name)
        {
            switch (ModelScope)
            {
                case ScopeRule.Descend:
                    // Find the model in any of the components
                    foreach (Subcircuit subckt in csubckt)
                    {
                        // Try to find it in this subcircuit
                        if (subckt.Objects.Contains(name))
                        {
                            ICircuitObject c = subckt.Objects[name];
                            if (c is T)
                                return (T)c;
                        }
                    }

                    // Finally try to find it in the circuit
                    if (netlist.Circuit.Objects.Contains(name))
                    {
                        ICircuitObject c = netlist.Circuit.Objects[name];
                        if (c is T)
                            return (T)c;
                    }
                    break;

                case ScopeRule.GlobalLocal:
                    // Find the model locally
                    if (Objects.Contains(name))
                    {
                        ICircuitObject c = Objects[name];
                        if (c is T)
                            return (T)c;
                    }

                    // Find the model globally
                    if (netlist.Circuit.Objects.Contains(name))
                    {
                        ICircuitObject c = netlist.Circuit.Objects[name];
                        if (c is T)
                            return (T)c;
                    }
                    break;
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
            switch (DefinitionScope)
            {
                case ScopeRule.Descend:

                    // Find the definition in any of the subcircuit definitions
                    foreach (SubcircuitDefinition def in csubcktdef)
                    {
                        // try to find it in this definition
                        if (def.ContainsDefinition(name))
                            return def.GetDefinition(name);
                    }
                    break;

                case ScopeRule.GlobalLocal:
                    if (csubcktdef.Count > 0)
                    {
                        if (csubcktdef.Peek().ContainsDefinition(name))
                            return csubcktdef.Peek().GetDefinition(name);
                    }
                    break;
            }

            // Finally try to find it in the global definitions
            if (definitions.ContainsKey(name))
                return definitions[name];
            throw new ParseException($"Cannot find subcircuit \"{name}\"");
        }

        /// <summary>
        /// Merge parameters
        /// </summary>
        /// <param name="def">The circuit definition containing the default parameters</param>
        /// <param name="parameters">The parameters</param>
        private Dictionary<string, double> MergeParameters(SubcircuitDefinition def, Dictionary<string, Token> parameters)
        {
            Dictionary<string, double> nparameters = new Dictionary<string, double>();

            // Convert all parameters now
            foreach (var item in parameters)
                nparameters.Add(item.Key, netlist.ParseDouble(item.Value));

            // Add definition defaults
            foreach (var item in def.Defaults)
            {
                if (!nparameters.ContainsKey(item.Key))
                    nparameters.Add(item.Key, netlist.ParseDouble(item.Value));
            }

            // Merge the parameters depending on the scope rules
            switch (ParameterScope)
            {
                case ScopeRule.Descend:

                    // Add all previous parameters that aren't overwritten if they aren't yet added
                    foreach (var item in Parameters)
                    {
                        if (!nparameters.ContainsKey(item.Key))
                            nparameters.Add(item.Key, item.Value);
                    }
                    break;

                case ScopeRule.GlobalLocal:

                    // Only add the global parameters
                    foreach (var item in globalparameters)
                    {
                        if (!nparameters.ContainsKey(item.Key))
                            nparameters.Add(item.Key, item.Value);
                    }
                    break;
            }

            return nparameters;
        }
    }

    /// <summary>
    /// Event data when the subcircuit path changes
    /// </summary>
    public class SubcircuitPathChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The subcircuit
        /// </summary>
        public Subcircuit Subcircuit { get; }

        /// <summary>
        /// The definition
        /// </summary>
        public SubcircuitDefinition Definition { get; }

        /// <summary>
        /// Enumeration of change types
        /// </summary>
        public enum ChangeType
        {
            Descend,
            Ascend
        }

        /// <summary>
        /// The parameters for the new path
        /// </summary>
        public Dictionary<string, double> Parameters { get; }

        /// <summary>
        /// The type of path change
        /// </summary>
        public ChangeType Type { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="subckt">The subcircuit</param>
        /// <param name="def">The definition</param>
        public SubcircuitPathChangedEventArgs(Subcircuit subckt, SubcircuitDefinition def, ChangeType type, Dictionary<string, double> parameters)
        {
            Subcircuit = subckt;
            Definition = def;
            Type = type;
            Parameters = parameters;
        }
    }

    /// <summary>
    /// An event that can be fired when the subcircuit path changes
    /// </summary>
    /// <param name="sender">The sender</param>
    /// <param name="e">The data</param>
    public delegate void SubcircuitPathChangedEventHandler(object sender, SubcircuitPathChangedEventArgs e);
}
