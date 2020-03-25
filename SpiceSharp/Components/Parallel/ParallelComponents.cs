using SpiceSharp.Behaviors;
using SpiceSharp.Components.ParallelBehaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A component that can execute multiple <see cref="IComponent"/> instances in parallel.
    /// </summary>
    /// <seealso cref="Entity" />
    /// <seealso cref="IComponent" />
    public class ParallelComponents : Entity, IComponent,
        IParameterized<BaseParameters>
    {
        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        public BaseParameters Parameters { get; } = new BaseParameters();

        /// <summary>
        /// Gets or sets the model of the component.
        /// </summary>
        /// <value>
        /// The model of the component.
        /// </value>
        public string Model { get; set; }

        /// <summary>
        /// Gets the number of nodes.
        /// </summary>
        /// <value>
        /// The number of nodes.
        /// </value>
        public int PinCount
        {
            get
            {
                var count = 0;
                foreach (var entity in Parameters.Entities)
                {
                    if (entity is IComponent component)
                        count += component.PinCount;
                }
                return count;
            }
        }

        /// <summary>
        /// Connects the component in the circuit.
        /// </summary>
        /// <param name="nodes">The node indices.</param>
        /// <returns>
        /// The instance calling the method for chaining.
        /// </returns>
        public IComponent Connect(params string[] nodes)
        {
            // We don't really have any connections of our own
            return this;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelComponents"/> class.
        /// </summary>
        /// <param name="name">The name of the component.</param>
        public ParallelComponents(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelComponents"/> class.
        /// </summary>
        /// <param name="name">The name of the component.</param>
        /// <param name="entities">The components that can be executed in parallel.</param>
        public ParallelComponents(string name, params IEntity[] entities)
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
        public ParallelComponents(string name, IEnumerable<IEntity> entities)
            : base(name)
        {
            var collection = new EntityCollection();
            foreach (var entity in entities)
                collection.Add(entity);
            Parameters.Entities = collection;
        }

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void CreateBehaviors(ISimulation simulation)
        {
            // Nothing to see here!
            if (Parameters.Entities == null || Parameters.Entities.Count == 0)
                return;

            var container = new BehaviorContainer(Name);
            CalculateDefaults();

            // Create our parallel simulation
            var psim = new ParallelSimulation(simulation, this);
            ConvergenceBehavior.Prepare(psim);
            FrequencyBehavior.Prepare(psim);
            NoiseBehavior.Prepare(psim);

            // Create the behaviors
            psim.Run(Parameters.Entities);

            // Create the parallel behaviors
            container
                .AddIfNo<ITemperatureBehavior>(simulation, () => new TemperatureBehavior(Name, psim))
                .AddIfNo<IAcceptBehavior>(simulation, () => new AcceptBehavior(Name, psim))
                .AddIfNo<ITimeBehavior>(simulation, () => new TimeBehavior(Name, psim))
                .AddIfNo<IConvergenceBehavior>(simulation, () => new ConvergenceBehavior(Name, psim))
                .AddIfNo<IBiasingBehavior>(simulation, () => new BiasingBehavior(Name, psim))
                .AddIfNo<IBiasingUpdateBehavior>(simulation, () => new BiasingUpdateBehavior(Name, psim))
                .AddIfNo<INoiseBehavior>(simulation, () => new NoiseBehavior(Name, psim))
                .AddIfNo<IFrequencyBehavior>(simulation, () => new FrequencyBehavior(Name, psim))
                .AddIfNo<IFrequencyUpdateBehavior>(simulation, () => new FrequencyUpdateBehavior(Name, psim));
            simulation.EntityBehaviors.Add(container);
        }

        /// <summary>
        /// Gets the node name by pin index.
        /// </summary>
        /// <param name="index">The pin index.</param>
        /// <returns>
        /// The node index.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is out of range.</exception>
        public string GetNode(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (Parameters.Entities != null)
            {
                foreach (var entity in Parameters.Entities)
                {
                    if (entity is IComponent component)
                    {
                        if (index > component.PinCount)
                            index -= component.PinCount;
                        else
                            return component.GetNode(index);
                    }
                }
            }
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        /// <summary>
        /// Gets the node indexes (in order).
        /// </summary>
        /// <param name="variables">The set of variables.</param>
        /// <returns>
        /// An enumerable for all nodes.
        /// </returns>
        public IReadOnlyList<IVariable> MapNodes(IVariableSet variables)
        {
            variables.ThrowIfNull(nameof(variables));
            var list = new List<IVariable>();
            if (Parameters.Entities != null)
            {
                foreach (var entity in Parameters.Entities)
                {
                    if (entity is IComponent component)
                    {
                        foreach (var variable in component.MapNodes(variables))
                            list.Add(variable);
                    }
                }
            }
            return list.AsReadOnly();
        }
    }
}
