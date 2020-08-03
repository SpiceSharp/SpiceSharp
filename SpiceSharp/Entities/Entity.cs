using SpiceSharp.Behaviors;
using SpiceSharp.Diagnostics;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Entities
{
    /// <summary>
    /// Base class for any circuit object that can take part in simulations.
    /// </summary>
    /// <seealso cref="ParameterSetCollection" />
    /// <seealso cref="IEntity" />
    public abstract class Entity : ParameterSetCollection,
        IEntity
    {
        /// <summary>
        /// Gets or sets a value indicating whether the parameters should reference that of the entity.
        /// If the parameters are not referenced, then the parameters are cloned instead.
        /// </summary>
        /// <value>
        ///   <c>true</c> if parameters are referenced instead of cloned; otherwise, <c>false</c>.
        /// </value>
        public bool LinkParameters { get; set; } = true;

        /// <inheritdoc/>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        protected Entity(string name)
        {
            Name = name.ThrowIfNull(nameof(name));
        }

        /// <inheritdoc/>
        public abstract void CreateBehaviors(ISimulation simulation);

        /// <summary>
        /// Sets the value of a parameter of the specified type and with the specified name. This is just a wrapper
        /// that allows chaining these commands.
        /// </summary>
        /// <typeparam name="P">The parameter value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>
        /// The entity.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        /// <exception cref="ParameterNotFoundException">Thrown if the parameter was not found.</exception>
        public new Entity SetParameter<P>(string name, P value)
        {
            base.SetParameter(name, value);
            return this;
        }

        /// <inheritdoc/>
        protected override ICloneable Clone()
        {
            var clone = (Entity)Activator.CreateInstance(GetType(), Name);
            clone.CopyFrom(this);
            return clone;
        }

        /// <summary>
        /// Returns a string that represents the current entity.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return "{0} ({1})".FormatString(Name, GetType().Name);
        }
    }

    /// <summary>
    /// Base class for any circuit object that can take part in simulations.
    /// This implementation will by default use dependency injection for resolving behaviors.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <seealso cref="ParameterSetCollection" />
    /// <seealso cref="IEntity" />
    public abstract class Entity<TContext> : Entity,
        IEntity<TContext>
        where TContext : IBindingContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Entity{TContext}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        protected Entity(string name) 
            : base(name)
        {
        }

        /// <inheritdoc />
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            DI.Resolve(simulation, this, behaviors);
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
