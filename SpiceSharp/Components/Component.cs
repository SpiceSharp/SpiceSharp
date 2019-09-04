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
        /// <param name="nodes">The node indices.</param>
        public void Connect(params string[] nodes)
        {
            nodes.ThrowIfNot(nameof(nodes), _connections.Length);
            for (var i = 0; i < nodes.Length; i++)
            {
                nodes[i].ThrowIfNull("node{0}".FormatString(i + 1));
                _connections[i] = nodes[i];
            }
        }

        /// <summary>
        /// Creates behaviors for the specified simulation that describe this <see cref="Entity"/>.
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
        /// Binds the behaviors to the simulation.
        /// </summary>
        /// <param name="behaviors">The behaviors that needs to be bound to the simulation.</param>
        /// <param name="simulation">The simulation to be bound to.</param>
        /// <param name="entities">The entities that the entity may be connected to.</param>
        protected override void BindBehaviors(IEnumerable<IBehavior> behaviors, ISimulation simulation, IEntityCollection entities)
        {
            // Create the context, specifically for components
            var context = new ComponentBindingContext(simulation, Name, ApplyConnections(simulation.Variables), Model);

            // Bind the behaviors
            foreach (var behavior in behaviors)
            {
                behavior.Bind(context);
                context.Behaviors.Add(behavior.GetType(), behavior);
            }
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
        protected virtual int[] ApplyConnections(IVariableSet nodes)
        {
            nodes.ThrowIfNull(nameof(nodes));
            if (_connections == null)
                return new int[0];

            // Map connected nodes
            var indexes = new int[_connections.Length];
            for (var i = 0; i < _connections.Length; i++)
                indexes[i] = nodes.MapNode(_connections[i], VariableType.Voltage).Index;
            return indexes;
        }

        /// <summary>
        /// Gets the node indexes (in order).
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <returns>An enumerable for all nodes.</returns>
        public virtual IEnumerable<int> GetNodeIndexes(IVariableSet nodes)
        {
            nodes.ThrowIfNull(nameof(nodes));

            // Map connected nodes
            foreach (var node in _connections)
            {
                var index = nodes.MapNode(node, VariableType.Voltage).Index;
                yield return index;
            }
        }

        /// <summary>
        /// Clone the component for instantiating.
        /// </summary>
        /// <param name="data">The instance data.</param>
        /// <returns></returns>
        public override Entity Clone(InstanceData data)
        {
            var clone = (Component)base.Clone(data);

            // Manage connections
            if (data is ComponentInstanceData cid)
            {
                // Map nodes
                string[] nodes = new string[PinCount];
                for (var i = 0; i < PinCount; i++)
                    nodes[i] = cid.GenerateNodeName(_connections[i]);
                clone.Connect(nodes);

                // Map the model
                if (Model != null)
                    clone.Model = cid.GenerateModelName(Model);
            }

            return clone;
        }

        /// <summary>
        /// Copy from another component.
        /// </summary>
        /// <param name="source">The source component.</param>
        public override void CopyFrom(Entity source)
        {
            base.CopyFrom(source);
            var c = (Component)source;
            for (var i = 0; i < PinCount; i++)
                _connections[i] = c._connections[i];
        }
    }
}
