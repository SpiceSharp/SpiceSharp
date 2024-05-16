using SpiceSharp.Attributes;
using SpiceSharp.Components;
using SpiceSharp.ParameterSets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SpiceSharp.Documentation
{
    /// <summary>
    /// A helper class that helps listing documentation at runtime.
    /// </summary>
    public static class Documentation
    {
        /// <summary>
        /// Gets all the members that are defined on a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>All the members on a type.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is <c>null</c>.</exception>
        public static IEnumerable<MemberDocumentation> GetMembers(Type type)
        {
            var info = type.ThrowIfNull(nameof(type)).GetTypeInfo();
            foreach (var member in info.GetMembers(BindingFlags.Instance | BindingFlags.Public))
            {
                var mt = member.MemberType;

                // Filter by type
                if (mt != MemberTypes.Property &&
                    mt != MemberTypes.Field &&
                    mt != MemberTypes.Method)
                    continue;

                // Make sure it can be named
                if (!member.GetCustomAttributes<ParameterNameAttribute>().Any())
                    continue;

                // Create documentation
                yield return new MemberDocumentation(member);
            }
        }

        /// <summary>
        /// Enumerates all pins of a component type.
        /// </summary>
        /// <param name="type">The component type.</param>
        /// <returns>The pin names.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> Pins(Type type)
        {
            type.ThrowIfNull(nameof(type));
            var pinAttributes = type.GetCustomAttributes<PinAttribute>().ToArray();
            string[] pins = new string[pinAttributes.Length];
            foreach (var attribute in pinAttributes)
                pins[attribute.Index] = attribute.Name;
            return pins;
        }

        /// <summary>
        /// Enumerates all pins of a component type.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <returns>The pin names.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="component"/> is <c>null</c>.</exception>
        public static IEnumerable<string> Pins(this IComponent component)
            => Pins(component?.GetType());

        /// <summary>
        /// Enumerates all the named members.
        /// </summary>
        /// <param name="parameterized">The parameterized object.</param>
        /// <returns>
        /// The named parameters.
        /// </returns>
        public static IEnumerable<MemberDocumentation> Parameters(this IParameterSetCollection parameterized)
        {
            foreach (var ps in parameterized.ParameterSets)
            {
                // Allow recursive parameterized objects
                if (ps is IParameterSetCollection child)
                {
                    foreach (var md in Parameters(child))
                        yield return md;
                }

                // Show the parameters in the parameter set
                foreach (var md in Parameters(ps))
                    yield return md;
            }
        }

        /// <summary>
        /// Enumerates all the named parameters and properties of an <see cref="IParameterSet"/>.
        /// </summary>
        /// <param name="parameters">The parameter set.</param>
        /// <returns>
        /// All the named parameters.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="parameters"/> is <c>null</c>.</exception>
        public static IEnumerable<MemberDocumentation> Parameters(this IParameterSet parameters)
            => GetMembers(parameters?.GetType());

        /// <summary>
        /// Creates a dictionary of all properties and their values on a parameter set.
        /// </summary>
        /// <typeparam name="T">The parameter value type.</typeparam>
        /// <param name="parameterSetCollection">The parameter set.</param>
        /// <param name="givenOnly">If <c>true</c>, only parameters that were set/given are returned and parameters that are left to their default/nonsense value are skipped. <c>true</c> by default.</param>
        /// <returns>
        /// A read-only dictionary for all members and their values.
        /// </returns>
        public static IReadOnlyDictionary<MemberDocumentation, T> ParameterValues<T>(this IParameterSetCollection parameterSetCollection, bool givenOnly = true)
        {
            var result = new Dictionary<MemberDocumentation, T>();
            foreach (var ps in parameterSetCollection.ParameterSets)
            {
                var n = ParameterValues<T>(ps, givenOnly);
                foreach (var pair in n)
                    result.Add(pair.Key, pair.Value);
            }
            return new ReadOnlyDictionary<MemberDocumentation, T>(result);
        }

        /// <summary>
        /// Creates a dictionary of all properties and their values on a parameter set.
        /// </summary>
        /// <typeparam name="T">The parameter value type.</typeparam>
        /// <param name="parameterSet">The parameter set.</param>
        /// <param name="givenOnly">If <c>true</c>, only parameters that were set/given are returned and parameters that are left to their default/nonsense value are skipped. <c>true</c> by default.</param>
        /// <returns>
        /// A read-only dictionary for all members and their values.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="parameterSet"/> is <c>null</c>.</exception>
        public static IReadOnlyDictionary<MemberDocumentation, T> ParameterValues<T>(this IParameterSet parameterSet, bool givenOnly = true)
        {
            parameterSet.ThrowIfNull(nameof(parameterSet));
            var result = new Dictionary<MemberDocumentation, T>();
            foreach (var member in Parameters(parameterSet))
            {
                if (member.Names == null || member.Names.Count == 0)
                    continue;
                if (givenOnly)
                {
                    if (parameterSet.TryGetProperty<GivenParameter<T>>(member.Names[0], out var gp))
                    {
                        if (gp.Given)
                            result.Add(member, gp.Value);
                        continue;
                    }
                }
                if (parameterSet.TryGetProperty<T>(member.Names[0], out var value))
                    result.Add(member, value);
            }
            return new ReadOnlyDictionary<MemberDocumentation, T>(result);
        }

        /// <summary>
        /// Creates a (long) string of NAME=VALUE segments separated by a space. The first name of each parameter is used.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="parameterValues">The parameter values.</param>
        /// <returns>
        /// The string representation for all parameters and their values.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="parameterValues"/> is <c>null</c>.</exception>
        public static string AsString<T>(this IReadOnlyDictionary<MemberDocumentation, T> parameterValues)
        {
            parameterValues.ThrowIfNull(nameof(parameterValues));
            var sb = new StringBuilder(parameterValues.Count * 10);
            bool first = true;
            foreach (var value in parameterValues.Where(p => p.Key.IsParameter && p.Key.IsProperty))
            {
                if (first)
                    first = false;
                else
                    sb.Append(' ');
                sb.Append("{0}={1}".FormatString(value.Key.Names[0] ?? "?", value.Value));
            }
            return sb.ToString();
        }
    }
}
