using System;
using System.Collections.Generic;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// Contains and manages circuit nodes.
    /// </summary>
    public class Nodes : ICloneable
    {
        /// <summary>
        /// Private variables
        /// </summary>
        List<Node> _nodes = new List<Node>();
        Dictionary<Identifier, Node> _map = new Dictionary<Identifier, Node>();
        bool _locked;

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
        public Node Ground { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Nodes()
        {
            // Setup the ground node
            Ground = new Node(new Identifier("0"), 0);
            _map.Add(Ground.Name, Ground);
            _map.Add(new Identifier("GND"), Ground);

            // Unlock
            _locked = false;
        }

        /// <summary>
        /// Gets a node by identifier
        /// </summary>
        /// <param id="id">Identifier</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
        public Node this[Identifier id]
        {
            get => _map[id];
        }

        /// <summary>
        /// Find a node by index
        /// </summary>
        /// <param id="index"></param>
        /// <returns></returns>
        public Node this[int index] => _nodes[index];

        /// <summary>
        /// Gets the node count
        /// </summary>
        public int Count => _nodes.Count;

        /// <summary>
        /// Map a node in the circuit
        /// </summary>
        /// <param id="id">Identifier</param>
        /// <param id="type">Type</param>
        /// <returns></returns>
        public Node Map(Identifier id, Node.NodeType type)
        {
            if (_locked)
                throw new CircuitException("Nodes are locked, mapping is not allowed anymore");

            // Check the node
            if (_map.ContainsKey(id))
                return _map[id];

            var node = new Node(id, type, _nodes.Count + 1);
            _nodes.Add(node);
            _map.Add(id, node);
            return node;
        }

        /// <summary>
        /// Map a node in the circuit
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns></returns>
        public Node Map(Identifier id) => Map(id, Node.NodeType.Voltage);

        /// <summary>
        /// Create a new node without reference
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <param name="type">Type</param>
        /// <returns></returns>
        public Node Create(Identifier id, Node.NodeType type)
        {
            int index = _nodes.Count + 1;
            var node = new Node(id, type, index);
            _nodes.Add(node);
            return node;
        }

        /// <summary>
        /// Create a new node without reference
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns></returns>
        public Node Create(Identifier id) => Create(id, Node.NodeType.Voltage);

        /// <summary>
        /// Check if a node exists
        /// </summary>
        /// <param id="id">Identifier</param>
        /// <returns></returns>
        public bool Contains(Identifier id) => _map.ContainsKey(id);

        /// <summary>
        /// Try to get a node
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <param name="node">Node</param>
        /// <returns></returns>
        public bool TryGetNode(Identifier id, out Node node) => _map.TryGetValue(id, out node);

        /// <summary>
        /// Avoid changing to the internal structure by locking the node list
        /// </summary>
        public void Lock()
        {
            _locked = true;
        }

        /// <summary>
        /// Clear all nodes
        /// </summary>
        public void Clear()
        {
            _nodes.Clear();

            // Setup ground node
            _map.Clear();
            _map.Add(Ground.Name, Ground);
            _map.Add(new Identifier("GND"), Ground);

            // Unlock
            _locked = false;
        }

        /// <summary>
        /// Clone the nodes
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            // Create a new object
            var cloned = new Nodes();

            // Copy node list and map
            foreach (var node in _nodes)
            {
                var clonedNode = (Node)node.Clone();
                cloned._nodes.Add(clonedNode);
                cloned._map.Add(clonedNode.Name, clonedNode);
            }

            // Clone initial conditions and nodesets
            foreach (var ic in InitialConditions)
                cloned.InitialConditions.Add(ic.Key, ic.Value);
            foreach (var ns in NodeSets)
                cloned.NodeSets.Add(ns.Key, ns.Value);

            return cloned;
        }
    }
}
