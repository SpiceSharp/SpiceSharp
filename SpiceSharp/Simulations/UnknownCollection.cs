using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Contains and manages circuit nodes.
    /// </summary>
    [Serializable]
    public class UnknownCollection
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private readonly List<Unknown> _unknowns = new List<Unknown>();
        private readonly Dictionary<Identifier, Unknown> _map = new Dictionary<Identifier, Unknown>();
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
        public Unknown Ground { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public UnknownCollection()
        {
            // Setup the ground node
            Ground = new Unknown("0", 0);
            _map.Add(Ground.Name, Ground);
            _map.Add("GND", Ground);

            // Unlock
            _locked = false;
        }

        /// <summary>
        /// Gets a node by identifier
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns></returns>
        public Unknown this[Identifier id] => _map[id];

        /// <summary>
        /// Find a node by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Unknown this[int index] => _unknowns[index];

        /// <summary>
        /// Gets the node count
        /// </summary>
        public int Count => _unknowns.Count;

        /// <summary>
        /// Map a node in the circuit
        /// If the node already exists, then that node is returned
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <param name="type">Type</param>
        /// <returns></returns>
        public Unknown MapNode(Identifier id, UnknownType type)
        {
            if (_locked)
                throw new CircuitException("Nodes are locked, mapping is not allowed anymore");

            // Check the node
            if (_map.ContainsKey(id))
                return _map[id];

            var node = new Unknown(id, type, _unknowns.Count + 1);
            _unknowns.Add(node);
            _map.Add(id, node);
            return node;
        }

        /// <summary>
        /// Map a node in the circuit
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns></returns>
        public Unknown MapNode(Identifier id) => MapNode(id, UnknownType.Voltage);

        /// <summary>
        /// Make an alias for a node
        /// </summary>
        /// <param name="original">Original name</param>
        /// <param name="alias">The alias that will be turned into this alias</param>
        public void AliasNode(Identifier original, Identifier alias)
        {
            var originalNode = _map[original];
            _map.Add(alias, originalNode);
        }

        /// <summary>
        /// Create a new unknown
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <param name="type">Type</param>
        /// <returns></returns>
        public Unknown Create(Identifier id, UnknownType type)
        {
            // Create the node
            int index = _unknowns.Count + 1;
            var node = new Unknown(id, type, index);
            _unknowns.Add(node);
            return node;
        }

        /// <summary>
        /// Create a new node without reference
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns></returns>
        public Unknown Create(Identifier id) => Create(id, UnknownType.Voltage);

        /// <summary>
        /// Check if a node exists
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns></returns>
        public bool ContainsNode(Identifier id) => _map.ContainsKey(id);

        /// <summary>
        /// Check if an unknown exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ContainsUnknown(Identifier id) => _unknowns.Exists(node => node.Name.Equals(id));

        /// <summary>
        /// Try to get a node
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <param name="node">Node</param>
        /// <returns></returns>
        public bool TryGetNode(Identifier id, out Unknown node) => _map.TryGetValue(id, out node);

        /// <summary>
        /// Get an unknown
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns>Returns null if no unknown was found</returns>
        public Unknown GetUnknown(Identifier id) => _unknowns.FirstOrDefault(node => node.Name.Equals(id));

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
