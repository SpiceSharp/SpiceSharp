using System.Collections.Generic;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A class that represents a (Spice) component/device.
    /// </summary>
    public abstract class Component : Entity, IComponent
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private readonly string[] _connections;

        /// <summary>
        /// Gets the number of nodes.
        /// </summary>
        /// <value>
        /// The number of nodes.
        /// </value>
        public int PinCount => _connections.Length;

        /// <summary>
        /// Gets or sets the model of the component.
        /// </summary>
        /// <value>
        /// The model of the component.
        /// </value>
        public string Model { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Component" /> class.
        /// </summary>
        /// <param name="name">The string of the entity.</param>
        /// <param name="nodeCount">The node count.</param>
        protected Component(string name, int nodeCount)
            : base(name)
        {
            // Initialize
            _connections = nodeCount > 0 ? new string[nodeCount] : null;
        }

        /// <summary>
        /// Connects the component in the circuit.
        /// </summary>
        /// <param name="nodes">The node indices.</param>
        /// <returns>
        /// The instance calling the method for chaining.
        /// </returns>
        public IComponent Connect(params string[] nodes)
        {
            nodes.ThrowIfNot(nameof(nodes), _connections.Length);
            for (var i = 0; i < nodes.Length; i++)
            {
                nodes[i].ThrowIfNull("node{0}".FormatString(i + 1));
                _connections[i] = nodes[i];
            }
            return this;
        }

        /// <summary>
        /// Creates behaviors for the specified simulation that describe this <see cref="Entity" />.
        /// </summary>
        /// <param name="simulation">The simulation requesting the behaviors.</param>
        /// <param name="entities">The entities being processed, used by the entity to find linked entities.</param>
        public override void CreateBehaviors(ISimulation simulation, IEntityCollection entities)
        {
            if (Model != null)
                entities[Model].CreateBehaviors(simulation, entities);
            base.CreateBehaviors(simulation, entities);
        }

        /// <summary>
        /// Gets the node index of a pin.
        /// </summary>
        /// <param name="index">The pin index.</param>
        /// <returns>The node index.</returns>
        public string GetNode(int index)
        {
            if (index < 0 || index >= _connections.Length)
                throw new CircuitException("Invalid node {0}".FormatString(index));
            return _connections[index];
        }

        /// <summary>
        /// Gets the node indexes (in order).
        /// </summary>
        /// <param name="variables">The set of variables.</param>
        /// <returns>An enumerable for all nodes.</returns>
        public IEnumerable<Variable> MapNodes(IVariableSet variables)
        {
            variables.ThrowIfNull(nameof(variables));

            // Map connected nodes
            foreach (var c in _connections)
            {
                var node = variables.MapNode(c, VariableType.Voltage);
                yield return node;
            }
        }

        /// <summary>
        /// Copy from another component.
        /// </summary>
        /// <param name="source">The source component.</param>
        protected override void CopyFrom(ICloneable source)
        {
            base.CopyFrom(source);
            var c = (Component)source;
            for (var i = 0; i < PinCount; i++)
                _connections[i] = c._connections[i];
        }
    }
}
