using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace SpiceSharp.General
{
    /// <summary>
    /// A static class that caches inherited objects of a specified type.
    /// </summary>
    public static class InheritanceCache
    {
        private static readonly ConcurrentDictionary<Type, Type[]> _interfaces = new();

        /// <summary>
        /// Gets the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>An enumerable for the interfaces of the type.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is <c>null</c>.</exception>
        public static IEnumerable<Type> Get(Type type)
        {
            type.ThrowIfNull(nameof(type));
            return _interfaces.GetOrAdd(type, t =>
            {
                var result = new List<Type>(8);
                t = t.GetTypeInfo().BaseType;
                while (t != null && t != typeof(object))
                {
                    result.Add(t);
                    t = t.GetTypeInfo().BaseType;
                }
                return result.ToArray();
            });
        }
    }
}
