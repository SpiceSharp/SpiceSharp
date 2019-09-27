using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharp.Components.SubcircuitBehaviors;
using System.Collections.Generic;
using SpiceSharp.Entities.Local;

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
                { typeof(IInitialConditionBehavior), e => new InitialConditionBehavior(e.Name) },
                { typeof(ITemperatureBehavior), e => new TemperatureBehavior(e.Name) },
                { typeof(IBiasingBehavior), e => new BiasingBehavior(e.Name) },
                { typeof(ITimeBehavior), e => new TimeBehavior(e.Name) },
                { typeof(IAcceptBehavior), e => new AcceptBehavior(e.Name) },
                { typeof(IFrequencyBehavior), e => new FrequencyBehavior(e.Name) },
                { typeof(INoiseBehavior), e => new NoiseBehavior(e.Name) }
            });
        }

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
        public Subcircuit(string name, IEntityCollection entities, params string[] pins)
            : base(name, pins.Length)
        {
            var bp = new BaseParameters(entities, pins);
            Parameters.Add(bp);
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
            var ebp = Parameters.GetValue<BaseParameters>();
            var nodemap = new Dictionary<string, string>(simulation.Variables.Comparer);
            for (var i = 0; i < PinCount; i++)
                nodemap.Add(ebp.Pins[i], GetNode(i));
            if (ebp.GlobalNodes != null)
            {
                foreach (var g in ebp.GlobalNodes)
                    nodemap.Add(g, g);
            }

            // We need to use a proxy simulation to create the behaviors
            _subcktSimulation = new SubcircuitSimulation(Name, simulation, nodemap);
            var ec = new LocalEntityCollection(entities, ebp.Entities, simulation);
            _subcktSimulation.Run(ec);

            // Now let's create our own behaviors
            base.CreateBehaviors(simulation, entities);
        }

        /// <summary>
        /// Binds the behaviors to the simulation.
        /// </summary>
        /// <param name="eb">The entity behaviors and parameters.</param>
        /// <param name="simulation">The simulation to be bound to.</param>
        /// <param name="entities">The entities that the entity may be connected to.</param>
        protected override void BindBehaviors(BehaviorContainer eb, ISimulation simulation, IEntityCollection entities)
        {
            // We want to make sure that the behaviors are accessible through the behavior container
            eb.Parameters.Add(new BehaviorBaseParameters(_subcktSimulation.EntityBehaviors));

            var context = new SubcircuitBindingContext(simulation, eb, _subcktSimulation.EntityBehaviors);
            foreach (var behavior in eb.Ordered)
                behavior.Bind(context);
        }
    }
}
