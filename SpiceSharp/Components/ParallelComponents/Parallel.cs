using SpiceSharp.Behaviors;
using SpiceSharp.Components.ParallelComponents;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using SpiceSharp.Validation;
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
    /// <para>It is possible to combine entities into a <see cref="Subcircuit"/> first, and having them use
    /// a local solver. This keeps the shared resources very limited, allowing each subcircuit to do
    /// its work without interference from read-write locking. This option is very advantageous if the
    /// subcircuits are large, but have only a few voltage nodes common with the outside.</para>
    /// </remarks>
    /// <seealso cref="Entity" />
    /// <seealso cref="IComponent" />
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="Parameters"/>
    public class Parallel : Entity<Parameters>,
        IEntity,
        IRuleSubject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Parallel"/> class.
        /// </summary>
        /// <param name="name">The name of the component.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public Parallel(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parallel"/> class.
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
        /// Initializes a new instance of the <see cref="Parallel"/> class.
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
            var behaviors = new BehaviorContainer(Name);
            if (Parameters.Entities != null || Parameters.Entities.Count > 0)
            {
                // Create our local simulation and binding context to allow behaviors to do stuff
                var localSim = new ParallelSimulation(simulation, this);
                var context = new ParallelBindingContext(this, localSim, behaviors);

                // Let's create our behaviors
                // Note: we do this first, such that any parallel simulation states can be added to the local simulation
                behaviors.Add(new EntitiesBehavior(context));
                behaviors.Build(simulation, context)
                    .AddIfNo<ITemperatureBehavior>(context => new Temperature(context))
                    .AddIfNo<IConvergenceBehavior>(context => new Convergence(context))
                    .AddIfNo<IBiasingBehavior>(context => new Biasing(context))
                    .AddIfNo<IBiasingUpdateBehavior>(context => new BiasingUpdate(context))
                    .AddIfNo<IFrequencyBehavior>(context => new Frequency(context))
                    .AddIfNo<IFrequencyUpdateBehavior>(context => new FrequencyUpdate(context))
                    .AddIfNo<INoiseBehavior>(context => new ParallelComponents.Noise(context))
                    .AddIfNo<ITimeBehavior>(context => new Time(context))
                    .AddIfNo<IAcceptBehavior>(context => new Accept(context));

                // Run the simulation
                foreach (var _ in localSim.Run(Parameters.Entities))
                { }

                // Allow the behaviors to fetch the behaviors if they want
                foreach (var behavior in behaviors)
                {
                    if (behavior is IParallelBehavior parallelBehavior)
                        parallelBehavior.FetchBehaviors(context);
                }
            }
            simulation.EntityBehaviors.Add(behaviors);
        }

        /// <inheritdoc/>
        public void Apply(IRules rules)
        {
            foreach (var entity in Parameters.Entities)
            {
                if (entity is IRuleSubject subject)
                    subject.Apply(rules);
            }
        }
    }
}
