using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A subcircuit that can contain a collection of entities.
    /// </summary>
    /// <seealso cref="Entity" />
    /// <seealso cref="IComponent" />
    public class Subcircuit : Entity, IComponent
    {
        private string[] _connections;

        /// <summary>
        /// Gets or sets the model of the component.
        /// </summary>
        /// <value>
        /// The model of the component.
        /// </value>
        public string Model { get; set; }

        /// <summary>
        /// Gets the entities.
        /// </summary>
        /// <value>
        /// The entities.
        /// </value>
        public ISubcircuitEntityCollection Entities { get; }

        /// <summary>
        /// Gets the number of nodes.
        /// </summary>
        /// <value>
        /// The number of nodes.
        /// </value>
        public int PinCount => Entities?.PinCount ?? 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Subcircuit"/> class.
        /// </summary>
        /// <param name="name">The name of the subcircuit.</param>
        /// <param name="entities">The entities in the subcircuit.</param>
        public Subcircuit(string name, ISubcircuitEntityCollection entities)
            : base(name)
        {
            Entities = entities;
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
        /// Create one or more behaviors for the simulation.
        /// </summary>
        /// <param name="simulation">The simulation for which behaviors need to be created.</param>
        /// <param name="entities">The other entities.</param>
        /// <param name="behaviors">A container where all behaviors are to be stored.</param>
        protected override void CreateBehaviors(ISimulation simulation, IEntityCollection entities, IBehaviorContainer behaviors)
        {
            if (Entities == null)
                return;
            Entities.CreateBehaviors(simulation, entities, behaviors, _connections);
        }

        /// <summary>
        /// Connects the specified nodes.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        public IComponent Connect(params string[] nodes)
        {
            nodes.ThrowIfNull(nameof(nodes));
            _connections = new string[nodes.Length];
            for (var i = 0; i < nodes.Length; i++)
                _connections[i] = nodes[i].ThrowIfNull($"node {0}".FormatString(i + 1));
            return this;
        }

        /// <summary>
        /// Gets the node index of a pin.
        /// </summary>
        /// <param name="index">The pin index.</param>
        /// <returns>
        /// The node index.
        /// </returns>
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
        /// Copy properties from another entity.
        /// </summary>
        /// <param name="source">The source entity.</param>
        protected override void CopyFrom(ICloneable source)
        {
            base.CopyFrom(source);
            var s = (Subcircuit)source;
            _connections = new string[_connections.Length];
            for (var i = 0; i < _connections.Length; i++)
                _connections[i] = s._connections[i];
        }
    }
}
