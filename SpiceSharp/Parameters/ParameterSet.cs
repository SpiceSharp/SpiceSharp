using System;

namespace SpiceSharp.ParameterSets
{
    /// <summary>
    /// The default implementation for a <see cref="IParameterSet"/>. It uses reflection as a 
    /// last line of defense.
    /// </summary>
    /// <remarks>
    /// This class will use the <see cref="IExportPropertySet{P}"/> or <see cref="IImportParameterSet{P}"/>
    /// if they are defined on the class to avoid reflection. If it isn't defined, it will fall back to
    /// reflection.
    /// </remarks>
    /// <seealso cref="IParameterSet"/>
    public abstract class ParameterSet : IParameterSet
    {
        /// <summary>
        /// Clones the instance.
        /// </summary>
        /// <returns>
        /// The cloned instance.
        /// </returns>
        protected virtual ICloneable Clone()
        {
            var clone = (ParameterSet)Activator.CreateInstance(GetType());
            clone.CopyFrom(this);
            return clone;
        }

        /// <inheritdoc/>
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
            ReflectionHelper.CopyPropertiesAndFields(source, this);
        }

        /// <inheritdoc/>
        void ICloneable.CopyFrom(ICloneable source) => CopyFrom(source);

        /// <inheritdoc/>
        public virtual void SetParameter<P>(string name, P value)
        {
            if (this is IImportParameterSet<P> ips)
                ips.SetParameter(name, value);
            else
                ReflectionHelper.Set(this, name, value);
        }

        /// <inheritdoc/>
        public virtual bool TrySetParameter<P>(string name, P value)
        {
            if (this is IImportParameterSet<P> ips)
                return ips.TrySetParameter(name, value);
            else
                return ReflectionHelper.TrySet(this, name, value);
        }

        /// <inheritdoc/>
        public virtual P GetProperty<P>(string name)
        {
            if (this is IExportPropertySet<P> eps)
                return eps.GetProperty(name);
            else
                return ReflectionHelper.Get<P>(this, name);
        }

        /// <inheritdoc/>
        public virtual bool TryGetProperty<P>(string name, out P value)
        {
            if (this is IExportPropertySet<P> eps)
            {
                value = eps.TryGetProperty(name, out var isValid);
                return isValid;
            }
            else
                return ReflectionHelper.TryGet(this, name, out value);
        }

        /// <inheritdoc/>
        public virtual Action<P> CreateParameterSetter<P>(string name)
        {
            if (this is IImportParameterSet<P> ips)
                return ips.CreateParameterSetter(name);
            else
                return ReflectionHelper.CreateSetter<P>(this, name);
        }

        /// <inheritdoc/>
        public virtual Func<P> CreatePropertyGetter<P>(string name)
        {
            if (this is IExportPropertySet<P> eps)
                return eps.CreatePropertyGetter(name);
            else
                return ReflectionHelper.CreateGetter<P>(this, name);
        }
    }
}
