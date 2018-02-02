using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Circuits;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A class that represents a circuit component/device.
    /// It can be connected in a circuit and it also has parameters.
    /// </summary>
    public abstract class Component : Entity
    {
        /// <summary>
        /// Private variables
        /// </summary>
        Identifier[] connections;
        int[] indices;

        /// <summary>
        /// Gets the number of nodes
        /// </summary>
        public virtual int PinCount => connections.Length;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the component</param>
        protected Component(Identifier name, int nodeCount)
            : base(name)
        {
            // Initialize
            if (nodeCount > 0)
            {
                connections = new Identifier[nodeCount];
                indices = new int[nodeCount];
            }
            else
            {
                connections = null;
                indices = null;
            }
        }

        /// <summary>
        /// Gets the model of the circuit component (if any)
        /// </summary>
        public Entity Model { get; protected set; } = null;

        /// <summary>
        /// Connect the component in the circuit
        /// </summary>
        /// <param name="nodes"></param>
        public virtual void Connect(params Identifier[] nodes)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));
            if (nodes.Length != connections.Length)
                throw new CircuitException("{0}: Node count mismatch. {1} given, {2} expected.".FormatString(Name, nodes.Length, connections.Length));
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] == null)
                    throw new ArgumentNullException("node " + (i + 1));
                connections[i] = nodes[i];
            }
        }

        /// <summary>
        /// Gets a behavior
        /// </summary>
        /// <typeparam name="T">Base behavior</typeparam>
        /// <param name="pool">Pool of all behaviors</param>
        /// <returns></returns>
        public override T GetBehavior<T>(BehaviorPool pool)
        {
            T behavior = base.GetBehavior<T>(pool);

            // Extra functionality for behaviors that can be connected
            if (behavior is IConnectedBehavior cb)
            {
                cb.Connect(indices);
            }
            return behavior;
        }

        /// <summary>
        /// Build the data provider for setting up behaviors
        /// </summary>
        /// <param name="pool">All behaviors</param>
        /// <returns></returns>
        protected override SetupDataProvider BuildSetupDataProvider(BehaviorPool pool)
        {
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));
            var provider = base.BuildSetupDataProvider(pool);

            // Add our model parameters and behaviors
            if (Model != null)
            {
                provider.Add(Model.ParameterSets);
                provider.Add(pool.GetEntityBehaviors(Model.Name));
            }

            return provider;
        }

        /// <summary>
        /// Gets the connection of the component
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns></returns>
        public virtual Identifier GetNode(int index)
        {
            if (index < 0 || index >= connections.Length)
                throw new CircuitException("Invalid node {0}".FormatString(index));
            return connections[index];
        }

        /// <summary>
        /// Gets the node index of the component
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns></returns>
        public virtual int GetNodeIndex(int index)
        {
            if (index < 0 || index >= connections.Length)
                throw new CircuitException("Invalid node {0}".FormatString(index));
            return indices[index];
        }

        /// <summary>
        /// Helper function for binding nodes to the circuit
        /// </summary>
        /// <param name="circuit"></param>
        /// <returns></returns>
        protected Node[] BindNodes(Circuit circuit)
        {
            if (circuit == null)
                throw new ArgumentNullException(nameof(circuit));

            // Map connected nodes
            Node[] nodes = new Node[connections.Length];
            for (int i = 0; i < connections.Length; i++)
            {
                nodes[i] = circuit.Nodes.Map(connections[i]);
                indices[i] = nodes[i].Index;
            }

            // Return all nodes
            return nodes;
        }
    }
}
