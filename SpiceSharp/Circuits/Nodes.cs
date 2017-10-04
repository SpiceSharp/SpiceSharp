using System.Collections.Generic;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// Contains and manages circuit nodes.
    /// </summary>
    public class Nodes
    {
        /// <summary>
        /// Make nodes case-insensitive
        /// </summary>
        public bool CaseInsensitive = true;

        /// <summary>
        /// Global nodes that escape prefixes
        /// </summary>
        public HashSet<string> Globals { get; } = new HashSet<string>();

        /// <summary>
        /// Private variables
        /// </summary>
        private List<CircuitNode> nodes = new List<CircuitNode>();
        private Dictionary<string, CircuitNode> map = new Dictionary<string, CircuitNode>();
        private bool locked = false;

        private Stack<Dictionary<string, CircuitNode>> pinmap = new Stack<Dictionary<string, CircuitNode>>();
        private Stack<string> prefix = new Stack<string>();

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
            Ground.Name = "0";
            map.Add(Ground.Name, Ground);

            // Add a few alias
            map.Add("gnd", Ground);
            Globals.Add("gnd");
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
        /// <param name="name">The node name</param>
        /// <param name="type">The node type</param>
        /// <returns></returns>
        public CircuitNode Map(string name, CircuitNode.NodeType type = CircuitNode.NodeType.Voltage)
        {
            if (locked)
                throw new CircuitException("Nodes locked, mapping is not allowed");

            if (CaseInsensitive)
                name = name.ToLower();

            // Transform the name if necessary
            if (prefix.Count > 0)
            {
                if (pinmap.Peek().ContainsKey(name))
                    return pinmap.Peek()[name];
                else if (name != Ground.Name && !Globals.Contains(name))
                    name = prefix.Peek() + name;
            }

            // Check the node
            if (map.ContainsKey(name))
                return map[name];

            var node = Create(name, type);
            map.Add(name, node);
            return node;
        }

        /// <summary>
        /// Create a node without mapping it to the circuit
        /// </summary>
        /// <param name="name">The node name</param>
        /// <param name="type">The node type</param>
        /// <returns></returns>
        public CircuitNode Create(string name, CircuitNode.NodeType type = CircuitNode.NodeType.Voltage)
        {
            var node = new CircuitNode(type, nodes.Count + 1);
            node.Name = name;
            nodes.Add(node);
            return node;
        }

        /// <summary>
        /// Push a new (temporary) pin map of nodes
        /// This can be used to temporarely map certain nodes to possibly already existing nodes
        /// </summary>
        /// <param name="map">A dictionary of nodes that should be mapped to other nodes</param>
        public void PushPinMap(string addprefix, Dictionary<string, string> map)
        {
            // Find the new prefix
            if (CaseInsensitive)
                addprefix = addprefix.ToLower();
            addprefix = (prefix.Count > 0 ? prefix.Peek() : "") + addprefix;

            var nmap = new Dictionary<string, CircuitNode>();
            foreach (var item in map)
            {
                string node = item.Key;
                if (CaseInsensitive)
                    node = node.ToLower();
                nmap.Add(node, Map(item.Value));
            }

            prefix.Push(addprefix);
            pinmap.Push(nmap);
        }

        /// <summary>
        /// Remove the last (temporary) pin map and go back to the previous pin map
        /// </summary>
        public void PopPinMap()
        {
            // Restore to previous pin map state
            prefix.Pop();
            pinmap.Pop();
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
