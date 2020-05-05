using System;
using System.Collections.Generic;
using SpiceSharp.Attributes;
using System.Reflection;
using SpiceSharp.Components;
using SpiceSharp.ParameterSets;
using System.Linq;

namespace SpiceSharp.Reflection
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
        public static IEnumerable<MemberDescription> GetMembers(Type type)
        {
            var info = type.ThrowIfNull(nameof(type)).GetTypeInfo();
            foreach (var member in info.GetMembers(BindingFlags.Instance | BindingFlags.Public))
            {
                var mt = member.MemberType;
                if (mt != MemberTypes.Property &&
                    mt != MemberTypes.Field &&
                    mt != MemberTypes.Method)
                    continue;
                var md = new MemberDescription(member);
                if (md.Names != null && md.Names.Count > 0)
                    yield return md;
            }
        }

        /// <summary>
        /// Enumerates all pins of a component type.
        /// </summary>
        /// <param name="type">The component type.</param>
        /// <returns>The pin names.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is <c>null</c>.</exception>
        public static IEnumerable<string> Pins(Type type)
        {
            type.ThrowIfNull(nameof(type));
            var attributes = AttributeCache
                .GetAttributes(type)
                .Where(a => a is PinAttribute)
                .Cast<PinAttribute>()
                .ToArray();

            // Store the pin names in order
            var pins = new string[attributes.Length];
            foreach (var attr in attributes)
                pins[attr.Index] = attr.Name;
            return pins;
        }

        /// <summary>
        /// Enumerates all pins of a component type.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <returns>The pin names.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="component"/> is <c>null</c>.</exception>
        public static IEnumerable<string> Pins(IComponent component) 
            => Pins(component.ThrowIfNull(nameof(component)).GetType());

        /// <summary>
        /// Enumerates all the named members.
        /// </summary>
        /// <param name="parameterized">The parameterized object.</param>
        /// <returns>
        /// The named parameters.
        /// </returns>
        public static IEnumerable<MemberDescription> Parameters(IParameterSetCollection parameterized)
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
        public static IEnumerable<MemberDescription> Parameters(IParameterSet parameters)
            => Parameters(parameters.ThrowIfNull(nameof(parameters)).GetType());

        /// <summary>
        /// Enumerates all the named members (all parameters and properties with the <see cref="ParameterNameAttribute"/> attribute) on a type.
        /// </summary>
        /// <param name="type">The type containing the parameters.</param>
        /// <returns>
        /// The named parameters.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is <c>null</c>.</exception>
        public static IEnumerable<MemberDescription> Parameters(Type type)
            => ReflectionHelper.GetParameterMap(type.ThrowIfNull(nameof(type))).Members.Where(p => p.Names.Count > 0);
    }
}
