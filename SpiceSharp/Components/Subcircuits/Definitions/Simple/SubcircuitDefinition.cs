using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.SubcircuitBehaviors;
using SpiceSharp.Components.SubcircuitBehaviors.Simple;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharp.Validation;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A standard implementation of a <see cref="ISubcircuitDefinition" />.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    /// <seealso cref="ISubcircuitDefinition" />
    public class SubcircuitDefinition : Parameterized, ISubcircuitDefinition,
        IParameterized<BaseParameters>
    {
        private readonly string[] _pins;

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public BaseParameters Parameters { get; } = new BaseParameters();

        /// <summary>
        /// Gets the entities defined in the subcircuit.
        /// </summary>
        /// <value>
        /// The entities inside the subcircuit.
        /// </value>
        [ParameterName("entities"), ParameterInfo("The entities in the subcircuit.")]
        public IEntityCollection Entities { get; }

        /// <summary>
        /// Gets the number of pins defined by the subcircuit.
        /// </summary>
        /// <value>
        /// The pin count.
        /// </value>
        [ParameterName("pins"), ParameterInfo("The number of pins.")]
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
        /// <param name="subcircuit">The subcircuit that wants to create the behaviors through the definition.</param>
        /// <param name="parentSimulation">The parent simulation.</param>
        /// <param name="behaviors">The <see cref="IBehaviorContainer" /> used for the subcircuit.</param>
        /// <exception cref="NodeMismatchException">Thrown if the number of nodes do not match.</exception>
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
            BiasingBehavior.Prepare(simulation);
            FrequencyBehavior.Prepare(simulation);

            // Create the behaviors for the subcircuit
            simulation.Run(Entities);

            // Create the behaviors necessary for the subcircuit
            behaviors
                .AddIfNo<ITemperatureBehavior>(simulation, () => new TemperatureBehavior(subcircuit.Name, simulation))
                .AddIfNo<IBiasingUpdateBehavior>(parentSimulation, () => new BiasingUpdateBehavior(subcircuit.Name, simulation))
                .AddIfNo<ITimeBehavior>(parentSimulation, () => new TimeBehavior(subcircuit.Name, simulation))
                .AddIfNo<IBiasingBehavior>(parentSimulation, () => new BiasingBehavior(subcircuit.Name, simulation))
                .AddIfNo<IAcceptBehavior>(parentSimulation, () => new AcceptBehavior(subcircuit.Name, simulation))
                .AddIfNo<IFrequencyUpdateBehavior>(parentSimulation, () => new FrequencyUpdateBehavior(subcircuit.Name, simulation))
                .AddIfNo<IFrequencyBehavior>(parentSimulation, () => new FrequencyBehavior(subcircuit.Name, simulation))
                .AddIfNo<INoiseBehavior>(parentSimulation, () => new NoiseBehavior(subcircuit.Name, simulation));
        }

        /// <summary>
        /// Applies the subject to any rules in the validation provider.
        /// </summary>
        /// <param name="subcircuit">The subcircuit instance.</param>
        /// <param name="rules">The rule provider.</param>
        public void Apply(Subcircuit subcircuit, IRules rules)
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
