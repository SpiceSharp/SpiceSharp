using SpiceSharp.Behaviors;
using SpiceSharp.Components.SubcircuitBehaviors;
using SpiceSharp.Entities;
using SpiceSharp.General;
using SpiceSharp.Simulations;
using SpiceSharp.Validation;
using System;
using System.Collections.Generic;

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
        /// Gets or sets the model of the component.
        /// </summary>
        /// <value>
        /// The model of the component.
        /// </value>
        public string Model { get; set; }

        /// <summary>
        /// Gets the number of nodes.
        /// </summary>
        /// <value>
        /// The number of nodes.
        /// </value>
        public int PinCount
        {
            get
            {
                if (Parameters.TryGetValue<ISubcircuitDefinition>(out var result))
                    return result.PinCount;
                return 0;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Subcircuit"/> class.
        /// </summary>
        /// <param name="name">The name of the subcircuit.</param>
        /// <param name="entities">The entities in the subcircuit.</param>
        /// <param name="nodes">The nodes that the subcircuit is connected to.</param>
        public Subcircuit(string name, ISubcircuitDefinition entities, params string[] nodes)
            : base(name, new ParameterSetDictionary(new InterfaceTypeDictionary<IParameterSet>()))
        {
            Parameters.Add(entities);
            Connect(nodes);
        }

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name,
                LinkParameters ? Parameters : (IParameterSetDictionary)Parameters.Clone());
            behaviors.Parameters.CalculateDefaults();
            var definition = Parameters.GetValue<ISubcircuitDefinition>();
            definition.CreateBehaviors(simulation, behaviors, _connections);
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
        /// Gets the node index of a pin.
        /// </summary>
        /// <param name="index">The pin index.</param>
        /// <returns>
        /// The node index.
        /// </returns>
        public string GetNode(int index)
        {
            if (index < 0 || index >= _connections.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
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

        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <param name="container">The container with all the rules that should be validated.</param>
        public void ApplyTo(IRuleContainer container)
        {
            // Also allow checks on the subcircuit
            foreach (var rule in container.GetAllValues<IComponentRule>())
                rule.ApplyComponent(this);

            // We don't know about conductive paths (this should be taken care of by the subcircuit definition)
            foreach (var rule in container.GetAllValues<IConductivePathRule>())
                rule.ApplyConductivePath(this);

            // Validate the subcircuit definition if possible
            if (Parameters.TryGetValue<ISubcircuitRuleSubject>(out var result))
                result.ApplyTo(this, _connections, container);
        }
    }
}
