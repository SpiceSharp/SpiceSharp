using SpiceSharp.ParameterSets;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Subcircuits;
using SpiceSharp.Components.Subcircuits.Simple;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharp.Validation;
using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A standard implementation of a <see cref="ISubcircuitDefinition" />.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    /// <seealso cref="ISubcircuitDefinition" />
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="Subcircuits.Simple.Parameters"/>
    public class SubcircuitDefinition : ParameterSetCollection, 
        ISubcircuitDefinition,
        IParameterized<Parameters>
    {
        private readonly string[] _pins;

        /// <inheritdoc/>
        public Parameters Parameters { get; } = new Parameters();

        /// <inheritdoc/>
        [ParameterName("entities"), ParameterInfo("The entities in the subcircuit.")]
        public IEntityCollection Entities { get; }

        /// <inheritdoc/>
        [ParameterName("pins"), ParameterInfo("The number of pins.")]
        public int PinCount => _pins.Length;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitDefinition"/> class.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="pins">The pins.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entities"/> is <c>null</c>.</exception>
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
                _pins = Array<string>.Empty();
        }

        /// <inheritdoc/>
        public virtual void CreateBehaviors(Subcircuit subcircuit, ISimulation parentSimulation, IBehaviorContainer behaviors)
        {
            if (Entities.Count == 0)
                return;

            // Make a list of node bridges
            var outNodes = subcircuit.Nodes;
            if ((outNodes == null && _pins.Length > 0) || outNodes.Count != _pins.Length)
                throw new NodeMismatchException(_pins.Length, outNodes?.Count ?? 0);
            var nodes = new Bridge<string>[_pins.Length];
            for (var i = 0; i < _pins.Length; i++)
                nodes[i] = new Bridge<string>(_pins[i], outNodes[i]);

            // Keep all local behaviors in our subcircuit simulation instead of adding them to the parent simulation.
            var simulation = new SubcircuitSimulation(subcircuit.Name, parentSimulation, this, nodes);
            Biasing.Prepare(simulation);
            Frequency.Prepare(simulation);

            // Create the behaviors for the subcircuit
            simulation.Run(Entities);

            // Create the behaviors necessary for the subcircuit
            behaviors
                .AddIfNo<ITemperatureBehavior>(simulation, () => new Temperature(subcircuit.Name, simulation))
                .AddIfNo<IBiasingUpdateBehavior>(parentSimulation, () => new BiasingUpdate(subcircuit.Name, simulation))
                .AddIfNo<ITimeBehavior>(parentSimulation, () => new Time(subcircuit.Name, simulation))
                .AddIfNo<IBiasingBehavior>(parentSimulation, () => new Biasing(subcircuit.Name, simulation))
                .AddIfNo<IAcceptBehavior>(parentSimulation, () => new Accept(subcircuit.Name, simulation))
                .AddIfNo<IFrequencyUpdateBehavior>(parentSimulation, () => new FrequencyUpdate(subcircuit.Name, simulation))
                .AddIfNo<IFrequencyBehavior>(parentSimulation, () => new Frequency(subcircuit.Name, simulation))
                .AddIfNo<INoiseBehavior>(parentSimulation, () => new Subcircuits.Simple.Noise(subcircuit.Name, simulation));
        }

        /// <inheritdoc/>
        void ISubcircuitDefinition.Apply(Subcircuit subcircuit, IRules rules)
        {
            // Make a list of node bridges
            var outNodes = subcircuit.Nodes;
            if ((outNodes == null && _pins.Length > 0) || outNodes.Count != _pins.Length)
                throw new NodeMismatchException(_pins.Length, outNodes?.Count ?? 0);
            var nodes = new Bridge<string>[_pins.Length];
            for (var i = 0; i < _pins.Length; i++)
                nodes[i] = new Bridge<string>(_pins[i], outNodes[i]);

            var crp = rules.GetParameterSet<ComponentRuleParameters>();
            var newRules = new SubcircuitRules(rules, new ComponentRuleParameters(
                new VariableFactory(subcircuit.Name, crp.Factory, nodes, crp.Comparer),
                crp.Comparer));
            foreach (var c in Entities)
            {
                if (c is IRuleSubject subject)
                    subject.Apply(newRules);
            }
        }
    }
}
