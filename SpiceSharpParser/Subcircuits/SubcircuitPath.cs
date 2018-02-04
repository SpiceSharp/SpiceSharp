using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Parser.Readers;

namespace SpiceSharp.Parser.Subcircuits
{
    /// <summary>
    /// Keeps track of subcircuits and subcircuit definitions
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

            // Only strictly local and global variables/models are visible
            GlobalLocal
        }

        /// <summary>
        /// The scope rules for parameters
        /// </summary>
        public static ScopeRule ParameterScope { get; set; } = ScopeRule.Descend;

        /// <summary>
        /// The scope rules for models
        /// </summary>
        public static ScopeRule ModelScope { get; set; } = ScopeRule.Descend;

        /// <summary>
        /// The scope rules for subcircuit definitions
        /// </summary>
        public static ScopeRule DefinitionScope { get; set; } = ScopeRule.Descend;

        /// <summary>
        /// Gets or sets the current instance path
        /// </summary>
        public Identifier InstancePath { get; }

        /// <summary>
        /// Gets or sets the current subcircuit definition path
        /// </summary>
        public Identifier DefinitionPath { get; }

        /// <summary>
        /// The current parameters
        /// If no parameters are active, the global parameters are returned
        /// </summary>
        public Dictionary<Identifier, double> Parameters { get; }

        /// <summary>
        /// Gets the currently active node map
        /// </summary>
        public Dictionary<Identifier, Identifier> NodeMap { get; }

        /// <summary>
        /// Get the global variables
        /// </summary>
        public HashSet<Identifier> Globals { get; }

        /// <summary>
        /// Private variables
        /// </summary>
        private Dictionary<Identifier, double> globalparameters;

        /// <summary>
        /// Constructor for the root path
        /// </summary>
        public SubcircuitPath()
        {
            // Root path? Ignore all other parameters
            globalparameters = new Dictionary<Identifier, double>();
            Parameters = globalparameters;
            InstancePath = null;
            DefinitionPath = null;
            NodeMap = new Dictionary<Identifier, Identifier>();
            Globals = new HashSet<Identifier>();
            Globals.Add(new Identifier("0"));

            // Add globals to the nodemap
            foreach (var id in Globals)
                NodeMap.Add(id, id);
        }

        /// <summary>
        /// Constructor when reading subcircuit definitions
        /// </summary>
        /// <param name="parent">Parent path</param>
        /// <param name="definition">Subcircuit definition</param>
        public SubcircuitPath(SubcircuitPath parent, SubcircuitDefinition definition)
        {
            // Same global parameters
            globalparameters = parent.globalparameters;

            // No parameters or instances
            Parameters = null; // Should not be used!
            InstancePath = null;

            // Build the definition path
            if (parent.DefinitionPath != null)
                DefinitionPath = parent.DefinitionPath.Grow(definition.Name.Name);
            else
                DefinitionPath = definition.Name;

            // No node map
            NodeMap = null; // Should not be used!
        }

        /// <summary>
        /// Constructor when reading subcircuit instances
        /// </summary>
        /// <param name="parent">Parent</param>
        /// <param name="subckt">Subcircuit definition</param>
        public SubcircuitPath(Netlist netlist, SubcircuitPath parent, Subcircuit subckt)
        {
            // Same global parameters
            globalparameters = parent.globalparameters;

            // Same globals
            Globals = parent.Globals;

            // Build parameters
            Parameters = GenerateParameters(netlist, subckt.Definition, subckt.Parameters);

            // Build instance path
            if (parent.InstancePath != null)
                InstancePath = parent.InstancePath.Grow(subckt.Name.Name);
            else
                InstancePath = subckt.Name;

            // Build the definition path
            if (parent.DefinitionPath != null)
                DefinitionPath = parent.DefinitionPath.Grow(subckt.Definition.Name.Name);
            else
                DefinitionPath = subckt.Definition.Name;

            // Node map
            NodeMap = GenerateNodeMap(subckt.Definition, subckt.Pins);

            // Add globals to the nodemap
            foreach (var id in Globals)
            {
                if (!NodeMap.ContainsKey(id))
                    NodeMap.Add(id, id);
            }

            // Check for recursively called paths
            if (parent.DefinitionPath != null)
            {
                for (int i = 0; i < parent.DefinitionPath.Path.Length; i++)
                {
                    if (parent.DefinitionPath.Path[i] == subckt.Definition.Name.Name)
                        throw new ParseException($"Subcircuit definition {subckt.Definition} is called recursively");
                }
            }
        }

        /// <summary>
        /// Find a model using the current scope rules
        /// </summary>
        /// <param name="id">Identifier of the model</param>
        /// <returns></returns>
        public T FindModel<T>(Entities obj, Identifier id) where T : class, Entity
        {
            Entity co;
            T model = null;
            Identifier orig = id;

            switch (ModelScope)
            {
                case ScopeRule.Descend:

                    // Find the identifier in all parent paths, starting at the current level
                    while (id != null)
                    {
                        // Try to find the subcircuit
                        if (obj.TryGetObject(id, out co))
                        {
                            model = co as T;
                            if (model != null)
                                return model;
                        }

                        // We didn't find the model yet, so try the parent path
                        id = id.Shrink();
                    }
                    break;

                case ScopeRule.GlobalLocal:

                    // Find the model locally
                    if (obj.TryGetObject(id, out co))
                    {
                        model = co as T;
                        if (model != null)
                            return model;
                    }

                    // Find the model globally
                    if (id.Path.Length > 1)
                    {
                        id = new Identifier(id.Name);
                        if (obj.TryGetObject(id, out co))
                        {
                            model = co as T;
                            if (model != null)
                                return model;
                        }
                    }
                    break;
            }

            throw new ParseException($"Cannot find model \"{orig}\"");
        }

        /// <summary>
        /// Find a subcircuit definition using the current scope rules
        /// </summary>
        /// <param name="id">Subcircuit definition identifier</param>
        /// <returns></returns>
        public SubcircuitDefinition FindDefinition(Dictionary<Identifier, SubcircuitDefinition> definitions, Identifier id)
        {
            SubcircuitDefinition result = null;
            Identifier orig = id;

            switch (DefinitionScope)
            {
                case ScopeRule.Descend:

                    while (id != null)
                    {
                        // Try to find the definition
                        if (definitions.TryGetValue(id, out result))
                            return result;

                        // Not found, go to the parent path
                        id = id.Shrink();
                    }
                    break;

                case ScopeRule.GlobalLocal:

                    // Try to find the definition locally
                    if (definitions.TryGetValue(id, out result))
                        return result;

                    // Try to find the definition globally
                    if (id.Path.Length > 1)
                    {
                        id = new Identifier(id.Name);
                        if (definitions.TryGetValue(id, out result))
                            return result;
                    }
                    break;
            }
            throw new ParseException($"Cannot find subcircuit \"{orig}\"");
        }

        /// <summary>
        /// Find the parameters
        /// </summary>
        /// <param name="definition">Subcircuit definition</param>
        /// <param name="parameters">Parameters</param>
        /// <returns></returns>
        private Dictionary<Identifier, double> GenerateParameters(Netlist netlist, SubcircuitDefinition definition, Dictionary<Identifier, Token> parameters)
        {
            // Our new parameters
            Dictionary<Identifier, double> np = new Dictionary<Identifier, double>();

            // Add local parameters
            if (parameters != null)
            {
                foreach (var item in parameters)
                    np.Add(item.Key, netlist.ParseDouble(item.Value));
            }

            // Add default parameters
            if (definition.Defaults != null)
            {
                foreach (var item in definition.Defaults)
                {
                    if (!np.ContainsKey(item.Key))
                        np.Add(item.Key, netlist.ParseDouble(item.Value));
                }
            }

            // Other parameters
            if (Parameters != null)
            {
                switch (ParameterScope)
                {
                    case ScopeRule.Descend:

                        // Copy all parameters from the previous node
                        foreach (var item in Parameters)
                        {
                            if (!np.ContainsKey(item.Key))
                                np.Add(item.Key, item.Value);
                        }
                        break;

                    case ScopeRule.GlobalLocal:

                        // Only copy the global parameters
                        foreach (var item in globalparameters)
                        {
                            if (!np.ContainsKey(item.Key))
                                np.Add(item.Key, item.Value);
                        }
                        break;
                }
            }

            // Return results
            return np;
        }

        /// <summary>
        /// Find the nodemap for a subcircuit definition
        /// </summary>
        /// <param name="definition">Subcircuit definition</param>
        /// <param name="pins">The new pins</param>
        /// <returns></returns>
        private Dictionary<Identifier, Identifier> GenerateNodeMap(SubcircuitDefinition definition, List<Identifier> pins)
        {
            // Initialize
            Dictionary<Identifier, Identifier> nodemap = new Dictionary<Identifier, Identifier>();

            // This is actually simple, just a one on one relation
            for (int i = 0; i < pins.Count; i++)
                nodemap.Add(definition.Pins[i], pins[i]);
            return nodemap;
        }
    }
}
