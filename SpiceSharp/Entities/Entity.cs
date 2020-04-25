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

        void ICloneable.CopyFrom(ICloneable source) => CopyFrom(source);

        /// <inheritdoc/>
        public new IEntity SetParameter(string name)
        {
            base.SetParameter(name);
            return this;
        }

        /// <inheritdoc/>
        public new IEntity SetParameter<P>(string name, P value)
        {
            base.SetParameter(name, value);
            return this;
        }
    }
}
