using SpiceSharp.Diagnostics;
using SpiceSharp.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SpiceSharp
{
    /// <summary>
    /// A base class for instances that define <see cref="IParameterSet"/>.
    /// </summary>
    /// <seealso cref="IParameterized"/>
    /// <seealso cref="IImportParameterSet"/>
    /// <seealso cref="IExportPropertySet"/>
    public abstract class Parameterized : IParameterized, IImportParameterSet, IExportPropertySet
    {
        /// <inheritdoc/>
        public P GetParameterSet<P>() where P : IParameterSet
        {
            if (this is IParameterized<P> parameterized && parameterized.Parameters != null)
                return parameterized.Parameters;
            throw new TypeNotFoundException(Properties.Resources.ParameterSets_NotDefined.FormatString(typeof(P).FullName));
        }

        /// <inheritdoc/>
        public bool TryGetParameterSet<P>(out P value) where P : IParameterSet
        {
            if (this is IParameterized<P> parameterized)
            {
                value = parameterized.Parameters;
                return true;
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
        public virtual void CalculateDefaults()
        {
            foreach (var ps in ParameterSets)
                ps.CalculateDefaults();
        }

        /// <inheritdoc/>
        public void SetParameter(string name)
        {
            foreach (var ps in ParameterSets)
            {
                if (ps != null && ps.TrySetParameter(name))
                    return;
            }
            throw new ParameterNotFoundException(this, name, typeof(void));
        }

        /// <inheritdoc/>
        public virtual bool TrySetParameter(string name)
        {
            foreach (var ps in ParameterSets)
            {
                if (ps != null && ps.TrySetParameter(name))
                    return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public virtual void SetParameter<P>(string name, P value)
        {
            foreach (var ps in ParameterSets)
            {
                if (ps != null && ps.TrySetParameter(name, value))
                    return;
            }
            throw new ParameterNotFoundException(this, name, typeof(P));
        }

        /// <inheritdoc/>
        public virtual bool TrySetParameter<P>(string name, P value)
        {
            foreach (var ps in ParameterSets)
            {
                if (ps.TrySetParameter(name, value))
                    return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public virtual P GetProperty<P>(string name)
        {
            foreach (var ps in ParameterSets)
            {
                if (ps.TryGetProperty(name, out P value))
                    return value;
            }
            throw new ParameterNotFoundException(this, name, typeof(P));
        }

        /// <inheritdoc/>
        public virtual bool TryGetProperty<P>(string name, out P value)
        {
            foreach (var ps in ParameterSets)
            {
                if (ps.TryGetProperty(name, out value))
                    return true;
            }
            value = default;
            return false;
        }

        /// <inheritdoc/>
        public virtual Func<P> CreatePropertyGetter<P>(string name)
        {
            foreach (var ps in ParameterSets)
            {
                var method = ps.CreatePropertyGetter<P>(name);
                if (method != null)
                    return method;
            }
            return null;
        }

        /// <inheritdoc/>
        public virtual Action<P> CreateParameterSetter<P>(string name)
        {
            foreach (var ps in ParameterSets)
            {
                var method = ps.CreateParameterSetter<P>(name);
                if (method != null)
                    return method;
            }
            return null;
        }
    }
}
