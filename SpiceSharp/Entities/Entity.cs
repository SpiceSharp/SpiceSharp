using System;
using SpiceSharp.Simulations;

namespace SpiceSharp.Entities
{
    /// <summary>
    /// Base class for any circuit object that can take part in simulations.
    /// </summary>
    public abstract class Entity : Parameterized, IEntity
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
        {
            Name = name;
        }

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public abstract void CreateBehaviors(ISimulation simulation);

        /// <summary>
        /// Clones the instance.
        /// </summary>
        /// <returns>
        /// The cloned instance.
        /// </returns>
        protected virtual Entity Clone()
        {
            var clone = (Entity)Activator.CreateInstance(GetType(), Name);
            Reflection.CopyPropertiesAndFields(this, clone);
            return clone;
        }

        /// <summary>
        /// Clones the instance.
        /// </summary>
        /// <returns>
        /// The cloned instance.
        /// </returns>
        ICloneable ICloneable.Clone() => Clone();

        /// <summary>
        /// Copy properties from another entity.
        /// </summary>
        /// <param name="source">The source entity.</param>
        protected virtual void CopyFrom(ICloneable source)
        {
            source.ThrowIfNull(nameof(source));
            Reflection.CopyPropertiesAndFields(source, this);
        }

        /// <summary>
        /// Copies the contents of one interface to this one.
        /// </summary>
        /// <param name="source">The source parameter.</param>
        void ICloneable.CopyFrom(ICloneable source) => CopyFrom(source);

        /// <summary>
        /// Call a parameter method with the specified name.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <returns>
        /// The current instance for chaining.
        /// </returns>
        public new IEntity SetParameter(string name)
        {
            base.SetParameter(name);
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
        public new IEntity SetParameter<P>(string name, P value)
        {
            base.SetParameter(name, value);
            return this;
        }
    }
}
