using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.SubcircuitBehaviors;
using SpiceSharp.Components.SubcircuitBehaviors.Simple;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharp.Validation;
using SpiceSharp.Validation.Rules;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A standard implementation of a <see cref="ISubcircuitDefinition" />.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    /// <seealso cref="ISubcircuitDefinition" />
    /// <seealso cref="ISubcircuitRuleSubject" />
    public class SubcircuitDefinition : Parameterized, ISubcircuitDefinition, ISubcircuitRuleSubject,
        IParameterized<BaseParameters>
    {
        private string[] _pins;

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
            var nodes = subcircuit.MapNodes(parentSimulation.Variables).ToArray();
            if ((nodes == null && _pins.Length > 0) || nodes.Length != _pins.Length)
                throw new NodeMismatchException(_pins.Length, nodes?.Length ?? 0);

            // Keep all local behaviors in our subcircuit simulation
            string name = behaviors.Name;
            Variable[] sharedNodes = new Variable[_pins.Length];
            for (var i = 0; i < sharedNodes.Length; i++)
                sharedNodes[i] = parentSimulation.Variables.MapNode(subcircuit.GetNode(i), VariableType.Voltage);
            var simulation = new SubcircuitSimulation(name, parentSimulation, this, sharedNodes);

            // We can now prepare the subcircuit simulation
            for (var i = 0; i < sharedNodes.Length; i++)
                simulation.Variables.AliasNode(sharedNodes[i], _pins[i]);
            BiasingBehavior.Prepare(simulation);
            FrequencyBehavior.Prepare(simulation);

            // Create the behaviors for the subcircuit
            simulation.Run(Entities);

            // Create the behaviors necessary for the subcircuit
            behaviors
                .AddIfNo<ITemperatureBehavior>(simulation, () => new TemperatureBehavior(name, simulation))
                .AddIfNo<IBiasingUpdateBehavior>(parentSimulation, () => new BiasingUpdateBehavior(name, simulation))
                .AddIfNo<ITimeBehavior>(parentSimulation, () => new TimeBehavior(name, simulation))
                .AddIfNo<IBiasingBehavior>(parentSimulation, () => new BiasingBehavior(name, simulation))
                .AddIfNo<IAcceptBehavior>(parentSimulation, () => new AcceptBehavior(name, simulation))
                .AddIfNo<IFrequencyUpdateBehavior>(parentSimulation, () => new FrequencyUpdateBehavior(name, simulation))
                .AddIfNo<IFrequencyBehavior>(parentSimulation, () => new FrequencyBehavior(name, simulation))
                .AddIfNo<INoiseBehavior>(parentSimulation, () => new NoiseBehavior(name, simulation));
        }

        /// <summary>
        /// Validates the subcircuit definition.
        /// </summary>
        /// <param name="subckt">The subcircuit entity that needs to validated.</param>
        /// <param name="nodes">The nodes that the subcircuit definition is connected to.</param>
        /// <param name="container">The rule container.</param>
        public void ApplyTo(Subcircuit subckt, string[] nodes, IRuleContainer container)
        {
            if (Entities == null || Entities.Count == 0)
                return;
            if ((nodes == null && _pins.Length > 0) || nodes.Length != _pins.Length)
                throw new NodeMismatchException(_pins.Length, nodes?.Length ?? 0);

            // Change the variable set to work with subcircuits
            var vconfig = container.Configuration.GetValue<VariableParameters>();
            var original = vconfig.Variables;
            vconfig.Variables = new SubcircuitVariableSet(subckt.Name, original);

            // We can now alias the inside- and outside nodes
            for (var i = 0; i < nodes.Length; i++)
            {
                var node = original.MapNode(nodes[i], VariableType.Voltage);
                vconfig.Variables.AliasNode(node, _pins[i]);
            }

            // Run the rules on the entities using these variables
            foreach (var subject in Subjects)
                subject.ApplyTo(container);

            // Restore the original variables
            vconfig.Variables = original;
        }

        /// <summary>
        /// Gets the subjects that can apply to rules.
        /// </summary>
        /// <value>
        /// The subjects.
        /// </value>
        protected IEnumerable<IRuleSubject> Subjects
        {
            get
            {
                if (Entities == null)
                    yield break;
                foreach (var entity in Entities)
                {
                    if (entity is IRuleSubject subject)
                        yield return subject;
                }
            }
        }
    }
}
