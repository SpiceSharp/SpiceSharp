using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Entities.Local;
using SpiceSharp.Entities.ParallelLoaderBehaviors;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading;

namespace SpiceSharp.Entities
{
    /// <summary>
    /// An entity that will allow multiple entities to be loaded in parallel.
    /// </summary>
    /// <seealso cref="Entity" />
    public class ParallelEntity : Entity
    {
        static ParallelEntity()
        {
            RegisterBehaviorFactory(typeof(ParallelEntity), new BehaviorFactoryDictionary
            {
                { typeof(IInitialConditionBehavior), e => new InitialConditionBehavior(e.Name) },
                { typeof(ITemperatureBehavior), e => new TemperatureBehavior(e.Name) },
                { typeof(IBiasingBehavior), e => new BiasingBehavior(e.Name) },
                { typeof(IFrequencyBehavior), e => new FrequencyBehavior(e.Name) },
                { typeof(ITimeBehavior), e => new TimeBehavior(e.Name) },
                { typeof(IAcceptBehavior), e => new AcceptBehavior(e.Name) },
                { typeof(INoiseBehavior), e => new NoiseBehavior(e.Name) }
            });

            RegisterSimulationPreparers(typeof(IBiasingBehavior), new BiasingPreparer());
            RegisterSimulationPreparers(typeof(IFrequencyBehavior), new FrequencyPreparer());
            RegisterSimulationPreparers(typeof(ITimeBehavior), new TimePreparer());
            RegisterSimulationPreparers(typeof(INoiseBehavior), new NoisePreparer());
        }

        /// <summary>
        /// Gets the preparers that act on the <see cref="ParallelSimulation"/> before and after binding behaviors.
        /// </summary>
        /// <value>
        /// The preparers.
        /// </value>
        private readonly static Dictionary<Type, IParallelPreparer> Preparers = new Dictionary<Type, IParallelPreparer>();
        private readonly static ReaderWriterLockSlim Lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        /// <summary>
        /// Registers the simulation preparers.
        /// </summary>
        /// <param name="behaviorType">Type of the behavior.</param>
        /// <param name="preparer">The preparer.</param>
        public static void RegisterSimulationPreparers(Type behaviorType, IParallelPreparer preparer)
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
        /// Gets or sets the entities.
        /// </summary>
        /// <value>
        /// The entities.
        /// </value>
        public IEntityCollection Entities { get; set; }

        private IParallelSimulation[] _simulations;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelEntity"/> class.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        public ParallelEntity(string name)
            : base(name)
        {
            Parameters.Add(new BaseParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelEntity"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="entities">The entities.</param>
        public ParallelEntity(string name, EntityCollection entities)
            : this(name)
        {
            Entities = entities;
        }

        /// <summary>
        /// Creates behaviors for the specified simulation that describe this <see cref="Entity" />.
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
            simulation.ThrowIfNull(nameof(simulation));
            if (simulation.EntityBehaviors.ContainsKey(Name))
                return;
            if (Entities == null)
                return;

            // Intercept the local behaviors to add them to our own behaviors
            _simulations = new IParallelSimulation[GetTaskCount(entities)];
            var ec = new LocalEntityCollection(entities, Entities, simulation);
            for (var i = 0; i < _simulations.Length; i++)
            {
                _simulations[i] = new ParallelSimulation(simulation, i);
                foreach (var type in simulation.BehaviorTypes)
                {
                    if (Preparers.TryGetValue(type, out var preparer))
                        preparer.Prepare(_simulations[i], simulation, Parameters);
                }

                // Basically creates the behaviors for the simulation
                _simulations[i].Run(ec);
            }

            // Create our own behaviors
            base.CreateBehaviors(simulation, entities);
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
                if (bp.Tasks == 0)
                    return entities.Count;
                if (bp.Tasks > 0)
                    return bp.Tasks;
                return Environment.ProcessorCount;
            }
            return 1;
        }

        /// <summary>
        /// Binds the behaviors to the simulation.
        /// </summary>
        /// <param name="eb">The entity behaviors and parameters.</param>
        /// <param name="simulation">The simulation to be bound to.</param>
        /// <param name="entities">The entities that the entity may be connected to.</param>
        protected override void BindBehaviors(BehaviorContainer eb, ISimulation simulation, IEntityCollection entities)
        {
            var context = new ParallelBindingContext(simulation, eb, _simulations);
            foreach (var behavior in eb.Ordered)
                behavior.Bind(context);
        }
    }
}
