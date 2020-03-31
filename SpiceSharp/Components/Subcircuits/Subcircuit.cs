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

        /// <summary>
        /// Gets or sets the model of the component.
        /// </summary>
        /// <value>
        /// The model of the component.
        /// </value>
        public string Model { get; set; }

        /// <summary>
        /// Gets the nodes.
        /// </summary>
        /// <value>
        /// The nodes.
        /// </value>
        public IReadOnlyList<string> Nodes => new ReadOnlyCollection<string>(_connections);

        /// <summary>
        /// Initializes a new instance of the <see cref="Subcircuit"/> class.
        /// </summary>
        /// <param name="name">The name of the subcircuit.</param>
        /// <param name="definition">The subcircuit definition.</param>
        /// <param name="nodes">The nodes that the subcircuit is connected to.</param>
        public Subcircuit(string name, ISubcircuitDefinition definition, params string[] nodes)
            : base(name)
        {
            Definition = definition.ThrowIfNull(nameof(definition));
            Connect(nodes);
        }

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            CalculateDefaults();
            Definition.CreateBehaviors(this, simulation, behaviors);
            simulation.EntityBehaviors.Add(behaviors);
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

        /// <summary>
        /// Applies the subject to any rules in the validation provider.
        /// </summary>
        /// <param name="rules">The provider.</param>
        public void Apply(IRules rules)
        {
            Definition.Apply(this, rules);
        }
    }
}
