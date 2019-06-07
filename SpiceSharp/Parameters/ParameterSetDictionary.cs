using System;
using System.Collections.Generic;

namespace SpiceSharp
{
    /// <summary>
    /// A dictionary of <see cref="ParameterSet" />. Only one instance of each type is allowed.
    /// </summary>
    /// <seealso cref="TypeDictionary{ParameterSet}" />
    public class ParameterSetDictionary : TypeDictionary<ParameterSet>
    {
        /// <summary>
        /// Adds a parameter set to the dictionary.
        /// </summary>
        /// <param name="set">The parameter set.</param>
        public void Add(ParameterSet set)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));

            Add(set.GetType(), set);
        }

        /// <summary>
        /// Gets a parameter from any parameter set in the dictionary.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>
        /// The parameter of the specified type and with the specified name, or <c>null</c> if no parameter was found.
        /// </returns>
        public Parameter<T> GetParameter<T>(string name, IEqualityComparer<string> comparer = null) where T : struct
        {
            comparer = comparer ?? EqualityComparer<string>.Default;
            foreach (var ps in Values)
            {
                foreach (var member in ParameterHelper.GetNamedMembers(ps, name, comparer))
                {
                    if (Reflection.GetMember<Parameter<T>>(ps, member, out var p))
                        return p;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the principal parameter from any parameter set in the dictionary.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <returns>
        /// The principal parameter of the specified type, or <c>null</c> if no principal parameter was found.
        /// </returns>
        public Parameter<T> GetParameter<T>() where T : struct
        {
            foreach (var ps in Values)
            {
                foreach (var member in ParameterHelper.GetPrincipalMembers(ps))
                {
                    if (Reflection.GetMember<Parameter<T>>(ps, member, out var p))
                        return p;
                }
            }

            return null;
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
        public Action<T> CreateSetter<T>(string name, IEqualityComparer<string> comparer = null) where T : struct
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
        public Action<T> CreateSetter<T>() where T : struct
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
        /// Sets all parameters with a specified name in any parameter set in the dictionary.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>
        ///   <c>true</c> if one or more parameters were set, otherwise <c>false</c>.
        /// </returns>
        public bool SetParameter<T>(string name, T value, IEqualityComparer<string> comparer = null) where T : struct
        {
            comparer = comparer ?? EqualityComparer<string>.Default;

            var isset = false;
            foreach (var ps in Values)
            {
                if (ps.SetParameter(name, value, comparer))
                    isset = true;
            }

            return isset;
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
        public bool SetPrincipalParameter<T>(T value) where T : struct
        {
            foreach (var ps in Values)
            {
                if (ps.SetPrincipalParameter(value))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Calls a parameter method with a specified name. If multiple methods exist,
        /// all of them will be called.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <returns>
        ///   <c>true</c> if one or more methods were called; otherwise <c>false</c>.
        /// </returns>
        public bool SetParameter(string name)
        {
            var isset = false;
            foreach (var ps in Values)
            {
                if (ps.SetParameter(name))
                    isset = true;
            }

            return isset;
        }

        /// <summary>
        /// Calls a parameter method with a specified name. If multiple methods exist,
        /// all of them will be called.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>
        ///   <c>true</c> if one or more methods were called; otherwise <c>false</c>.
        /// </returns>
        public bool SetParameter(string name, IEqualityComparer<string> comparer)
        {
            comparer = comparer ?? EqualityComparer<string>.Default;

            var isset = false;
            foreach (var ps in Values)
            {
                if (ps.SetParameter(name, comparer))
                    isset = true;
            }

            return isset;
        }
    }
}
