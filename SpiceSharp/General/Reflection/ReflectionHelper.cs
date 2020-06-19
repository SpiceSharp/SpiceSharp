using SpiceSharp.Diagnostics;
using SpiceSharp.ParameterSets;
using SpiceSharp.Reflection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SpiceSharp
{
    /// <summary>
    /// Helper class for using reflection in combination with Spice#.
    /// </summary>
    public static class ReflectionHelper
    {
        private static IEqualityComparer<string> _comparer = EqualityComparer<string>.Default;
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

        #region Parameter helpers
        /// <summary>
        /// Sets the parameter.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns>The original object, can be used to chain.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="name"/> is <c>null</c>.</exception>
        /// <exception cref="ParameterNotFoundException">Thrown if the the parameter with the specified <paramref name="name"/> could not be found.</exception>
        public static void Set<P>(object source, string name, P value)
        {
            source.ThrowIfNull(nameof(source));
            var map = GetParameterMap(source.GetType());
            if (!map.TrySet(source, name, value))
                throw new ParameterNotFoundException(source, name, typeof(P));
        }

        /// <summary>
        /// Gets the parameter.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="name"/> is <c>null</c>.</exception>
        /// <exception cref="ParameterNotFoundException">Thrown if the the parameter with the specified <paramref name="name"/> could not be found.</exception>
        public static P Get<P>(object source, string name)
        {
            source.ThrowIfNull(nameof(source));
            var map = GetParameterMap(source.GetType());
            if (!map.TryGet(source, name, out P value))
                throw new ParameterNotFoundException(source, name, typeof(P));
            return value;
        }

        /// <summary>
        /// Tries to set the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the parameter was set; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="name"/> is <c>null</c>.</exception>
        public static bool TrySet<P>(object source, string name, P value)
        {
            source.ThrowIfNull(nameof(source));
            var map = GetParameterMap(source.GetType());
            return map.TrySet(source, name, value);
        }

        /// <summary>
        /// Tries to get the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the parameter is returned; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="name"/> is <c>null</c>.</exception>
        public static bool TryGet<P>(object source, string name, out P value)
        {
            source.ThrowIfNull(nameof(source));
            var map = GetParameterMap(source.GetType());
            return map.TryGet(source, name, out value);
        }

        /// <summary>
        /// Creates a getter for the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// The function that can return the value of the parameter, or <c>null</c> if the property doesn't exist.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="name"/> is <c>null</c>.</exception>
        public static Func<P> CreateGetter<P>(object source, string name)
        {
            source.ThrowIfNull(nameof(source));
            var map = GetParameterMap(source.GetType());
            return map.CreateGetter<P>(source, name);
        }

        /// <summary>
        /// Creates a setter for the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// The action that can set the value of the parameter, or <c>null</c> if the parameter doesn't exist.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="name"/> is <c>null</c>.</exception>
        public static Action<P> CreateSetter<P>(object source, string name)
        {
            source.ThrowIfNull(nameof(source));
            var map = GetParameterMap(source.GetType());
            return map.CreateSetter<P>(source, name);
        }
        #endregion

        #region General reflection helper methods
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

        /// <summary>
        /// Creates a setter for a member.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="member">The member reflection information.</param>
        /// <returns>An action that sets the member of this object.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="member"/> is <c>null</c>.</exception>
        public static Action<T> CreateSetterForMember<T>(object source, MemberInfo member)
        {
            return member.ThrowIfNull(nameof(member)) switch
            {
                MethodInfo mi => CreateSetterForMethod<T>(source, mi),
                PropertyInfo pi => CreateSetterForProperty<T>(source, pi),
                FieldInfo fi => CreateSetterForField<T>(source, fi),
                _ => null,
            };
        }

        /// <summary>
        /// Creates a getter for a member.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="member">The member reflection information.</param>
        /// <returns>A function that gets the member of this object.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="member"/> is <c>null</c>.</exception>
        public static Func<T> CreateGetterForMember<T>(object source, MemberInfo member)
        {
            return member.ThrowIfNull(nameof(member)) switch
            {
                MethodInfo mi => CreateGetterForMethod<T>(source, mi),
                PropertyInfo pi => CreateGetterForProperty<T>(source, pi),
                FieldInfo fi => CreateGetterForField<T>(source, fi),
                _ => null,
            };
        }

        /// <summary>
        /// Creates a setter for a method.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="method">The method reflection information.</param>
        /// <returns>
        /// An action that calls the method for this instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="method" /> is <c>null</c>.</exception>
        public static Action<T> CreateSetterForMethod<T>(object source, MethodInfo method)
        {
            method.ThrowIfNull(nameof(method));

            // Match the return type
            if (method.ReturnType != typeof(void))
                return null;

            // Get parameters
            var parameters = method.GetParameters();
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(T))
                return (Action<T>)method.CreateDelegate(typeof(Action<T>), source);

            // Could not turn it into a setter
            return null;
        }

        /// <summary>
        /// Creates a getter for a method.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="method">The method reflection information.</param>
        /// <returns>
        /// A function that calls the method for this instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="method" /> is <c>null</c>.</exception>
        public static Func<T> CreateGetterForMethod<T>(object source, MethodInfo method)
        {
            method.ThrowIfNull(nameof(method));

            // Match the return type
            if (method.ReturnType != typeof(T))
                return null;

            // Get parameters
            var parameters = method.GetParameters();
            if (parameters.Length > 0)
                return null;

            // Turn it into a getter
            return (Func<T>)method.CreateDelegate(typeof(Func<T>), source);
        }

        /// <summary>
        /// Creates a setter for a property.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="property">The property reflection information.</param>
        /// <returns>
        /// An action that sets the property value for this instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="property"/> is <c>null</c>.</exception>
        public static Action<T> CreateSetterForProperty<T>(object source, PropertyInfo property)
        {
            property.ThrowIfNull(nameof(property));

            // Double properties are supported
            if (property.PropertyType == typeof(T))
                return (Action<T>)property.GetSetMethod()?.CreateDelegate(typeof(Action<T>), source);

            // Could not turn it into a setter
            return null;
        }

        /// <summary>
        /// Creates a getter for a property.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="property">The property reflection information.</param>
        /// <returns>
        /// A function that gets the property value for this instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="property"/> is <c>null</c>.</exception>
        public static Func<T> CreateGetterForProperty<T>(object source, PropertyInfo property)
        {
            property.ThrowIfNull(nameof(property));

            // Double properties are supported
            if (property.PropertyType == typeof(T))
                return (Func<T>)property.GetGetMethod()?.CreateDelegate(typeof(Func<T>), source);

            // Could not turn it into a getter
            return null;
        }

        /// <summary>
        /// Creates a setter for a field.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="field">The field reflection information.</param>
        /// <returns>
        /// An action that sets the field value for this instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="field"/> is <c>null</c>.</exception>
        public static Action<T> CreateSetterForField<T>(object source, FieldInfo field)
        {
            field.ThrowIfNull(nameof(field));

            if (field.FieldType == typeof(T))
            {
                var constThis = Expression.Constant(source);
                var constField = Expression.Field(constThis, field);
                var paramValue = Expression.Parameter(typeof(T), "value");
                var assignField = Expression.Assign(constField, paramValue);
                return Expression.Lambda<Action<T>>(assignField, paramValue).Compile();
            }

            // Could not turn this into a setter
            return null;
        }

        /// <summary>
        /// Creates a getter for a field.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="field">The field reflection information.</param>
        /// <returns>
        /// A function that gets the field value for this instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="field"/> is <c>null</c>.</exception>
        public static Func<T> CreateGetterForField<T>(object source, FieldInfo field)
        {
            field.ThrowIfNull(nameof(field));

            if (field.FieldType == typeof(T))
            {
                var constThis = Expression.Constant(source);
                var constField = Expression.Field(constThis, field);
                var returnLabel = Expression.Label(typeof(T));
                return Expression.Lambda<Func<T>>(Expression.Label(returnLabel, constField)).Compile();
            }

            // Could not turn this into a getter
            return null;
        }
        #endregion
    }
}
