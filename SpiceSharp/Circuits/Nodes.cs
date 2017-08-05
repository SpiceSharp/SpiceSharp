using System.Collections.Generic;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// A class that keeps track of all circuit nodes
    /// </summary>
    public class Nodes
    {
        /// <summary>
        /// Make nodes case-insensitive
        /// </summary>
        public bool CaseInsensitive = true;

        /// <summary>
        /// Private variables
        /// </summary>
        private List<CircuitNode> nodes = new List<CircuitNode>();
        private Dictionary<string, CircuitNode> map = new Dictionary<string, CircuitNode>();
        private bool locked = false;

        /// <summary>
        /// The initial conditions
        /// This is the initial value when simulation starts
        /// </summary>
        public Dictionary<string, double> IC { get; } = new Dictionary<string, double>();

        /// <summary>
        /// The nodeset values
        /// This value can help convergence
        /// </summary>
        public Dictionary<string, double> Nodeset { get; } = new Dictionary<string, double>();

        /// <summary>
        /// Gets the ground node
        /// </summary>
        public CircuitNode Ground { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Nodes()
        {
            Ground = new CircuitNode(CircuitNode.NodeType.Voltage);
            Ground.Name = "gnd";
            map.Add(Ground.Name, Ground);
            map.Add("0", Ground);
        }

        /// <summary>
        /// Get a node by name
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public CircuitNode this[string node]
        {
            get
            {
                if (CaseInsensitive)
                    node = node.ToLower();
                if (map.ContainsKey(node))
                    return map[node];
                return null;
            }
        }

        /// <summary>
        /// Find a node by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public CircuitNode this[int index] => nodes[index];

        /// <summary>
        /// Get the node count
        /// </summary>
        public int Count => nodes.Count;

        /// <summary>
        /// Map a node in the circuit
        /// </summary>
        /// <param name="name">Can be a node name or NULL for an internal node that does not need a name</param>
        /// <param name="type">The node type</param>
        /// <returns></returns>
        public CircuitNode Map(string name, CircuitNode.NodeType type = CircuitNode.NodeType.Voltage)
        {
            if (locked)
                throw new CircuitException($"Nodes locked, mapping is not allowed");

            // Check for an existing node
            if (name != null)
            {
                if (CaseInsensitive)
                    name = name.ToLower();
                if (map.ContainsKey(name))
                    return map[name];
            }

            // Create a new node
            var node = new CircuitNode(type, nodes.Count + 1);
            node.Name = name;
            nodes.Add(node);

            // Keep a reference if it is not an internal node (null)
            if (name != null)
                map.Add(name, node);
            return node;
        }

        /// <summary>
        /// Check if a node exists
        /// </summary>
        /// <param name="node">The node name</param>
        /// <returns></returns>
        public bool Contains(string node)
        {
            if (node == null)
                return false;
            if (CaseInsensitive)
                node = node.ToLower();
            return map.ContainsKey(node);
        }

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
            map.Add("GND", Ground);
            map.Add("0", Ground);
            locked = false;
        }
    }
}
