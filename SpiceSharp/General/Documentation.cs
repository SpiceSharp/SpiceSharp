using System;
using System.Collections.Generic;
using SpiceSharp.General;
using System.Reflection;
using SpiceSharp.Components;
using SpiceSharp.Attributes;
using System.Linq;

namespace SpiceSharp
{
    /// <summary>
    /// A helper class that helps listing documentation.
    /// </summary>
    public static class Documentation
    {
        /// <summary>
        /// Gets all members.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
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
        /// Enumerates all pins of the entity.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <returns>The pin names.</returns>
        public static IEnumerable<string> Pins(Component component)
        {
            var info = component.GetType().GetTypeInfo();
            var attributes = info.GetCustomAttributes<PinAttribute>().ToArray();
            var pins = new string[attributes.Length];
            foreach (var attr in attributes)
                pins[attr.Index] = attr.Name;
            return pins;
        }

        /// <summary>
        /// Enumerates all the named members.
        /// </summary>
        /// <param name="parameterized">The parameterized object.</param>
        /// <returns>
        /// The named parameters.
        /// </returns>
        public static IEnumerable<MemberDescription> Parameters(IParameterized parameterized)
        {
            foreach (var ps in parameterized.ParameterSets)
            {
                // Allow recursive parameterized objects
                if (ps is IParameterized child)
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
        /// Enumerates all the named members.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// The named parameters.
        /// </returns>
        public static IEnumerable<MemberDescription> Parameters(IParameterSet parameters)
            => Reflection.GetMembers(parameters.GetType()).Where(p => p.Names.Count > 0);
    }
}
