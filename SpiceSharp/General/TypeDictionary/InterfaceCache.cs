using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SpiceSharp.General
{
    /// <summary>
    /// A static class that caches interfaces of a specified type.
    /// </summary>
    public static class InterfaceCache
    {
        private static readonly ConcurrentDictionary<Type, Type[]> _interfaces = new ConcurrentDictionary<Type, Type[]>();

        /// <summary>
        /// Gets the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>An enumerable for the interfaces of the type.</returns>
        public static IEnumerable<Type> Get(Type type)
        {
            return _interfaces.GetOrAdd(type,
                t => t.GetTypeInfo().GetInterfaces().ToArray());
        }
    }
}
