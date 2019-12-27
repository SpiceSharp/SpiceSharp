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
    public abstract class Parameterized : IParameterized
    {
        /// <summary>
        /// Gets the parameter set of the specified type.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <returns>
        /// The parameter set.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if the parameter set could not be found.</exception>
        public P GetParameterSet<P>() where P : IParameterSet
        {
            if (this is IParameterized<P> parameterized)
                return parameterized.Parameters;
            throw new ArgumentException(Properties.Resources.Parameters_ParameterSetNotFound.FormatString(typeof(P)));
        }

        /// <summary>
        /// Tries to get the parameter set of the specified type.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <param name="value">The parameter set.</param>
        /// <returns>
        ///   <c>true</c> if the parameter set was found; otherwise, <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// Gets all parameter sets.
        /// </summary>
        /// <value>
        /// The parameter sets.
        /// </value>
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

        /// <summary>
        /// Method for calculating the default values of derived parameters.
        /// </summary>
        /// <remarks>
        /// These calculations should be run whenever a parameter has been changed.
        /// </remarks>
        public virtual void CalculateDefaults()
        {
            foreach (var ps in ParameterSets)
                ps.CalculateDefaults();
        }

        /// <summary>
        /// Call a parameter method with the specified name.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        public void SetParameter(string name)
        {
            foreach (var ps in ParameterSets)
            {
                if (ps.TrySetParameter(name))
                    return;
            }
            throw new ParameterNotFoundException(name, this);
        }

        /// <summary>
        /// Tries calling a parameter method with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        ///   <c>true</c> if the method was called; otherwise <c>false</c>.
        /// </returns>
        public virtual bool TrySetParameter(string name)
        {
            foreach (var ps in ParameterSets)
            {
                if (ps.TrySetParameter(name))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        public virtual void SetParameter<P>(string name, P value)
        {
            foreach (var ps in ParameterSets)
            {
                if (ps.TrySetParameter(name, value))
                    return;
            }
            throw new ParameterNotFoundException(name, this);
        }

        /// <summary>
        /// Tries to set the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the parameter was set; otherwise <c>false</c>.
        /// </returns>
        public virtual bool TrySetParameter<P>(string name, P value)
        {
            foreach (var ps in ParameterSets)
            {
                if (ps.TrySetParameter(name, value))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name.</param>
        /// <returns>
        /// The value.
        /// </returns>
        public virtual P GetProperty<P>(string name)
        {
            foreach (var ps in ParameterSets)
            {
                if (ps.TryGetProperty(name, out P value))
                    return value;
            }
            throw new ParameterNotFoundException(name, this);
        }

        /// <summary>
        /// Tries to get the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the parameter was found; otherwise <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// Creates a getter for a parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// A getter if the parameter exists; otherwise <c>null</c>.
        /// </returns>
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

        /// <summary>
        /// Creates a setter for a parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// A setter if the parameter exists; otherwise <c>null</c>.
        /// </returns>
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
