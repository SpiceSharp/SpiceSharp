using SpiceSharp.Attributes;
using SpiceSharp.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace SpiceSharp
{
    /// <summary>
    /// Helper class for using reflection in Spice#.
    /// </summary>
    public static class Reflection
    {
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

        private static IEqualityComparer<string> _comparer = EqualityComparer<string>.Default;
        private static readonly Dictionary<Type, ParameterMap> _parameterMapDict = new Dictionary<Type, ParameterMap>();
        private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        /// <summary>
        /// Gets the members for a specific type.
        /// </summary>
        /// <param name="type">The member type.</param>
        /// <returns>
        /// An enumeration of all members of the type.
        /// </returns>
        public static IEnumerable<MemberDescription> GetMembers(Type type)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (!_parameterMapDict.TryGetValue(type, out var result))
                {
                    result = new ParameterMap(type, Comparer);
                    _lock.EnterWriteLock();
                    try
                    {
                        _parameterMapDict.Add(type, result);
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }
                return result.Members;
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Gets the member for a specific type with the specified name and target type.
        /// </summary>
        /// <param name="type">The type that should be searched for the parameter.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>The member description.</returns>
        public static MemberDescription GetMember(Type type, string name)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (!_parameterMapDict.TryGetValue(type, out var result))
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        result = new ParameterMap(type, Comparer);
                        _parameterMapDict.Add(type, result);
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }
                return result.Get(name);
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
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
        public static void Set<P>(object source, string name, P value)
        {
            var desc = GetMember(source.GetType(), name);
            if (desc == null || !desc.TrySet(source, value))
                throw new ParameterNotFoundException(name, source);
        }

        /// <summary>
        /// Calls the method with the specified name (tagged with <see cref="ParameterNameAttribute"/>).
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="name">The name of the parameter method.</param>
        /// <returns>The original object, can be used to chain.</returns>
        public static void Set(object source, string name)
        {
            var desc = GetMember(source.GetType(), name);
            if (desc == null || !(desc.Member is MethodInfo mi) || mi.GetParameters().Length > 0)
                throw new ParameterNotFoundException(name, source);
            mi.Invoke(source, null);
        }

        /// <summary>
        /// Gets the parameter.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public static P Get<P>(object source, string name)
        {
            var desc = GetMember(source.GetType(), name);
            if (desc != null && desc.TryGet(source, out P value))
                return value;
            throw new ParameterNotFoundException(name, source);
        }

        /// <summary>
        /// Tries to set the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static bool TrySet<P>(object source, string name, P value)
        {
            var desc = GetMember(source.GetType(), name);
            return desc != null && desc.TrySet(source, value);
        }

        /// <summary>
        /// Tries calling a method with the specified name (tagged with the <see cref="ParameterNameAttribute"/>).
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        ///     <c>true</c> if the method was called; otherwise <c>false</c>.
        /// </returns>
        public static bool TrySet(object source, string name)
        {
            var desc = GetMember(source.GetType(), name);
            if (desc == null || !(desc.Member is MethodInfo mi) || mi.GetParameters().Length != 0)
                return false;
            mi.Invoke(source, new object[] { });
            return true;
        }

        /// <summary>
        /// Tries to get the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///     <c>true</c> if the parameter is returned; otherwise <c>false</c>.
        /// </returns>
        public static bool TryGet<P>(object source, string name, out P value)
        {
            var desc = GetMember(source.GetType(), name);
            if (desc == null)
            {
                value = default;
                return false;
            }
            return desc.TryGet(source, out value);
        }

        /// <summary>
        /// Creates a getter for the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// The function that can return the value of the parameter.
        /// </returns>
        public static Func<P> CreateGetter<P>(object source, string name)
        {
            var desc = GetMember(source.GetType(), name);
            return desc?.CreateGetter<P>(source);
        }

        /// <summary>
        /// Creates a setter for the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// The action that cna set the value of the parameter.
        /// </returns>
        public static Action<P> CreateSetter<P>(object source, string name)
        {
            var desc = GetMember(source.GetType(), name);
            return desc?.CreateSetter<P>(source);
        }
        #endregion

        #region General reflection helper methods
        /// <summary>
        /// Sets the value of a member.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="member">The member information.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the member was set; otherwise <c>false</c>.
        /// </returns>
        public static bool SetMember<T>(object source, MemberInfo member, T value)
        {
            member.ThrowIfNull(nameof(member));

            if (member is PropertyInfo pi)
            {
                var info = pi.PropertyType.GetTypeInfo();
                if (pi.CanWrite && info.IsAssignableFrom(typeof(T)))
                {
                    pi.SetValue(source, value);
                    return true;
                }
                if (pi.CanRead && info.IsAssignableFrom(typeof(Parameter<T>)))
                {
                    var p = (Parameter<T>)pi.GetValue(source);
                    p.Value = value;
                    return true;
                }
            }
            else if (member is FieldInfo fi)
            {
                var info = fi.FieldType.GetTypeInfo();
                if (info.IsAssignableFrom(typeof(T)))
                {
                    fi.SetValue(source, value);
                    return true;
                }
                if (info.IsAssignableFrom(typeof(Parameter<T>)))
                {
                    var p = (Parameter<T>)fi.GetValue(source);
                    p.Value = value;
                    return true;
                }
            }
            else if (member is MethodInfo mi)
            {
                // Methods
                if (mi.ReturnType == typeof(void))
                {
                    var paraminfo = mi.GetParameters();
                    if (paraminfo.Length == 1 && paraminfo[0].ParameterType.GetTypeInfo().IsAssignableFrom(typeof(T)))
                    {
                        mi.Invoke(source, new object[] { value });
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a value of a member.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="member">The member information.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///     <c>true</c> if the member was read; otherwise <c>false</c>.
        /// </returns>
        public static bool GetMember<T>(object source, MemberInfo member, out T value)
        {
            member.ThrowIfNull(nameof(member));

            var info = typeof(T).GetTypeInfo();
            if (member is PropertyInfo pi)
            {
                if (pi.CanRead && info.IsAssignableFrom(pi.PropertyType))
                {
                    value = (T)pi.GetValue(source);
                    return true;
                }
            }
            else if (member is FieldInfo fi)
            {
                if (info.IsAssignableFrom(fi.FieldType))
                {
                    value = (T)fi.GetValue(source);
                    return true;
                }
            }
            else if (member is MethodInfo mi)
            {
                if (info.IsAssignableFrom(mi.ReturnType))
                {
                    // Methods
                    var paraminfo = mi.GetParameters();
                    if (paraminfo.Length == 0)
                    {
                        value = (T)mi.Invoke(source, new object[] { });
                        return true;
                    }
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Copies all properties and fields from a source object to a destination object.
        /// </summary>
        /// <remarks>
        /// This method heavily uses reflection to find valid properties and methods. It supports properties and fields
        /// of types <see cref="double"/>, <see cref="int"/>, <see cref="string"/>, <see cref="bool"/> and
        /// <see cref="ICloneable"/>.
        /// </remarks>
        /// <param name="source">The source object.</param>
        /// <param name="destination">The destination object</param>
        public static void CopyPropertiesAndFields(object source, object destination)
        {
            source.ThrowIfNull(nameof(source));
            destination.ThrowIfNull(nameof(destination));
            if (source.GetType() != destination.GetType())
                throw new UnexpectedTypeMismatch(destination.GetType(), source.GetType());

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

                    if (pi.CanWrite)
                    {
                        if (pi.PropertyType == typeof(double))
                            pi.SetValue(destination, (double)pi.GetValue(source));
                        else if (pi.PropertyType == typeof(int))
                            pi.SetValue(destination, (int)pi.GetValue(source));
                        else if (pi.PropertyType == typeof(string))
                            pi.SetValue(destination, (string)pi.GetValue(source));
                        else if (pi.PropertyType == typeof(bool))
                            pi.SetValue(destination, (bool)pi.GetValue(source));
                        else if (pi.PropertyType.GetTypeInfo().GetInterfaces().Contains(typeof(ICloneable)))
                        {
                            var target = (ICloneable)pi.GetValue(destination);
                            var from = (ICloneable)pi.GetValue(source);
                            if (target != null && from != null)
                                target.CopyFrom(from);
                            else
                                pi.SetValue(destination, from?.Clone());
                        }
                    }
                    else
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
                    if (fi.FieldType == typeof(double))
                        fi.SetValue(destination, (double)fi.GetValue(source));
                    else if (fi.FieldType == typeof(int))
                        fi.SetValue(destination, (int)fi.GetValue(source));
                    else if (fi.FieldType == typeof(string))
                        fi.SetValue(destination, (string)fi.GetValue(source));
                    else if (fi.FieldType == typeof(bool))
                        fi.SetValue(destination, (bool)fi.GetValue(source));
                    else if (fi.FieldType.GetTypeInfo().GetInterfaces().Contains(typeof(ICloneable)))
                    {
                        var target = (ICloneable)fi.GetValue(destination);
                        var from = (ICloneable)fi.GetValue(source);
                        if (target != null && from != null)
                            target.CopyFrom(from);
                        else
                            fi.SetValue(destination, from?.Clone());
                    }
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
        public static Action<T> CreateSetterForMember<T>(object source, MemberInfo member)
        {
            member.ThrowIfNull(nameof(member));

            switch (member)
            {
                case MethodInfo mi:
                    return CreateSetterForMethod<T>(source, mi);
                case PropertyInfo pi:
                    return CreateSetterForProperty<T>(source, pi);
                case FieldInfo fi:
                    return CreateSetterForField<T>(source, fi);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Creates a getter for a member.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="member">The member reflection information.</param>
        /// <returns>A function that gets the member of this object.</returns>
        public static Func<T> CreateGetterForMember<T>(object source, MemberInfo member)
        {
            member.ThrowIfNull(nameof(member));

            switch (member)
            {
                case MethodInfo mi:
                    return CreateGetterForMethod<T>(source, mi);
                case PropertyInfo pi:
                    return CreateGetterForProperty<T>(source, pi);
                case FieldInfo fi:
                    return CreateGetterForField<T>(source, fi);
                default:
                    return null;
            }
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
