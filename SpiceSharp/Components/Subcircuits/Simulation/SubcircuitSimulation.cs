using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// A simulation look-alike to use proxies.
    /// </summary>
    /// <seealso cref="SpiceSharp.Simulations.Simulation" />
    public class SubcircuitSimulation : ISimulation
    {
        private ISimulation _parent;

        /// <summary>
        /// Gets the name of the <see cref="ISimulation" />.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the current status of the <see cref="ISimulation" />.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public SimulationStatus Status => _parent.Status;

        /// <summary>
        /// Gets a set of configurations for the <see cref="ISimulation" />.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public ParameterSetDictionary Configurations => _parent.Configurations;

        /// <summary>
        /// Gets a set of <see cref="SimulationState" /> instances used by the <see cref="ISimulation" />.
        /// </summary>
        /// <value>
        /// The states.
        /// </value>
        public TypeDictionary<SimulationState> States => _parent.States;

        /// <summary>
        /// Gets a set of <see cref="SpiceSharp.Simulations.Statistics" /> instances tracked by the <see cref="ISimulation" />.
        /// </summary>
        /// <value>
        /// The statistics.
        /// </value>
        public TypeDictionary<Statistics> Statistics => _parent.Statistics;

        /// <summary>
        /// Gets the variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public IVariableSet Variables { get; }

        /// <summary>
        /// Gets the entity behaviors.
        /// </summary>
        /// <value>
        /// The entity behaviors.
        /// </value>
        public BehaviorPool EntityBehaviors { get; }

        /// <summary>
        /// Gets the entity parameters.
        /// </summary>
        /// <value>
        /// The entity parameters.
        /// </value>
        public ParameterPool EntityParameters { get; }

        /// <summary>
        /// Gets the <see cref="IBehavior" /> types used by the <see cref="ISimulation" />.
        /// </summary>
        /// <value>
        /// The behavior types.
        /// </value>
        public IEnumerable<Type> BehaviorTypes => _parent.BehaviorTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitSimulation"/> class.
        /// </summary>
        /// <param name="name">The identifier of the subcircuit simulation.</param>
        /// <param name="parent">The parent simulation.</param>
        /// <param name="nodemap">The structure that maps the internal to external nodes.</param>
        public SubcircuitSimulation(string name, ISimulation parent, Dictionary<string, string> nodemap)
        {
            _parent = parent.ThrowIfNull(nameof(parent));
            Name = name.ThrowIfNull(nameof(name));

            Variables = new SubcircuitVariableSet(name, _parent.Variables);
            EntityBehaviors = new SubcircuitBehaviorPool(_parent.EntityBehaviors.Comparer, parent.EntityBehaviors);

            // Alias all pins to local nodes
            foreach (var pair in nodemap)
            {
                // Map the external node (may not have been used yet by other entities)
                _parent.Variables.MapNode(pair.Value, VariableType.Voltage);

                // Alias the internal node to it
                Variables.AliasNode(pair.Value, pair.Key);
            }
        }

        /// <summary>
        /// Runs the simulation on the specified circuit.
        /// </summary>
        /// <param name="entities">The entities to simulate.</param>
        /// <exception cref="CircuitException">Cannot run subcircuit simulation</exception>
        public void Run(IEntityCollection entities)
        {
            // We basically need to create the subcircuit behaviors
            foreach (var entity in entities)
                entity.CreateBehaviors(this, entities);
        }
    }
}
