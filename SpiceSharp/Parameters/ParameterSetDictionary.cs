using System;
using System.Collections.Generic;

namespace SpiceSharp
{
    /// <summary>
    /// A dictionary of <see cref="ParameterSet" />. Only one instance of each type is allowed.
    /// </summary>
    /// <seealso cref="TypeDictionary{ParameterSet}" />
    public class ParameterSetDictionary : TypeDictionary<ParameterSet>, ICloneable, ICloneable<ParameterSetDictionary>
    {
        /// <summary>
        /// Adds a parameter set to the dictionary.
        /// </summary>
        /// <param name="set">The parameter set.</param>
        public void Add(ParameterSet set)
        {
            set.ThrowIfNull(nameof(set));
            Add(set.GetType(), set);
        }

        /// <summary>
        /// Sets the principal parameter.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if a principal parameter was set; otherwise <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Only the first encountered principal parameter will be set.
        /// </remarks>
        public bool SetPrincipalParameter<T>(T value)
        {
            foreach (var ps in Values)
            {
                if (ps.TrySetPrincipalParameter(value))
                    return true;
            }

            throw new CircuitException("No principal parameter found of type {1}".FormatString(typeof(T).Name));
        }

        /// <summary>
        /// Calls a parameter method with a specified name. If multiple methods exist,
        /// all of them will be called.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>The source object (can be used for chaining).</returns>
        public ParameterSetDictionary SetParameter(string name, IEqualityComparer<string> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<string>.Default;

            var isset = false;
            foreach (var ps in Values)
            {
                if (ps.TrySetParameter(name, comparer))
                    isset = true;
            }

            if (!isset)
                throw new CircuitException("No parameter with the name '{0}' found".FormatString(name));
            return this;
        }

        /// <summary>
        /// Sets all parameters with a specified name in any parameter set in the dictionary.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>The source object (can be used for chaining).</returns>
        public ParameterSetDictionary SetParameter<T>(string name, T value, IEqualityComparer<string> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<string>.Default;

            var isset = false;
            foreach (var ps in Values)
            {
                if (ps.TrySetParameter(name, value, comparer))
                    isset = true;
            }

            if (!isset)
                throw new CircuitException("No parameter with the name '{0}' found of type {1}".FormatString(name, typeof(T).Name));
            return this;
        }

        /// <summary>
        /// Gets the principal parameter from any parameter set in the dictionary.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <returns>
        /// The principal parameter of the specified type.
        /// </returns>
        public T GetPrincipalParameter<T>()
        {
            foreach (var ps in Values)
            {
                foreach (var member in Reflection.GetPrincipalMembers(ps))
                {
                    if (ParameterHelper.GetMember<T>(ps, member, out var p))
                        return p;
                }
            }

            throw new CircuitException("No principal parameter found of type {1}".FormatString(typeof(T).Name));
        }

        /// <summary>
        /// Gets a named parameter from any parameter set in the dictionary.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns></returns>
        public T GetParameter<T>(string name, IEqualityComparer<string> comparer = null)
        {
            foreach (var ps in Values)
            {
                foreach (var member in Reflection.GetNamedMembers(ps, name, comparer))
                {
                    if (ParameterHelper.GetMember<T>(ps, member, out var p))
                        return p;
                }
            }

            throw new CircuitException("No parameter with the name '{0}' found of type {1}".FormatString(name, typeof(T).Name));
        }

        /// <summary>
        /// Gets a setter for a parameter with a specified name in any parameter set in the dictionary.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>
        /// An action for setting the parameter with the specified type and name, or <c>null</c> if no parameter was found.
        /// </returns>
        public Action<T> CreateSetter<T>(string name, IEqualityComparer<string> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<string>.Default;

            foreach (var ps in Values)
            {
                var s = ps.CreateSetter<T>(name, comparer);
                if (s != null)
                    return s;
            }

            return null;
        }

        /// <summary>
        /// Gets a setter for a principal parameter in any parameter set in the dictionary.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <returns>
        /// An action for setting the principal parameter with the specified type and name, or <c>null</c> if no parameter was found.
        /// </returns>
        /// <remarks>
        /// Only the first encountered principal parameter will be set using the setter returned from this method.
        /// </remarks>
        public Action<T> CreateSetter<T>()
        {
            foreach (var ps in Values)
            {
                var s = ps.CreateSetter<T>();
                if (s != null)
                    return s;
            }

            return null;
        }

        /// <summary>
        /// Clone the dictionary.
        /// </summary>
        /// <returns></returns>
        public virtual ParameterSetDictionary Clone()
        {
            var clone = Activator.CreateInstance(GetType());
            Reflection.CopyPropertiesAndFields(this, clone);
            return (ParameterSetDictionary)clone;
        }

        /// <summary>
        /// Clone the object.
        /// </summary>
        /// <returns></returns>
        ICloneable ICloneable.Clone() => Clone();

        /// <summary>
        /// Copy all parameter sets.
        /// </summary>
        /// <param name="source">The source object.</param>
        public virtual void CopyFrom(ParameterSetDictionary source)
        {
            var d = source as ParameterSetDictionary ?? throw new CircuitException("Cannot copy, type mismatch");
            foreach (var ps in d.Values)
            {
                // If the parameter set doesn't exist, then we will simply clone it, else copy them
                if (!TryGetValue(ps.GetType(), out var myps))
                    Add(ps.Clone());
                else
                    Reflection.CopyPropertiesAndFields(ps, myps);
            }
        }

        /// <summary>
        /// Copy all properties from another object to this one.
        /// </summary>
        /// <param name="source">The source object.</param>
        void ICloneable.CopyFrom(ICloneable source) => CopyFrom((ParameterSetDictionary)source);
    }
}
