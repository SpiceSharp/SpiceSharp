using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// A template for a class with named parameters. Named parameters are parameters one or more <see cref="ParameterNameAttribute" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.Parameterized" />
    public abstract class NamedParameterized : Parameterized
    {
        /// <summary>
        /// Returns all members tagged with a certain name.
        /// </summary>
        /// <param name="name">The name of the member.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>An enumerable with all member info containing at least one name.</returns>
        protected IEnumerable<MemberInfo> Named(string name, IEqualityComparer<string> comparer = null)
        {
            // Make sure we always have a default
            comparer = comparer ?? EqualityComparer<string>.Default;
            return Members.Where(m =>
                {
                    return m.GetCustomAttributes<ParameterNameAttribute>().Any(pn => comparer.Equals(pn.Name, name));
                });
        }

        /// <summary>
        /// Returns the first principal member of the class.
        /// </summary>
        protected MemberInfo Principal
        {
            get
            {
                return Members.FirstOrDefault(m =>
                    {
                        return m.GetCustomAttributes<ParameterInfoAttribute>().Any(pi => pi.IsPrincipal);
                    });
            }
        }

        /// <summary>
        /// Create setters for all named parameters.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>A dictionary with all setters by their name.</returns>
        public Dictionary<string, Action<T>> CreateSetters<T>(IEqualityComparer<string> comparer = null) where T : struct
        {
            var result = new Dictionary<string, Action<T>>(comparer);
            foreach (var member in Members)
            {
                if (!member.IsDefined(typeof(ParameterNameAttribute)))
                    continue;

                // Create a setter
                var setter = CreateSetter<T>(member);
                if (setter != null)
                {
                    foreach (var pn in member.GetCustomAttributes<ParameterNameAttribute>())
                        result[pn.Name] = setter;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a setter for the first found parameter with the specified name.
        /// </summary>
        /// <remarks>
        /// This method will only consider parameters that can be written to. Read-only parameters
        /// are ignored.
        /// </remarks>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>An action for setting the value of the parameter.</returns>
        public Action<T> CreateSetter<T>(string name, IEqualityComparer<string> comparer = null) where T : struct
        {
            foreach (var member in Named(name, comparer))
            {
                var result = CreateSetter<T>(member);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Returns a setter for the principal parameter.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <returns>An action for setting the value of the parameter.</returns>
        public Action<T> CreateSetter<T>() where T : struct
        {
            var p = Principal;
            if (p != null)
                return CreateSetter<T>(p);
            return null;
        }

        /// <summary>
        /// Returns a getter for the first parameter with a name that can be read.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="name">The name the parameter.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>A function returning the value of the parameter.</returns>
        public Func<T> CreateGetter<T>(string name, IEqualityComparer<string> comparer = null) where T : struct
        {
            foreach (var member in Named(name, comparer))
            {
                var result = CreateGetter<T>(member);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Returns a getter for the principal parameter.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <returns>A function returning the value of the parameter.</returns>
        public Func<T> CreateGetter<T>() where T : struct
        {
            var p = Principal;
            if (p != null)
                return CreateGetter<T>(p);
            return null;
        }

        /// <summary>
        /// Create getters for all named parameters.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>A dictionary with all getters by their name.</returns>
        public Dictionary<string, Func<T>> CreateGetters<T>(IEqualityComparer<string> comparer = null) where T : struct
        {
            var result = new Dictionary<string, Func<T>>(comparer);
            foreach (var member in Members)
            {
                if (!member.IsDefined(typeof(ParameterNameAttribute)))
                    continue;

                // Create a getter
                var getter = CreateGetter<T>(member);
                if (getter != null)
                {
                    foreach (var pn in member.GetCustomAttributes<ParameterNameAttribute>())
                        result[pn.Name] = getter;
                }
            }

            return result;
        }
    }
}
