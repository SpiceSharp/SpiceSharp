using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharp.Components.SubcircuitBehaviors;
using System.Collections.Generic;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A component that can contain other entities and group them.
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.Component" />
    public class Subcircuit : Component
    {
        static Subcircuit()
        {
            RegisterBehaviorFactory(typeof(Subcircuit), new BehaviorFactoryDictionary()
            {
                { typeof(InitialConditionBehavior), e => new InitialConditionBehavior(e.Name) },
                { typeof(TemperatureBehavior), e => new TemperatureBehavior(e.Name) },
                { typeof(BiasingBehavior), e => new BiasingBehavior(e.Name) },
                { typeof(TimeBehavior), e => new TimeBehavior(e.Name) },
                { typeof(AcceptBehavior), e => new AcceptBehavior(e.Name) },
                { typeof(FrequencyBehavior), e => new FrequencyBehavior(e.Name) },
                { typeof(NoiseBehavior), e => new NoiseBehavior(e.Name) }
            });
        }

        /// <summary>
        /// Gets the entities in the subcircuit.
        /// </summary>
        /// <value>
        /// The entities.
        /// </value>
        public IEntityCollection Entities { get; }

        /// <summary>
        /// Gets the set of global nodes.
        /// </summary>
        /// <remarks>
        /// Global nodes are nodes that are shared among all subcircuits without explicit connection. Typical examples
        /// are supply voltages ("VDD", "VEE", etc.), but any number can be specified. The ground node "0" is always
        /// treated as a global node, as well as any aliases to ground.
        /// </remarks>
        /// <value>
        /// The global nodes.
        /// </value>
        public HashSet<string> GlobalNodes { get; set; }

        /// <summary>
        /// Gets the local pin names. These will globally look like other pin names.
        /// </summary>
        /// <value>
        /// The local pin names.
        /// </value>
        public string[] LocalPinNames { get; }

        /// <summary>
        /// The mock simulation used to create behaviors inside the subcircuit
        /// </summary>
        private SubcircuitSimulation _subcktSimulation;

        /// <summary>
        /// Initializes a new instance of the <see cref="Subcircuit"/> class.
        /// </summary>
        /// <param name="name">The name of the subcircuit.</param>
        /// <param name="pins">The local node names in the subcircuit that will be connected outside.</param>
        /// <param name="entities">The entities in the subcircuit.</param>
        public Subcircuit(string name, string[] pins, IEntityCollection entities)
            : base(name, pins.Length)
        {
            Entities = entities.ThrowIfNull(nameof(entities));
            LocalPinNames = new string[pins.Length];
            for (var i = 0; i < pins.Length; i++)
                LocalPinNames[i] = pins[i].ThrowIfNull("pin");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Subcircuit"/> class.
        /// </summary>
        /// <param name="name">The name of the subcircuit.</param>
        /// <param name="pins">The local node names in the subcircuit that will be connected outside.</param>
        /// <param name="entities">The entities in the subcircuit.</param>
        /// <param name="connections">The node connections on the outside.</param>
        public Subcircuit(string name, string[] pins, IEntityCollection entities, string[] connections)
            : this(name, pins, entities)
        {
            Connect(connections);
        }

        /// <summary>
        /// Creates behaviors for the specified simulation that describe this <see cref="Entity"/>.
        /// </summary>
        /// <param name="simulation">The simulation requesting the behaviors.</param>
        /// <param name="entities">The entities being processed, used by the entity to find linked entities.</param>
        /// <remarks>
        /// The order typically indicates hierarchy. The entity will create the behaviors in reverse order, allowing
        /// the most specific child class to be used that is necessary. For example, the <see cref="OP" /> simulation needs
        /// <see cref="ITemperatureBehavior" /> and an <see cref="IBiasingBehavior" />. The entity will first look for behaviors
        /// of type <see cref="IBiasingBehavior" />, and then for the behaviors of type <see cref="ITemperatureBehavior" />. However,
        /// if the behavior that was created for <see cref="IBiasingBehavior" /> also implements <see cref="ITemperatureBehavior" />,
        /// then then entity will not create a new instance of the behavior.
        /// </remarks>
        public override void CreateBehaviors(ISimulation simulation, IEntityCollection entities)
        {
            // If our behaviors are already created, skip this
            if (simulation.EntityBehaviors.ContainsKey(Name))
                return;

            // Create the behaviors inside our behaviors
            var nodemap = new Dictionary<string, string>(simulation.Variables.Comparer);
            for (var i = 0; i < PinCount; i++)
                nodemap.Add(LocalPinNames[i], GetNode(i));
            if (GlobalNodes != null)
            {
                foreach (var g in GlobalNodes)
                    nodemap.Add(g, g);
            }

            // We need to use a proxy simulation to create the behaviors
            _subcktSimulation = new SubcircuitSimulation(Name, simulation, nodemap);
            var ec = new SubcircuitEntityCollection(entities, Entities, simulation);
            _subcktSimulation.Run(ec);

            // Now let's create our own behaviors
            base.CreateBehaviors(simulation, entities);
        }

        /// <summary>
        /// Build a binding context for a behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <returns></returns>
        protected override ComponentBindingContext BuildBindingContext(ISimulation simulation)
        {
            // Make sure our behaviors can find the behaviors of the subcircuit
            return new SubcircuitBindingContext(simulation, _subcktSimulation.EntityBehaviors);
        }
    }
}
