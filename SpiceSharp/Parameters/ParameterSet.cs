using System;
using SpiceSharp.Diagnostics;

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
        /// <inheritdoc/>
        public virtual void SetParameter<P>(string name, P value)
        {
            // If we have a generic implementation for it, use that instead
            if (this is IImportParameterSet<P> ips)
            {
                var method = ips.GetParameterSetter(name);
                if (method != null)
                {
                    method.Invoke(value);
                    return;
                }
            }
            throw new ParameterNotFoundException(this, name, typeof(P));
        }

        /// <inheritdoc/>
        public virtual bool TrySetParameter<P>(string name, P value)
        {
            // If we have a generic implementation for it, use that instead
            if (this is IImportParameterSet<P> ips)
            {
                var method = ips.GetParameterSetter(name);
                if (method != null)
                {
                    method.Invoke(value);
                    return true;
                }
            }
            return false;
        }

        /// <inheritdoc/>
        public virtual P GetProperty<P>(string name)
        {
            // If we have a generic implementation for it, use that instead
            if (this is IExportPropertySet<P> eps)
            {
                var method = eps.GetPropertyGetter(name);
                if (method != null)
                    return method.Invoke();
            }
            throw new ParameterNotFoundException(this, name, typeof(P));
        }

        /// <inheritdoc/>
        public virtual bool TryGetProperty<P>(string name, out P value)
        {
            // If we have a generic implementation for it, use that instead
            if (this is IExportPropertySet<P> eps)
            {
                var method = eps.GetPropertyGetter(name);
                if (method != null)
                {
                    value = method.Invoke();
                    return true;
                }
            }
            value = default;
            return false;
        }

        /// <inheritdoc/>
        public virtual Action<P> CreateParameterSetter<P>(string name)
        {
            // If we have a generic implementation for it, use that instead
            if (this is IImportParameterSet<P> ips)
                return ips.GetParameterSetter(name);
            return null;
        }

        /// <inheritdoc/>
        public virtual Func<P> CreatePropertyGetter<P>(string name)
        {
            // If we have a generic implementation for it, use that instead
            if (this is IExportPropertySet<P> eps)
                return eps.GetPropertyGetter(name);
            return null;
        }
    }

    /// <summary>
    /// The default implementation for a <see cref="IParameterSet"/>, but which is cloneable
    /// just by copying its members.
    /// </summary>
    /// <typeparam name="P">The parameter set type.</typeparam>
    public abstract class ParameterSet<P> : ParameterSet, ICloneable<P>
    {
        /// <inheritdoc/>
        public virtual P Clone() => (P)MemberwiseClone();
    }
}
