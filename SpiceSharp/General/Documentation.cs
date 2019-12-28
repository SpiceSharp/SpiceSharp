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
                if (md.Names != null && md.Names.Length > 0)
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
    }
}
