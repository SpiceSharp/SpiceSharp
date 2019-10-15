using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharp.Components.SubcircuitBehaviors;
using System.Collections.Generic;
using SpiceSharp.Entities.Local;
using System.Threading;
using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A component that can contain other entities and group them.
    /// </summary>
    /// <seealso cref="Component" />
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

            RegisterPreparer(typeof(IBiasingBehavior), new BiasingPreparer());
            RegisterPreparer(typeof(IFrequencyBehavior), new FrequencyPreparer());
            RegisterPreparer(typeof(ITimeBehavior), new TimePreparer());
            RegisterPreparer(typeof(INoiseBehavior), new NoisePreparer());
        }

        private readonly static Dictionary<Type, ISimulationPreparer> Preparers = new Dictionary<Type, ISimulationPreparer>();
        private readonly static ReaderWriterLockSlim Lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        /// <summary>
        /// Registers the simulation preparers.
        /// </summary>
        /// <param name="behaviorType">Type of the behavior.</param>
        /// <param name="preparer">The preparer.</param>
        public static void RegisterPreparer(Type behaviorType, ISimulationPreparer preparer)
        {
            Lock.EnterWriteLock();
            try
            {
                Preparers.Add(behaviorType, preparer);
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets or sets the entities in the subcircuit.
        /// </summary>
        /// <value>
        /// The entities.
        /// </value>
        public IEntityCollection Entities { get; set; }

        /// <summary>
        /// Gets the local pin names. These will globally look like other pin names.
        /// </summary>
        /// <value>
        /// The local pin names.
        /// </value>
        public string[] Pins { get; }

        /// <summary>
        /// The mock simulation used to create behaviors inside the subcircuit
        /// </summary>
        private SubcircuitSimulation[] _simulations;

        /// <summary>
        /// Initializes a new instance of the <see cref="Subcircuit"/> class.
        /// </summary>
        /// <param name="name">The name of the subcircuit.</param>
        /// <param name="pins">The local node names in the subcircuit that will be connected outside.</param>
        /// <param name="entities">The entities in the subcircuit.</param>
        public Subcircuit(string name, IEntityCollection entities, params string[] pins)
            : base(name, pins.Length)
        {
            Entities = entities;
            Pins = pins;
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
            if (Entities == null || Entities.Count == 0)
                return;

            // Create the behaviors inside our behaviors
            var nodemap = new Dictionary<string, string>(simulation.Variables.Comparer);
            for (var i = 0; i < PinCount; i++)
                nodemap.Add(Pins[i], GetNode(i));
            if (Parameters.TryGetValue<BaseParameters>(out var ebp))
            {
                if (ebp.GlobalNodes != null)
                {
                    foreach (var g in ebp.GlobalNodes)
                        nodemap.Add(g, g);
                }
            }

            // We need to use a proxy simulation to create the behaviors
            _simulations = new SubcircuitSimulation[GetTaskCount(Entities)];
            var ec = new LocalEntityCollection(entities, Entities, simulation);
            for (var i = 0; i < _simulations.Length; i++)
            {
                _simulations[i] = new SubcircuitSimulation(Name, simulation, nodemap, i, _simulations.Length);
                foreach (var type in simulation.BehaviorTypes)
                {
                    if (Preparers.TryGetValue(type, out var preparer))
                        preparer.Prepare(_simulations[i], simulation, Parameters);
                }
                _simulations[i].Run(ec);
            }

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
            var context = new SubcircuitBindingContext(simulation, eb, _simulations);
            foreach (var behavior in eb.Ordered)
                behavior.Bind(context);
        }

        /// <summary>
        /// Gets the task count.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns>
        /// The number of tasks that will be allocated.
        /// </returns>
        private int GetTaskCount(IEntityCollection entities)
        {
            if (Parameters.TryGetValue<BaseParameters>(out var bp))
            {
                if (bp.Tasks == -1)
                    return entities.Count;
                if (bp.Tasks > 0)
                    return Math.Min(bp.Tasks, entities.Count);
                return Math.Min(Environment.ProcessorCount, entities.Count);
            }
            return 1;
        }
    }
}
