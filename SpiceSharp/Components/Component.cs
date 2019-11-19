using System.Collections.Generic;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A class that represents a (Spice) component/device.
    /// </summary>
    public abstract class Component : Entity
    {
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
            // Initialize
            _connections = nodeCount > 0 ? new string[nodeCount] : null;
        }

        /// <summary>
        /// Gets the model of the circuit component (if any).
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Connects the component in the circuit.
        /// </summary>
        /// <remarks>
        /// This command is chainable.
        /// </remarks>
        /// <param name="nodes">The node indices.</param>
        public Component Connect(params string[] nodes)
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
        /// <remarks>
        /// The order typically indicates hierarchy. The entity will create the behaviors in reverse order, allowing
        /// the most specific child class to be used that is necessary. For example, the <see cref="OP" /> simulation needs
        /// <see cref="ITemperatureBehavior" /> and an <see cref="IBiasingBehavior" />. The entity will first look for behaviors
        /// of type <see cref="IBiasingBehavior" />, and then for the behaviors of type <see cref="ITemperatureBehavior" />. However,
        /// if the behavior that was created for <see cref="IBiasingBehavior" /> also implements <see cref="ITemperatureBehavior" />,
        /// then then entity will not create a new instance of the behavior.
        /// </remarks>
        public override void CreateBehaviors(ISimulation simulation, IEntityCollection entities)
        {
            if (Model != null)
                entities[Model].CreateBehaviors(simulation, entities);
            base.CreateBehaviors(simulation, entities);
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
        /// <param name="variables">The variable set.</param>
        /// <returns>The node indices.</returns>
        protected virtual Variable[] ApplyConnections(IVariableSet variables)
        {
            variables.ThrowIfNull(nameof(variables));
            if (_connections == null)
                return new Variable[0];

            // Map connected nodes
            var nodes = new Variable[_connections.Length];
            for (var i = 0; i < _connections.Length; i++)
                nodes[i] = variables.MapNode(_connections[i], VariableType.Voltage);
            return nodes;
        }

        /// <summary>
        /// Gets the node indexes (in order).
        /// </summary>
        /// <param name="variables">The set of variables.</param>
        /// <returns>An enumerable for all nodes.</returns>
        public virtual IEnumerable<Variable> GetNodes(IVariableSet variables)
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
