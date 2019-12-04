using SpiceSharp.Behaviors;
using SpiceSharp.Components.SubcircuitBehaviors;
using SpiceSharp.Components.SubcircuitBehaviors.Simple;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharp.Validation;
using SpiceSharp.Validation.Rules;
using System.Collections.Generic;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A standard implementation of a <see cref="ISubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="ISubcircuitDefinition" />
    public class SubcircuitDefinition : ParameterSet, ISubcircuitDefinition, ISubcircuitValidator
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
                throw new NodeMismatchException(_pins.Length, nodes?.Length ?? 0);

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
            behaviors
                .AddIfNo<ITemperatureBehavior>(simulation, () => new TemperatureBehavior(name, simulation))
                .AddIfNo<IBiasingUpdateBehavior>(parentSimulation, () => new BiasingUpdateBehavior(name, simulation))
                .AddIfNo<IBiasingBehavior>(parentSimulation, () => new BiasingBehavior(name, simulation))
                .AddIfNo<ITimeBehavior>(parentSimulation, () => new TimeBehavior(name, simulation))
                .AddIfNo<IAcceptBehavior>(parentSimulation, () => new AcceptBehavior(name, simulation))
                .AddIfNo<IFrequencyUpdateBehavior>(parentSimulation, () => new FrequencyUpdateBehavior(name, simulation))
                .AddIfNo<IFrequencyBehavior>(parentSimulation, () => new FrequencyBehavior(name, simulation))
                .AddIfNo<INoiseBehavior>(parentSimulation, () => new NoiseBehavior(name, simulation));
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

        /// <summary>
        /// Validates the subcircuit definition.
        /// </summary>
        /// <param name="subckt">The subcircuit entity that needs to validated.</param>
        /// <param name="nodes">The nodes that the subcircuit definition is connected to.</param>
        /// <param name="container">The rule container.</param>
        public void Validate(Subcircuit subckt, string[] nodes, IRuleContainer container)
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
            foreach (var validator in Validators)
                validator.Validate(container);

            // Restore the original variables
            vconfig.Variables = original;
        }


        /// <summary>
        /// Gets the validators.
        /// </summary>
        /// <value>
        /// The validators.
        /// </value>
        protected IEnumerable<IValidator> Validators
        {
            get
            {
                if (Entities == null)
                    yield break;
                foreach (var entity in Entities)
                {
                    if (entity is IValidator validator)
                        yield return validator;
                }
            }
        }
    }
}
