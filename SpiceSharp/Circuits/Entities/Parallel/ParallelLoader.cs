using SpiceSharp.Behaviors;
using SpiceSharp.Circuits.ParallelBehaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Entities.Local;
using SpiceSharp.Entities.ParallelLoaderBehaviors;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// An entity that will allow multiple entities to be loaded in parallel.
    /// </summary>
    /// <seealso cref="Entity" />
    public class ParallelLoader : Entity
    {
        static ParallelLoader()
        {
            RegisterBehaviorFactory(typeof(ParallelLoader), new BehaviorFactoryDictionary
            {
                { typeof(ITemperatureBehavior), e => new TemperatureBehavior(e.Name) },
                { typeof(IBiasingBehavior), e => new BiasingBehavior(e.Name) },
                { typeof(IFrequencyBehavior), e => new FrequencyBehavior(e.Name) }
            });
        }

        /// <summary>
        /// Gets or sets the entities.
        /// </summary>
        /// <value>
        /// The entities.
        /// </value>
        public IEntityCollection Entities { get; set; }

        private ParallelSimulation _simulation;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelLoader"/> class.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        public ParallelLoader(string name)
            : base(name)
        {
            Parameters.Add(new BaseParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelLoader"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="entities">The entities.</param>
        public ParallelLoader(string name, EntityCollection entities)
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
            _simulation = new ParallelSimulation(simulation);
            var ec = new LocalEntityCollection(entities, Entities, simulation);
            _simulation.Run(ec);

            // Create our own behaviors
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
            var context = new ParallelBindingContext(simulation, eb, _simulation.EntityBehaviors);
            foreach (var behavior in eb.Ordered)
                behavior.Bind(context);
        }
    }
}
