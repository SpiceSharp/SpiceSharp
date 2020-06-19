using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharp.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A subcircuit that can contain a collection of entities.
    /// </summary>
    /// <seealso cref="Entity" />
    /// <seealso cref="IComponent" />
    public class Subcircuit : Entity, IComponent, IRuleSubject
    {
        private string[] _connections;

        /// <summary>
        /// Gets the subcircuit definition.
        /// </summary>
        /// <value>
        /// The definition.
        /// </value>
        public ISubcircuitDefinition Definition { get; }

        /// <inheritdoc/>
        public string Model { get; set; }

        /// <inheritdoc/>
        public IReadOnlyList<string> Nodes => new ReadOnlyCollection<string>(_connections);

        /// <summary>
        /// Initializes a new instance of the <see cref="Subcircuit"/> class.
        /// </summary>
        /// <param name="name">The name of the subcircuit.</param>
        /// <param name="definition">The subcircuit definition.</param>
        /// <param name="nodes">The nodes that the subcircuit is connected to.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> or <paramref name="definition"/> is <c>null</c>.</exception>
        public Subcircuit(string name, ISubcircuitDefinition definition, params string[] nodes)
            : base(name)
        {
            Definition = definition.ThrowIfNull(nameof(definition));
            Connect(nodes);
        }

        /// <inheritdoc/>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            Definition.CreateBehaviors(this, simulation, behaviors);
            simulation.EntityBehaviors.Add(behaviors);
        }

        /// <inheritdoc/>
        public IComponent Connect(params string[] nodes)
        {
            nodes.ThrowIfNull(nameof(nodes));
            _connections = new string[nodes.Length];
            for (var i = 0; i < nodes.Length; i++)
                _connections[i] = nodes[i].ThrowIfNull($"node {0}".FormatString(i + 1));
            return this;
        }

        /// <inheritdoc/>
        protected override void CopyFrom(ICloneable source)
        {
            base.CopyFrom(source);
            var s = (Subcircuit)source;
            _connections = new string[_connections.Length];
            for (var i = 0; i < _connections.Length; i++)
                _connections[i] = s._connections[i];
        }

        /// <inheritdoc/>
        public void Apply(IRules rules)
        {
            Definition.Apply(this, rules);
        }
    }
}
