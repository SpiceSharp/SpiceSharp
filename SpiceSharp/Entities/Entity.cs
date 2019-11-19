using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Entities
{
    /// <summary>
    /// Base class for any circuit object that can take part in simulations.
    /// </summary>
    public abstract class Entity : Parameterized<IEntity>, IEntity
    {
        /// <summary>
        /// Gets or sets a value indicating whether the parameters should reference that of the entity.
        /// If the parameters are not referenced, then the parameters are cloned instead.
        /// </summary>
        /// <value>
        ///   <c>true</c> if parameters are referenced; otherwise, <c>false</c>.
        /// </value>
        public bool LinkParameters { get; set; } = true;

        /// <summary>
        /// Gets the name of the entity.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        protected Entity(string name)
            : base(new ParameterSetDictionary())
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parameters">The parameters.</param>
        protected Entity(string name, ParameterSetDictionary parameters)
            : base(parameters)
        {
            Name = name;
        }

        /// <summary>
        /// Creates behaviors for the specified simulation that describe this <see cref="Entity"/>.
        /// </summary>
        /// <remarks>
        /// The order typically indicates hierarchy. The entity will create the behaviors in reverse order, allowing
        /// the most specific child class to be used that is necessary. For example, the <see cref="OP"/> simulation needs
        /// <see cref="ITemperatureBehavior"/> and an <see cref="IBiasingBehavior"/>. The entity will first look for behaviors
        /// of type <see cref="IBiasingBehavior"/>, and then for the behaviors of type <see cref="ITemperatureBehavior"/>. However,
        /// if the behavior that was created for <see cref="IBiasingBehavior"/> also implements <see cref="ITemperatureBehavior"/>,
        /// then then entity will not create a new instance of the behavior.
        /// </remarks>
        /// <param name="simulation">The simulation requesting the behaviors.</param>
        /// <param name="entities">The entities being processed, used by the entity to find linked entities.</param>
        public virtual void CreateBehaviors(ISimulation simulation, IEntityCollection entities)
        {
            simulation.ThrowIfNull(nameof(simulation));
            entities.ThrowIfNull(nameof(entities));

            // Skip creating behaviors if the entity is already defined in the pool
            var pool = simulation.EntityBehaviors;
            if (pool.ContainsKey(Name))
                return;

            // Create our entity behavior container
            var eb = CreateBehaviorContainer(simulation, entities);
            if (eb != null && eb.Parameters.Count > 0 || eb.Count > 0)
                simulation.EntityBehaviors.Add(Name, eb);
        }

        /// <summary>
        /// Creates the <see cref="BehaviorContainer"/> containing the behaviors.
        /// </summary>
        /// <param name="simulation">The simulation that requests the behaviors.</param>
        /// <param name="entities">The other entities.</param>
        /// <returns>
        /// A container with behaviors for the simulation.
        /// </returns>
        protected virtual BehaviorContainer CreateBehaviorContainer(ISimulation simulation, IEntityCollection entities)
        {
            BehaviorContainer behaviors = null;
            if (Parameters.Count > 0)
            {
                behaviors = new BehaviorContainer(Name, (IParameterSetDictionary)(LinkParameters ? Parameters : Parameters.Clone()));
                foreach (var p in behaviors.Parameters.Values)
                    p.CalculateDefaults();
            }

            // Create behaviors
            if (behaviors == null)
                behaviors = new BehaviorContainer(Name);
            CreateBehaviors(simulation, entities, behaviors);
            return behaviors;
        }

        /// <summary>
        /// Create one or more behaviors for the simulation.
        /// </summary>
        /// <param name="simulation">The simulation for which behaviors need to be created.</param>
        /// <param name="entities">The other entities.</param>
        /// <param name="behaviors">A container where all behaviors are to be stored.</param>
        protected abstract void CreateBehaviors(ISimulation simulation, IEntityCollection entities, BehaviorContainer behaviors);

        /// <summary>
        /// Clones the entity
        /// </summary>
        /// <returns></returns>
        protected override ICloneable Clone()
        {
            var clone = (IEntity) Activator.CreateInstance(GetType(), Name);
            clone.CopyFrom(this);
            return clone;
        }

        /// <summary>
        /// Copy properties from another entity.
        /// </summary>
        /// <param name="source">The source entity.</param>
        protected override void CopyFrom(ICloneable source)
        {
            source.ThrowIfNull(nameof(source));
            Reflection.CopyPropertiesAndFields(source, this);
        }

        /// <summary>
        /// Call a parameter method with the specified name.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <returns>
        /// The current instance for chaining.
        /// </returns>
        public override IEntity Set(string name)
        {
            Parameters.Set(name);
            return this;
        }

        /// <summary>
        /// Sets the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The current instance for chaining.
        /// </returns>
        public override IEntity Set<P>(string name, P value)
        {
            Parameters.Set(name, value);
            return this;
        }
    }
}
