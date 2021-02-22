using SpiceSharp.Diagnostics;
using SpiceSharp.ParameterSets;
using SpiceSharp.Reflection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SpiceSharp.Attributes;

namespace SpiceSharp
{
    /// <summary>
    /// Helper class for using reflection in combination with Spice#.
    /// </summary>
    public static class ReflectionHelper
    {
        private static IEqualityComparer<string> _comparer = Constants.DefaultComparer;
        private static readonly ConcurrentDictionary<Type, ParameterMap> _parameterMapDict = new ConcurrentDictionary<Type, ParameterMap>();

        /// <summary>
        /// Gets or sets the default comparer used when creating a parameter mapping.
        /// </summary>
        /// <value>
        /// The default comparer used.
        /// </value>
        public static IEqualityComparer<string> Comparer
        {
            get => _comparer;
            set
            {
                var newComparer = value ?? EqualityComparer<string>.Default;
                if (value != _comparer)
                {
                    _comparer = newComparer;
                    foreach (var map in _parameterMapDict.Values)
                        map.Remap(_comparer);
                }
            }
        }

        /// <summary>
        /// Gets the parameter map of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The parameter map.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is <c>null</c>.</exception>
        public static ParameterMap GetParameterMap(Type type)
        {
            type.ThrowIfNull(nameof(type));
            return _parameterMapDict.GetOrAdd(type, t =>
            {
                return new ParameterMap(type, Comparer);
            });
        }

        /// <summary>
        /// Copies all properties and fields from a source object to a destination object.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="destination">The destination object</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="destination" /> does not have the same type as <paramref name="source" />.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source" /> or <paramref name="destination" /> is <c>null</c>.</exception>
        /// <remarks>
        /// This method heavily uses reflection to find valid properties and methods. It supports properties and fields
        /// of types <see cref="double" />, <see cref="int" />, <see cref="string" />, <see cref="bool" /> and
        /// <see cref="ICloneable" />.
        /// </remarks>
        public static void CopyPropertiesAndFields(object source, object destination)
        {
            source.ThrowIfNull(nameof(source));
            destination.ThrowIfNull(nameof(destination));
            if (source.GetType() != destination.GetType())
                throw new ArgumentException(Properties.Resources.Reflection_NotMatchingType, nameof(destination));

            var members = source.GetType().GetTypeInfo().GetMembers(BindingFlags.Instance | BindingFlags.Public);
            foreach (var member in members)
            {
                if (member is PropertyInfo pi)
                {
                    if (pi.GetCustomAttribute(typeof(DerivedPropertyAttribute)) != null)
                    {
                        // skip properties with DerivedPropertyAttribute because their value will be set elsewhere
                        continue;
                    }

                    if (pi.CanWrite && pi.CanRead)
                    {
                        var value = pi.GetValue(source);
                        if (value is ICloneable cloneable)
                            pi.SetValue(destination, cloneable.Clone());
                        else
                            pi.SetValue(destination, value);
                    }
                    else if (pi.CanRead)
                    {
                        // We can't write ourself, but maybe we can just copy
                        if (pi.PropertyType.GetTypeInfo().GetInterfaces().Contains(typeof(ICloneable)))
                        {
                            var target = (ICloneable)pi.GetValue(destination);
                            var from = (ICloneable)pi.GetValue(source);
                            if (target != null && from != null)
                                target.CopyFrom(from);
                        }
                    }
                }
                else if (member is FieldInfo fi)
                {
                    var value = fi.GetValue(source);
                    if (value is ICloneable cloneable)
                        fi.SetValue(destination, cloneable.Clone());
                    else
                        fi.SetValue(destination, value);
                }
            }
        }
    }
}
