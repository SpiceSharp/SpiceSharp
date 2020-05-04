using SpiceSharp.Behaviors;
using SpiceSharp.Components.ParallelComponents;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A component that can execute multiple behaviors created by <see cref="IEntity"/> instances in parallel.
    /// </summary>
    /// <remarks>
    /// <para>Running entity behaviors in parallel requires shared resources to be locked. Running entities
    /// in parallel are not a good idea if the entities spend a lot of time accessing these
    /// shared resources compared to the time they spend actually computing. Especially since
    /// there is also some overhead in setting up these resources and structures for executing
    /// behaviors in parallel.</para>
    /// <para>It is possible to combine entities into a <see cref="Subcircuit"/> first, and giving them
    /// a local solver. This keeps the shared resources very limited, allowing each subcircuit to do
    /// its work without interference from read-write locking. This option is very advantageous if the
    /// subcircuits are large, but have only a few voltage nodes common with the outside.</para>
    /// </remarks>
    /// <seealso cref="Entity" />
    /// <seealso cref="IComponent" />
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="ParallelComponents.Parameters"/>
    public class Parallel : Entity,
        IComponent,
        IParameterized<Parameters>
    {
        /// <inheritdoc/>
        public Parameters Parameters { get; } = new Parameters();

        /// <inheritdoc/>
        public string Model { get; set; }

        /// <inheritdoc/>
        public IReadOnlyList<string> Nodes
        {
            get
            {
                var list = new List<string>();
                foreach (var entity in Parameters.Entities)
                {
                    if (entity is IComponent component)
                        list.AddRange(component.Nodes);
                }
                return list.AsReadOnly();
            }
        }

        /// <inheritdoc/>
        public IComponent Connect(params string[] nodes)
        {
            // We don't really have any connections of our own
            return this;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelComponents"/> class.
        /// </summary>
        /// <param name="name">The name of the component.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public Parallel(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelComponents"/> class.
        /// </summary>
        /// <param name="name">The name of the component.</param>
        /// <param name="entities">The components that can be executed in parallel.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public Parallel(string name, params IEntity[] entities)
            : base(name)
        {
            var collection = new EntityCollection();
            foreach (var entity in entities)
                collection.Add(entity);
            Parameters.Entities = collection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelComponents"/> class.
        /// </summary>
        /// <param name="name">The name of the component.</param>
        /// <param name="entities">The components that can be executed in parallel.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public Parallel(string name, IEnumerable<IEntity> entities)
            : base(name)
        {
            var collection = new EntityCollection();
            foreach (var entity in entities)
                collection.Add(entity);
            Parameters.Entities = collection;
        }

        /// <inheritdoc/>
        public override void CreateBehaviors(ISimulation simulation)
        {
            // Nothing to see here!
            if (Parameters.Entities == null || Parameters.Entities.Count == 0)
                return;

            var container = new BehaviorContainer(Name);
            CalculateDefaults();

            // Create our parallel simulation
            var psim = new ParallelSimulation(simulation, this);
            Convergence.Prepare(psim);
            Frequency.Prepare(psim);
            ParallelComponents.Noise.Prepare(psim);

            // Create the behaviors
            psim.Run(Parameters.Entities);

            // Create the parallel behaviors
            container
                .AddIfNo<ITemperatureBehavior>(simulation, () => new Temperature(Name, psim))
                .AddIfNo<IAcceptBehavior>(simulation, () => new Accept(Name, psim))
                .AddIfNo<ITimeBehavior>(simulation, () => new Time(Name, psim))
                .AddIfNo<IConvergenceBehavior>(simulation, () => new Convergence(Name, psim))
                .AddIfNo<IBiasingBehavior>(simulation, () => new Biasing(Name, psim))
                .AddIfNo<IBiasingUpdateBehavior>(simulation, () => new BiasingUpdate(Name, psim))
                .AddIfNo<INoiseBehavior>(simulation, () => new ParallelComponents.Noise(Name, psim))
                .AddIfNo<IFrequencyBehavior>(simulation, () => new Frequency(Name, psim))
                .AddIfNo<IFrequencyUpdateBehavior>(simulation, () => new FrequencyUpdate(Name, psim));
            simulation.EntityBehaviors.Add(container);
        }
    }
}
