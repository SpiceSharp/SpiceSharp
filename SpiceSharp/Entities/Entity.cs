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
            : base(new ParameterSetDictionary(new InterfaceTypeDictionary<IParameterSet>()))
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parameters">The parameters.</param>
        protected Entity(string name, IParameterSetDictionary parameters)
            : base(parameters)
        {
            Name = name;
        }

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="behaviors">An <see cref="IBehaviorContainer" /> where the behaviors can be stored.</param>
        public virtual void CreateBehaviors(ISimulation simulation, IBehaviorContainer behaviors)
        {
            simulation.ThrowIfNull(nameof(simulation));
            behaviors.ThrowIfNull(nameof(behaviors));

            if (Parameters.Count > 0)
            {
                foreach (var ps in Parameters.Values)
                {
                    // TODO: This shouldn't be necessary. The unique values should be returned
                    if (!behaviors.Parameters.ContainsKey(ps.GetType()))
                    {
                        if (LinkParameters)
                            behaviors.Parameters.Add(ps);
                        else
                            behaviors.Parameters.Add((IParameterSet)ps.Clone());
                    }
                }
                behaviors.Parameters.CalculateDefaults();
            }
        }

        /// <summary>
        /// Clones the entity
        /// </summary>
        /// <returns></returns>
        protected override ICloneable Clone()
        {
            var clone = (IEntity)Activator.CreateInstance(GetType(), Name, Parameters.Clone());
            Reflection.CopyPropertiesAndFields(this, clone);
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
