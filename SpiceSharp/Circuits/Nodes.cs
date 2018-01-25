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
        /// Private variables
        /// </summary>
        List<Node> nodes = new List<Node>();
        Dictionary<Identifier, Node> map = new Dictionary<Identifier, Node>();
        bool locked;

        /// <summary>
        /// The initial conditions
        /// This is the initial value when simulation starts
        /// </summary>
        public Dictionary<Identifier, double> IC { get; } = new Dictionary<Identifier, double>();

        /// <summary>
        /// The nodeset values
        /// This value can help convergence
        /// </summary>
        public Dictionary<Identifier, double> Nodeset { get; } = new Dictionary<Identifier, double>();

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
            map.Add(Ground.Name, Ground);
            map.Add(new Identifier("gnd"), Ground);

            // Unlock
            locked = false;
        }

        /// <summary>
        /// Get a node by identifier
        /// </summary>
        /// <param id="id">Identifier</param>
        /// <returns></returns>
        public Node this[Identifier id]
        {
            get => map[id];
        }

        /// <summary>
        /// Find a node by index
        /// </summary>
        /// <param id="index"></param>
        /// <returns></returns>
        public Node this[int index] => nodes[index];

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
        public Node Map(Identifier id, Node.NodeType type = Node.NodeType.Voltage)
        {
            if (locked)
                throw new CircuitException("Nodes are locked, mapping is not allowed anymore");

            // Check the node
            if (map.ContainsKey(id))
                return map[id];

            var node = new Node(id, type, nodes.Count + 1);
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
        public Node Create(Identifier id, Node.NodeType type = Node.NodeType.Voltage)
        {
            int index = nodes.Count + 1;
            var node = new Node(id, type, index);
            nodes.Add(node);
            return node;
        }

        /// <summary>
        /// Check if a node exists
        /// </summary>
        /// <param id="id">Identifier</param>
        /// <returns></returns>
        public bool Contains(Identifier id) => map.ContainsKey(id);

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

            // Setup ground node
            map.Clear();
            map.Add(Ground.Name, Ground);
            map.Add(new Identifier("gnd"), Ground);

            // Unlock
            locked = false;
        }
    }
}
