using SpiceSharp.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SpiceSharp.ParameterSets
{
    /// <summary>
    /// The default implementation of an <see cref="IParameterSetCollection"/>. This implementation
    /// also makes the collection itself an <see cref="IParameterSet"/>.
    /// </summary>
    /// <seealso cref="IParameterSetCollection" />
    /// <seealso cref="ParameterSet" />
    public class ParameterSetCollection : ParameterSet,
        IParameterSetCollection
    {
        /// <inheritdoc/>
        public virtual P GetParameterSet<P>() where P : IParameterSet, ICloneable<P>
        {
            if (this is IParameterized<P> parameterized)
                return parameterized.Parameters;
            else
            {
                foreach (var ps in ParameterSets)
                {
                    if (ps is P result)
                        return result;
                }
            }
            throw new TypeNotFoundException(typeof(P), Properties.Resources.ParameterSets_NotDefined.FormatString(typeof(P).FullName));
        }

        /// <inheritdoc/>
        public virtual bool TryGetParameterSet<P>(out P value) where P : IParameterSet, ICloneable<P>
        {
            if (this is IParameterized<P> parameterized)
            {
                value = parameterized.Parameters;
                return true;
            }
            else
            {
                foreach (var ps in ParameterSets)
                {
                    if (ps is P result)
                    {
                        value = result;
                        return true;
                    }
                }
            }
            value = default;
            return false;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<IParameterSet> ParameterSets
        {
            get
            {
                foreach (var type in InterfaceCache.Get(GetType()).Where(t =>
                {
                    var info = t.GetTypeInfo();
                    return info.IsGenericType && info.GetGenericTypeDefinition() == typeof(IParameterized<>);
                }))
                {
                    // Get the value associated with this interface (returned by IParameterized<>.Parameters)
                    yield return (IParameterSet)type.GetTypeInfo().GetProperty("Parameters").GetValue(this);
                }
            }
        }

        /// <inheritdoc/>
        public override void SetParameter<P>(string name, P value)
        {
            // First try our parameter sets
            foreach (var ps in ParameterSets)
            {
                if (ps.TrySetParameter(name, value))
                    return;
            }
            base.SetParameter(name, value);
        }

        /// <inheritdoc/>
        public override bool TrySetParameter<P>(string name, P value)
        {
            // First try our parameter sets
            foreach (var ps in ParameterSets)
            {
                if (ps.TrySetParameter(name, value))
                    return true;
            }
            return base.TrySetParameter(name, value);
        }

        /// <inheritdoc/>
        public override P GetProperty<P>(string name)
        {
            // First try our parameter sets
            foreach (var ps in ParameterSets)
            {
                if (ps.TryGetProperty(name, out P value))
                    return value;
            }
            return base.GetProperty<P>(name);
        }

        /// <inheritdoc/>
        public override bool TryGetProperty<P>(string name, out P value)
        {
            // First try our parameter sets
            foreach (var ps in ParameterSets)
            {
                if (ps.TryGetProperty(name, out value))
                    return true;
            }
            return base.TryGetProperty(name, out value);
        }

        /// <inheritdoc/>
        public override Action<P> CreateParameterSetter<P>(string name)
        {
            // First try our parameter sets
            foreach (var ps in ParameterSets)
            {
                var setter = ps.CreateParameterSetter<P>(name);
                if (setter != null)
                    return setter;
            }
            return base.CreateParameterSetter<P>(name);
        }

        /// <inheritdoc/>
        public override Func<P> CreatePropertyGetter<P>(string name)
        {
            // First try our parameter sets
            foreach (var ps in ParameterSets)
            {
                var getter = ps.CreatePropertyGetter<P>(name);
                if (getter != null)
                    return getter;
            }
            return base.CreatePropertyGetter<P>(name);
        }
    }
}
