using System.Collections.Generic;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// Contains and manages circuit nodes.
    /// </summary>
    public class CircuitNodes
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private List<CircuitNode> nodes = new List<CircuitNode>();
        private Dictionary<CircuitIdentifier, CircuitNode> map = new Dictionary<CircuitIdentifier, CircuitNode>();
        private bool locked = false;

        /// <summary>
        /// The initial conditions
        /// This is the initial value when simulation starts
        /// </summary>
        public Dictionary<CircuitIdentifier, double> IC { get; } = new Dictionary<CircuitIdentifier, double>();

        /// <summary>
        /// The nodeset values
        /// This value can help convergence
        /// </summary>
        public Dictionary<CircuitIdentifier, double> Nodeset { get; } = new Dictionary<CircuitIdentifier, double>();

        /// <summary>
        /// Gets the ground node
        /// </summary>
        public CircuitNode Ground { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public CircuitNodes()
        {
            Ground = new CircuitNode(new CircuitIdentifier("0"), CircuitNode.NodeType.Voltage);
            map.Add(Ground.Name, Ground);
            map.Add(new CircuitIdentifier("gnd"), Ground);
        }

        /// <summary>
        /// Get a node by identifier
        /// </summary>
        /// <param id="id">Identifier</param>
        /// <returns></returns>
        public CircuitNode this[CircuitIdentifier id]
        {
            get => map[id];
        }

        /// <summary>
        /// Find a node by index
        /// </summary>
        /// <param id="index"></param>
        /// <returns></returns>
        public CircuitNode this[int index] => nodes[index];

        /// <summary>
        /// Get the node count
        /// </summary>
        public int Count => nodes.Count;

        /// <summary>
        /// Map a node in the circuit
        /// </summary>
        /// <param id="id">Identifier</param>
        /// <param id="type">Type</param>
        /// <returns></returns>
        public CircuitNode Map(CircuitIdentifier id, CircuitNode.NodeType type = CircuitNode.NodeType.Voltage)
        {
            if (locked)
                throw new CircuitException("Nodes are locked, mapping is not allowed");

            // Check the node
            if (map.ContainsKey(id))
                return map[id];

            var node = new CircuitNode(id, type, nodes.Count + 1);
            nodes.Add(node);
            map.Add(id, node);
            return node;
        }

        /// <summary>
        /// Create a new node without reference
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <param name="type">Type</param>
        /// <returns></returns>
        public CircuitNode Create(CircuitIdentifier id, CircuitNode.NodeType type = CircuitNode.NodeType.Voltage)
        {
            var node = new CircuitNode(id, type, nodes.Count + 1);
            nodes.Add(node);
            return node;
        }

        /// <summary>
        /// Check if a node exists
        /// </summary>
        /// <param id="id">Identifier</param>
        /// <returns></returns>
        public bool Contains(CircuitIdentifier id) => map.ContainsKey(id);

        /// <summary>
        /// Avoid changing to the internal structure by locking the node list
        /// </summary>
        public void Lock()
        {
            locked = true;
        }

        /// <summary>
        /// Clear all nodes
        /// </summary>
        public void Clear()
        {
            nodes.Clear();
            map.Clear();
            map.Add(Ground.Name, Ground);
            map.Add(new CircuitIdentifier("gnd"), Ground);
            locked = false;
        }
    }
}
