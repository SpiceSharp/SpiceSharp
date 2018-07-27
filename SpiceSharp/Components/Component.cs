using System;
using System.Collections.Generic;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;

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
        private readonly Identifier[] _connections;

        /// <summary>
        /// Gets the number of nodes
        /// </summary>
        public virtual int PinCount => _connections.Length;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the component</param>
        /// <param name="nodeCount">Node count</param>
        protected Component(Identifier name, int nodeCount)
            : base(name)
        {
            // Initialize
            _connections = nodeCount > 0 ? new Identifier[nodeCount] : null;
        }

        /// <summary>
        /// Gets the model of the circuit component (if any)
        /// </summary>
        public Entity Model { get; protected set; } = null;

        /// <summary>
        /// Connect the component in the circuit
        /// </summary>
        /// <param name="nodes"></param>
        public void Connect(params Identifier[] nodes)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));
            if (nodes.Length != _connections.Length)
                throw new CircuitException("{0}: Node count mismatch. {1} given, {2} expected.".FormatString(Name, nodes.Length, _connections.Length));
            for (var i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] == null)
                    throw new ArgumentNullException("node " + (i + 1));
                _connections[i] = nodes[i];
            }
        }

        /// <summary>
        /// Gets a behavior
        /// </summary>
        /// <typeparam name="T">Base behavior</typeparam>
        /// <param name="simulation"></param>
        /// <returns></returns>
        public override T CreateBehavior<T>(Simulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            var behavior = base.CreateBehavior<T>(simulation);

            // Extra functionality for behaviors that can be connected
            if (behavior is IConnectedBehavior cb)
            {
                // Connect the behavior
                var indexes = ApplyConnections(simulation.Nodes);
                cb.Connect(indexes);
            }
            return behavior;
        }

        /// <summary>
        /// Build the data provider for setting up behaviors
        /// </summary>
        /// <returns></returns>
        protected override SetupDataProvider BuildSetupDataProvider(ParameterPool parameters, BehaviorPool behaviors)
        {
            var provider = base.BuildSetupDataProvider(parameters, behaviors);

            // Add our model parameters and behaviors
            if (Model != null)
            {
                provider.Add("model", parameters.GetEntityParameters(Model.Name));
                provider.Add("model", behaviors.GetEntityBehaviors(Model.Name));
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
            if (index < 0 || index >= _connections.Length)
                throw new CircuitException("Invalid node {0}".FormatString(index));
            return _connections[index];
        }

        /// <summary>
        /// Update the indices for the component
        /// </summary>
        /// <param name="nodes">Nodes</param>
        /// <returns></returns>
        protected int[] ApplyConnections(VariableSet nodes)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));

            // Map connected nodes
            var indexes = new int[_connections.Length];
            for (var i = 0; i < _connections.Length; i++)
                indexes[i] = nodes.MapNode(_connections[i]).Index;
            return indexes;
        }

        /// <summary>
        /// Get the node indices in order
        /// </summary>
        /// <param name="nodes">Nodes</param>
        /// <returns></returns>
        public IEnumerable<int> GetNodeIndexes(VariableSet nodes)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));

            // Map connected nodes
            foreach (var node in _connections)
            {
                var index = nodes.MapNode(node).Index;
                yield return index;
            }
        }
    }
}
