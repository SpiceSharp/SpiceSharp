using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A class that represents a circuit component/device.
    /// It can be connected in a circuit and it also has parameters.
    /// </summary>
    public abstract class CircuitComponent : ICircuitObject
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private CircuitIdentifier[] connections = null;
        private int[] indices = null;

        /// <summary>
        /// Get the number of nodes
        /// </summary>
        public virtual int PinCount => connections.Length;

        /// <summary>
        /// Get the name of the component
        /// </summary>
        public CircuitIdentifier Name { get; }

        /// <summary>
        /// This parameter can change the order in which components are traversed
        /// Components with a higher priority will get the first chance to execute their methods
        /// </summary>
        public int Priority { get; protected set; } = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the component</param>
        public CircuitComponent(CircuitIdentifier name, int nodecount)
        {
            // Initialize
            if (nodecount > 0)
            {
                connections = new CircuitIdentifier[nodecount];
                indices = new int[nodecount];
            }
            else
            {
                connections = null;
                indices = null;
            }
            Name = name;
        }

        /// <summary>
        /// Get the model of the circuit component (if any)
        /// </summary>
        public ICircuitObject Model { get; protected set; } = null;

        /// <summary>
        /// Connect the component in the circuit
        /// </summary>
        /// <param name="nodes"></param>
        public virtual void Connect(params CircuitIdentifier[] nodes)
        {
            if (nodes.Length != connections.Length)
                throw new CircuitException($"{Name}: Node count mismatch. {nodes.Length} given, {connections.Length} expected.");
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] == null)
                    throw new ArgumentNullException("node " + (i + 1));
                connections[i] = nodes[i];
            }
        }

        /// <summary>
        /// Get the connection of the component
        /// </summary>
        /// <param name="i">The index</param>
        /// <returns></returns>
        public virtual CircuitIdentifier GetNode(int i)
        {
            if (i < 0 || i >= connections.Length)
                throw new IndexOutOfRangeException();
            return connections[i];
        }

        /// <summary>
        /// Get the node index of the component
        /// </summary>
        /// <param name="i">The index</param>
        /// <returns></returns>
        public virtual int GetNodeIndex(int i)
        {
            if (i < 0 || i >= connections.Length)
                throw new IndexOutOfRangeException();
            return indices[i];
        }

        /// <summary>
        /// Helper function for binding nodes to the circuit
        /// </summary>
        /// <param name="ckt"></param>
        /// <param name="extra"></param>
        /// <returns></returns>
        protected CircuitNode[] BindNodes(Circuit ckt)
        {
            // Map connected nodes
            CircuitNode[] nodes = new CircuitNode[connections.Length];
            for (int i = 0; i < connections.Length; i++)
            {
                nodes[i] = ckt.Nodes.Map(connections[i]);
                indices[i] = nodes[i].Index;
            }

            // Return all nodes
            return nodes;
        }

        /// <summary>
        /// Helper function for binding an extra equation
        /// </summary>
        /// <param name="ckt">The circuit</param>
        /// <param name="type">The type</param>
        /// <returns></returns>
        protected CircuitNode CreateNode(Circuit ckt, CircuitIdentifier name, CircuitNode.NodeType type = CircuitNode.NodeType.Voltage)
        {
            // Map the extra equations
            return ckt.Nodes.Create(name, type);
        }

        /// <summary>
        /// Setup the component
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public abstract void Setup(Circuit ckt);

        /// <summary>
        /// Unsetup/destroy the component
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public virtual void Unsetup(Circuit ckt)
        {
            // Do nothing
        }
    }
}
