using SpiceSharp.Behaviors;
using SpiceSharp.Components.SubcircuitBehaviors.Simple;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A standard implementation of a <see cref="ISubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="ISubcircuitDefinition" />
    public class SubcircuitDefinition : ParameterSet, ISubcircuitDefinition
    {
        private string[] _pins;

        /// <summary>
        /// Gets the entities defined in the subcircuit.
        /// </summary>
        /// <value>
        /// The entities inside the subcircuit.
        /// </value>
        public IEntityCollection Entities { get; }

        /// <summary>
        /// Gets the number of pins defined by the subcircuit.
        /// </summary>
        /// <value>
        /// The pin count.
        /// </value>
        public int PinCount => _pins.Length;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitDefinition"/> class.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="pins">The pins.</param>
        public SubcircuitDefinition(IEntityCollection entities, params string[] pins)
        {
            Entities = entities.ThrowIfNull(nameof(entities));
            if (pins != null)
            {
                _pins = new string[pins.Length];
                for (var i = 0; i < pins.Length; i++)
                    _pins[i] = pins[i].ThrowIfNull("node {0}".FormatString(i + 1));
            }
            else
                _pins = new string[0];
        }

        /// <summary>
        /// Creates the behaviors for the entities in the subcircuit.
        /// </summary>
        /// <param name="parentSimulation">The parent simulation.</param>
        /// <param name="behaviors">The <see cref="IBehaviorContainer" /> used for this subcircuit.</param>
        /// <param name="nodes">The nodes on the outside of the subcircuit.</param>
        public virtual void CreateBehaviors(ISimulation parentSimulation, IBehaviorContainer behaviors, string[] nodes)
        {
            if (Entities == null || Entities.Count == 0)
                return;
            if ((nodes == null && _pins.Length > 0) || nodes.Length != _pins.Length)
                throw new CircuitException("Node mismatch: subcircuit requires {0} nodes, but {1} given".FormatString(_pins.Length, nodes?.Length ?? 0));

            // We need to create behaviors for all subcircuit entities
            // So we'll make a subcircuit simulation matching the parent simulation.
            string name = behaviors.Name;
            var simulation = new SubcircuitSimulation(name, parentSimulation);

            // We can now alias the inside- and outside nodes
            for (var i = 0; i < nodes.Length; i++)
            {
                var node = parentSimulation.Variables.MapNode(nodes[i], VariableType.Voltage);
                simulation.Variables.AliasNode(node, _pins[i]);
            }

            // Creat the behaviors for the subcircuit
            simulation.Run(Entities);

            // Create the behaviors necessary for the subcircuit
            if (simulation.UsesBehaviors<ITemperatureBehavior>())
                behaviors.Add(new TemperatureBehavior(name, simulation));
            if (simulation.UsesBehaviors<IBiasingUpdateBehavior>())
                behaviors.Add(new BiasingUpdateBehavior(name, simulation));
            if (simulation.UsesBehaviors<IBiasingBehavior>())
                behaviors.Add(new BiasingBehavior(name, simulation));
            if (simulation.UsesBehaviors<ITimeBehavior>())
                behaviors.Add(new TimeBehavior(name, simulation));
            if (simulation.UsesBehaviors<IAcceptBehavior>())
                behaviors.Add(new AcceptBehavior(name, simulation));
            if (simulation.UsesBehaviors<IFrequencyUpdateBehavior>())
                behaviors.Add(new FrequencyUpdateBehavior(name, simulation));
            if (simulation.UsesBehaviors<IFrequencyBehavior>())
                behaviors.Add(new FrequencyBehavior(name, simulation));
            if (simulation.UsesBehaviors<INoiseBehavior>())
                behaviors.Add(new NoiseBehavior(name, simulation));
        }

        /// <summary>
        /// Creates a clone of the parameter set.
        /// </summary>
        /// <returns>
        /// A clone of the parameter set.
        /// </returns>
        protected override ICloneable Clone()
        {
            var clone = new SubcircuitDefinition((IEntityCollection)Entities.Clone(), _pins);
            return clone;
        }

        /// <summary>
        /// Copy properties and fields from another parameter set.
        /// </summary>
        /// <param name="source">The source parameter set.</param>
        protected override void CopyFrom(ICloneable source)
        {
            base.CopyFrom(source);
            var sd = (SubcircuitDefinition)source;
            _pins = new string[sd._pins.Length];
            for (var i = 0; i < _pins.Length; i++)
                _pins[i] = sd._pins[i];

            Entities.Clear();
            foreach (var entity in sd.Entities)
                Entities.Add(entity);
        }
    }
}
