using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace SpiceSharp.Reflection
{
    /// <summary>
    /// A static class that caches attributes for types.
    /// </summary>
    public static class AttributeCache
    {
        private static readonly Dictionary<Type, Attribute[]> _attributes = new Dictionary<Type, Attribute[]>();
        private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The attributes for a type.</returns>
        public static IEnumerable<Attribute> GetAttributes(Type type)
        {
            Attribute[] value;
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (!_attributes.TryGetValue(type, out value))
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        value = type.GetTypeInfo().GetCustomAttributes().ToArray();
                        _attributes.Add(type, value);
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
            foreach (var attribute in value)
                yield return attribute;
        }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <typeparam name="T">The base attribute type.</typeparam>
        /// <param name="type">The type.</param>
        /// <returns>The attributes for a type of the specified attribute type.</returns>
        public static IEnumerable<T> GetAttributes<T>(Type type) where T : Attribute
        {
            foreach (var attribute in GetAttributes(type))
            {
                if (attribute is T t)
                    yield return t;
            }
        }
    }
}
