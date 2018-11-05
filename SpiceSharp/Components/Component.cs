using System;
using System.Collections.Generic;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A class that represents a (Spice) component/device.
    /// </summary>
    public abstract class Component : Entity
    {
        /// <summary>
        /// The default priority for components.
        /// </summary>
        public const int ComponentPriority = 0;

        /// <summary>
        /// Private variables
        /// </summary>
        private readonly string[] _connections;

        /// <summary>
        /// Gets the number of nodes.
        /// </summary>
        public virtual int PinCount => _connections.Length;

        /// <summary>
        /// Initializes a new instance of the <see cref="Component" /> class.
        /// </summary>
        /// <param name="name">The string of the entity.</param>
        /// <param name="nodeCount">The node count.</param>
        protected Component(string name, int nodeCount)
            : base(name)
        {
            Priority = ComponentPriority;

            // Initialize
            _connections = nodeCount > 0 ? new string[nodeCount] : null;
        }

        /// <summary>
        /// Gets the model of the circuit component (if any).
        /// </summary>
        public Entity Model { get; protected set; } = null;

        /// <summary>
        /// Connects the component in the circuit.
        /// </summary>
        /// <param name="nodes">The node indices.</param>
        /// <exception cref="ArgumentNullException">
        /// nodes
        /// or
        /// node " + (i + 1)
        /// </exception>
        /// <exception cref="CircuitException">{0}: Node count mismatch. {1} given, {2} expected.".FormatString(Name, nodes.Length, _connections.Length)</exception>
        public void Connect(params string[] nodes)
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
        /// Creates a behavior of the specified type.
        /// </summary>
        /// <typeparam name="T">The base behavior type.</typeparam>
        /// <param name="simulation">The simulation.</param>
        /// <returns></returns>
        public override IBehavior CreateBehavior<T>(Simulation simulation)
        {
            var behavior = base.CreateBehavior<T>(simulation);

            // Apply our connections if necessary
            if (behavior is IConnectedBehavior conn)
            {
                var pins = ApplyConnections(simulation.Variables);
                conn.Connect(pins);
            }

            return behavior;
        }

        /// <summary>
        /// Build the data provider for setting up a behavior for the entity. The entity can control which parameters
        /// and behaviors are visible to behaviors using this method.
        /// </summary>
        /// <param name="parameters">The parameters in the simulation.</param>
        /// <param name="behaviors">The behaviors in the simulation.</param>
        /// <returns>
        /// A data provider for the behaviors.
        /// </returns>
        protected override SetupDataProvider BuildSetupDataProvider(ParameterPool parameters, BehaviorPool behaviors)
        {
            var provider = base.BuildSetupDataProvider(parameters, behaviors);

            // Add our model parameters and behaviors
            if (Model != null)
            {
                provider.Add("model", parameters[Model.Name]);
                provider.Add("model", behaviors[Model.Name]);
            }

            return provider;
        }

        /// <summary>
        /// Gets the node index of a pin.
        /// </summary>
        /// <remarks>
        /// This method will only return valid results after the component is set up using <see cref="ApplyConnections"/>.
        /// </remarks>
        /// <param name="index">The pin index.</param>
        /// <returns>The node index.</returns>
        public virtual string GetNode(int index)
        {
            if (index < 0 || index >= _connections.Length)
                throw new CircuitException("Invalid node {0}".FormatString(index));
            return _connections[index];
        }

        /// <summary>
        /// Update the indices for the component.
        /// </summary>
        /// <param name="nodes">The variable set.</param>
        /// <returns>The node indices.</returns>
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
        /// Gets the node indexes (in order).
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <returns>An enumerable for all nodes.</returns>
        /// <exception cref="ArgumentNullException">nodes</exception>
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
