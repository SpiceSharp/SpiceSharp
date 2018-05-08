using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Contains and manages circuit nodes.
    /// </summary>
    public class VariableSet
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private readonly List<Variable> _unknowns = new List<Variable>();
        private readonly Dictionary<Identifier, Variable> _map = new Dictionary<Identifier, Variable>();
        private bool _locked;

        /// <summary>
        /// The initial conditions
        /// This is the initial value when simulation starts
        /// </summary>
        public Dictionary<Identifier, double> InitialConditions { get; } = new Dictionary<Identifier, double>();

        /// <summary>
        /// The nodeset values
        /// This value can help convergence
        /// </summary>
        public Dictionary<Identifier, double> NodeSets { get; } = new Dictionary<Identifier, double>();

        /// <summary>
        /// Gets the ground node
        /// </summary>
        public Variable Ground { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public VariableSet()
        {
            // Setup the ground node
            Ground = new Variable("0", 0);
            _map.Add(Ground.Name, Ground);
            _map.Add("GND", Ground);

            // Unlock
            _locked = false;
        }

        /// <summary>
        /// Find a node by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Variable this[int index] => _unknowns[index];

        /// <summary>
        /// Gets the node count
        /// </summary>
        public int Count => _unknowns.Count;

        /// <summary>
        /// Map the voltage of a node
        /// If the variable already exists, then that variable is returned
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <param name="type">Type</param>
        /// <returns></returns>
        public Variable MapNode(Identifier id, VariableType type)
        {
            if (_locked)
                throw new CircuitException("Nodes are locked, mapping is not allowed anymore");

            // Check the node
            if (_map.ContainsKey(id))
                return _map[id];

            var node = new Variable(id, type, _unknowns.Count + 1);
            _unknowns.Add(node);
            _map.Add(id, node);
            return node;
        }

        /// <summary>
        /// Map a node in the circuit
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns></returns>
        public Variable MapNode(Identifier id) => MapNode(id, VariableType.Voltage);

        /// <summary>
        /// Make an alias for the voltage at a node
        /// </summary>
        /// <param name="original">Original name</param>
        /// <param name="alias">The alias that will be turned into this alias</param>
        public void AliasNode(Identifier original, Identifier alias)
        {
            var originalNode = _map[original];
            _map.Add(alias, originalNode);
        }

        /// <summary>
        /// Create a new variable
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <param name="type">Type</param>
        /// <returns></returns>
        public Variable Create(Identifier id, VariableType type)
        {
            // Create the node
            int index = _unknowns.Count + 1;
            var node = new Variable(id, type, index);
            _unknowns.Add(node);
            return node;
        }

        /// <summary>
        /// Create a new node without reference
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns></returns>
        public Variable Create(Identifier id) => Create(id, VariableType.Voltage);

        /// <summary>
        /// Check if a node voltage exists
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns></returns>
        public bool ContainsNode(Identifier id) => _map.ContainsKey(id);

        /// <summary>
        /// Check if an unknown exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Contains(Identifier id) => _unknowns.Exists(node => node.Name.Equals(id));

        /// <summary>
        /// Try to get a node
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <param name="node">Node</param>
        /// <returns></returns>
        public bool TryGetNode(Identifier id, out Variable node) => _map.TryGetValue(id, out node);

        /// <summary>
        /// Get a node voltage variable
        /// If the node voltage does not exist, an exception will be thrown
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns></returns>
        public Variable GetNode(Identifier id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (_map.TryGetValue(id, out var result))
                return result;
            throw new CircuitException("Could not find node {0}".FormatString(id));
        }

        /// <summary>
        /// Get an unknown
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns>Returns null if no unknown was found</returns>
        public Variable GetVariable(Identifier id) => _unknowns.FirstOrDefault(node => node.Name.Equals(id));

        /// <summary>
        /// Get unknowns
        /// </summary>
        public IEnumerable<Variable> GetVariables() => _unknowns;

        /// <summary>
        /// Avoid changing to the internal structure by locking the node list
        /// </summary>
        public void Lock() => _locked = true;

        /// <summary>
        /// Clear all nodes
        /// </summary>
        public void Clear()
        {
            _unknowns.Clear();

            // Setup ground node
            _map.Clear();
            _map.Add(Ground.Name, Ground);
            _map.Add("GND", Ground);

            // Unlock
            _locked = false;
        }
    }
}
