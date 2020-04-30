using System;
using SpiceSharp.Attributes;

namespace SpiceSharp
{
    /// <summary>
    /// Base class for a set of parameters.
    /// </summary>
    /// <remarks>
    /// This class allows accessing parameters by their metadata. Metadata is specified by using 
    /// the <see cref="ParameterNameAttribute"/> and <see cref="ParameterInfoAttribute"/>.
    /// </remarks>
    /// <seealso cref="IImportParameterSet{T}"/>
    /// <seealso cref="IParameterSet"/>
    public abstract class ParameterSet : 
        IImportParameterSet<ParameterSet>, IParameterSet
    {
        /// <inheritdoc/>
        public virtual void CalculateDefaults()
        {
        }

        /// <summary>
        /// Clones the instance.
        /// </summary>
        /// <returns>
        /// The cloned instance.
        /// </returns>
        protected virtual ICloneable Clone()
        {
            var clone = (ParameterSet) Activator.CreateInstance(GetType());
            clone.CopyFrom(this);
            return clone;
        }

        ICloneable ICloneable.Clone() => Clone();

        /// <summary>
        /// Copies the contents of one interface to this one.
        /// </summary>
        /// <param name="source">The source parameter.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="source"/> does not have the same type.</exception>
        protected virtual void CopyFrom(ICloneable source)
        {
            source.ThrowIfNull(nameof(source));
            Reflection.CopyPropertiesAndFields(source, this);
        }

        void ICloneable.CopyFrom(ICloneable source) => CopyFrom(source);

        /// <inheritdoc/>
        public ParameterSet SetParameter<P>(string name, P value)
        {
            Reflection.Set(this, name, value);
            return this;
        }

        void IImportParameterSet.SetParameter<P>(string name, P value) => SetParameter(name, value);

        /// <inheritdoc/>
        public bool TrySetParameter<P>(string name, P value)
            => Reflection.TrySet(this, name, value);

        /// <inheritdoc/>
        public P GetProperty<P>(string name)
            => Reflection.Get<P>(this, name);

        /// <inheritdoc/>
        public bool TryGetProperty<P>(string name, out P value)
            => Reflection.TryGet(this, name, out value);

        /// <inheritdoc/>
        public Func<P> CreatePropertyGetter<P>(string name)
            => Reflection.CreateGetter<P>(this, name);

        /// <inheritdoc/>
        public Action<P> CreateParameterSetter<P>(string name)
            => Reflection.CreateSetter<P>(this, name);
    }
}
