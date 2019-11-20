using System;
using System.Collections.Generic;
using SpiceSharp.General;
using System.Reflection;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;
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
        /// Enumerates all parameters in the <see cref="IParameterSet"/>.
        /// </summary>
        /// <param name="ps">The parameter set.</param>
        /// <returns>The member descriptions.</returns>
        public static IEnumerable<MemberDescription> Parameters(IParameterSet ps)
        {
            ps.ThrowIfNull(nameof(ps));
            foreach (var md in GetMembers(ps.GetType()))
                yield return md;
        }

        /// <summary>
        /// Enumerates all parameters in the <see cref="IParameterSetDictionary"/>
        /// </summary>
        /// <param name="psd">The parameter set dictionary.</param>
        /// <returns>The member descriptions.</returns>
        public static IEnumerable<MemberDescription> Parameters(IParameterSetDictionary psd)
        {
            psd.ThrowIfNull(nameof(psd));
            foreach (var ps in psd.Values)
            {
                foreach (var md in Parameters(ps))
                    yield return md;
            }
        }

        /// <summary>
        /// Enumerates all parameters in the <see cref="IParameterized{T}"/>.
        /// </summary>
        /// <typeparam name="T">The base type.</typeparam>
        /// <param name="pz">The parameterized object.</param>
        /// <returns>The member descriptions.</returns>
        public static IEnumerable<MemberDescription> Parameters<T>(IParameterized<T> pz)
        {
            pz.ThrowIfNull(nameof(pz));
            foreach (var md in Parameters(pz.Parameters))
                yield return md;
        }

        /// <summary>
        /// Enumerates all parameters in the <see cref="IBehaviorContainer"/>.
        /// </summary>
        /// <param name="container">The behavior container.</param>
        /// <returns>The member descriptions.</returns>
        public static IEnumerable<MemberDescription> Parameters(IBehaviorContainer container)
        {
            container.ThrowIfNull(nameof(container));
            foreach (var behavior in container.Values)
            {
                foreach (var md in GetMembers(behavior.GetType()))
                    yield return md;
            }

            foreach (var md in Parameters(container.Parameters))
                yield return md;
        }

        /// <summary>
        /// Enumerates all parameters in the <see cref="ISimulation"/>.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <returns>The member descriptions.</returns>
        public static IEnumerable<MemberDescription> Parameters(ISimulation simulation)
        {
            simulation?.ThrowIfNull(nameof(simulation));
            foreach (var md in Parameters(simulation.Configurations))
                yield return md;
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
