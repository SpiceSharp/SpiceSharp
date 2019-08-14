using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using SpiceSharp.Attributes;

namespace SpiceSharp
{
    /// <summary>
    /// Helper methods for managing (named) parameters using reflection.
    /// </summary>
    /// <remarks>
    /// "Parameter" here is a general term for any class member that can be interacted with by the user in a meaningful way for Spice#. Most Spice 
    /// models have files with the sole purpose of mapping parameter names on actual variables. Spice# makes this easier by using reflection
    /// instead.
    /// </remarks>
    public static class ParameterHelper
    {
        private static readonly Dictionary<Type, MethodInfo> _setvalue = new Dictionary<Type, MethodInfo>();
        private static readonly ReaderWriterLockSlim _setLock = new ReaderWriterLockSlim();
        private static readonly Dictionary<Type, MethodInfo> _getvalue = new Dictionary<Type, MethodInfo>();
        private static readonly ReaderWriterLockSlim _getLock = new ReaderWriterLockSlim();

        /// <summary>
        /// This method will check whether or not a type is a parameter (implements <seealso cref="Parameter{T}"/>).
        /// </summary>
        /// <param name="type">The parameter to check.</param>
        /// <param name="generic">If not null, the parameter is also verified to have the generic parameter.</param>
        /// <returns></returns>
        public static Type IsParameter(Type type, Type generic = null)
        {
            while (type != null)
            {
                var info = type.GetTypeInfo();
                var cur = info.IsGenericType ? info.GetGenericTypeDefinition() : type;
                if (cur == typeof(Parameter<>))
                {
                    if (generic == null)
                        return type;
                    if (info.GenericTypeArguments[0].GetTypeInfo().IsAssignableFrom(generic))
                        return type;
                    return null;
                }
                type = info.BaseType;
            }
            return null;
        }

        /// <summary>
        /// Sets the value of the principal parameter.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if a principal parameter was set; otherwise <c>false</c>.
        /// </returns>
        public static bool TrySetPrincipalParameter<T>(this object source, T value)
        {
            foreach (var member in Reflection.GetPrincipalMembers(source))
            {
                if (SetMember(source, member, value))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the value of the principal parameters.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="value">The value.</param>
        /// <returns>The source object (can be used for chaining).</returns>
        public static object SetPrincipalParameter<T>(this object source, T value)
        {
            if (!TrySetPrincipalParameter(source, value))
                throw new CircuitException("No principal parameter found of type {0}".FormatString(typeof(T).Name));
            return source;
        }

        /// <summary>
        /// Tries setting a parameter with a specified name.
        /// If multiple parameters have the same name, they will all be set.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>
        ///   <c>true</c> if there was one or more parameters set; otherwise <c>false</c>.
        /// </returns>
        public static bool TrySetParameter<T>(this object source, string name, T value, IEqualityComparer<string> comparer = null)
        {
            // Set the property if any
            var isset = false;
            foreach (var member in Reflection.GetNamedMembers(source, name, comparer))
            {
                // Set the member
                if (SetMember(source, member, value))
                    isset = true;
            }
            return isset;
        }

        /// <summary>
        /// Sets a parameter with a specified name. If multiple parameters have the same name, they will all be set.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>The source object (can be used for chaining).</returns>
        public static object SetParameter<T>(this object source, string name, T value, IEqualityComparer<string> comparer = null)
        {
            if (!TrySetParameter(source, name, value, comparer))
                throw new CircuitException("No parameter with the name '{0}' found of type {1}".FormatString(name, typeof(T).Name));
            return source;
        }

        /// <summary>
        /// Calls a method by name without arguments.
        /// If multiple parameters by this name exist, all of them will be called.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="name">The name of the method.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>
        ///   <c>true</c> if there was one or more methods called; otherwise <c>false</c>.
        /// </returns>
        public static bool TrySetParameter(this object source, string name, IEqualityComparer<string> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<string>.Default;

            // Set the property if any
            var isset = false;
            foreach (var member in Reflection.GetNamedMembers(source, name, comparer))
            {
                // Set the member
                if (member is MethodInfo mi)
                {
                    var parameters = mi.GetParameters();
                    if (parameters.Length == 0)
                    {
                        mi.Invoke(source, null);
                        isset = true;
                    }
                }
            }
            return isset;
        }

        /// <summary>
        /// Calls a method by name without arguments.
        /// If multiple parameters by this name exist, all of them will be called.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="name">The name of the method.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>The source object (can be used for chaining).</returns>
        public static object SetParameter(this object source, string name, IEqualityComparer<string> comparer = null)
        {
            if (!TrySetParameter(source, name, comparer))
                throw new CircuitException("No parameter with the name '{0}' found".FormatString(name));
            return source;
        }

        /// <summary>
        /// Get a parameter value. Only the first found parameter with the specified name is returned.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>
        ///     <c>true</c> if the parameter exists and the value was read; otherwise <c>false</c>.
        /// </returns>
        public static bool TryGetParameter<T>(this object source, string name, out T value, IEqualityComparer<string> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<string>.Default;

            // Get the associated member
            var member = Reflection.GetNamedMembers(source, name, comparer).FirstOrDefault();
            if (member == null)
            {
                value = default;
                return false;
            }

            // Get the value of this member
            return GetMember(source, member, out value);
        }

        /// <summary>
        /// Get a parameter value. Only the first found parameter with the specified name is returned.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns></returns>
        public static T GetParameter<T>(this object source, string name, IEqualityComparer<string> comparer = null)
        {
            if (TryGetParameter<T>(source, name, out var result, comparer))
                return result;
            throw new CircuitException("Parameter '{0}' does not exist for type '{1}'".FormatString(name, typeof(T).Name));
        }

        /// <summary>
        /// Get a parameter value. Only the first principal parameter is returned.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static bool TryGetParameter<T>(this object source, out T value)
        {
            // Get the first principal parameter
            var member = Reflection.GetPrincipalMembers(source).FirstOrDefault();
            if (member == null)
            {
                value = default;
                return false;
            }

            // Extract
            return GetMember<T>(source, member, out value);
        }

        /// <summary>
        /// Get a parameter value. Only the first principal parameter is returned.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <returns></returns>
        public static T GetParameter<T>(this object source)
        {
            if (TryGetParameter<T>(source, out var result))
                return result;
            throw new CircuitException("Principal parameter does not exist for type '{1}'".FormatString(typeof(T).Name));
        }

        /// <summary>
        /// Sets the value of a member. If the member is a property that implements <seealso cref="Parameter{T}" />
        /// then the parameter value is set.
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
                // Find out if the property implements a parameter
                var parameterType = IsParameter(pi.PropertyType, typeof(T));
                if (parameterType != null)
                {
                    // This is a parameter! Instead of getting
                    _setLock.EnterUpgradeableReadLock();
                    try
                    {
                        if (!_setvalue.TryGetValue(parameterType, out var method))
                        {
                            method = parameterType.GetTypeInfo().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance).GetSetMethod();
                            _setLock.EnterWriteLock();
                            try
                            {
                                _setvalue.Add(parameterType, method);
                            }
                            finally
                            {
                                _setLock.ExitWriteLock();
                            }
                        }
                        method.Invoke(pi.GetValue(source), new object[] { value });
                        return true;
                    }
                    finally
                    {
                        _setLock.ExitUpgradeableReadLock();
                    }
                }
            }

            return Reflection.SetMember(source, member, value);
        }

        /// <summary>
        /// Gets a value of a member. If the member is a property that implements <seealso cref="Parameter{T}" />
        /// then the parameter value is returned.
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

            if (member is PropertyInfo pi)
            {
                // Find out if the property implements a parameter
                var parameterType = IsParameter(pi.PropertyType, typeof(T));
                if (parameterType != null)
                {
                    // This is a parameter! Instead of getting
                    _getLock.EnterUpgradeableReadLock();
                    try
                    {
                        if (!_getvalue.TryGetValue(parameterType, out var method))
                        {
                            method = parameterType.GetTypeInfo().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance).GetGetMethod();
                            _getLock.EnterWriteLock();
                            try
                            {
                                _getvalue.Add(parameterType, method);
                            }
                            finally
                            {
                                _getLock.ExitWriteLock();
                            }
                        }
                        value = (T)method.Invoke(pi.GetValue(source), null);
                        return true;
                    }
                    finally
                    {
                        _getLock.ExitUpgradeableReadLock();
                    }
                }
            }

            return Reflection.GetMember(source, member, out value);
        }

        /// <summary>
        /// Create a getter for the principal parameter.
        /// </summary>
        /// <remarks>
        /// The principal parameter is a member flagged with a <seealso cref="ParameterInfoAttribute" />
        /// where the principal flag is set. It can be used to flag a class member as the default
        /// parameter.
        /// </remarks>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <param name="source">The object with parameters.</param>
        /// <returns>A function returning the value of the principal parameter, or <c>null</c> if there is no principal parameter.</returns>
        public static Func<T> CreateGetter<T>(this object source)
        {
            foreach (var member in Reflection.GetPrincipalMembers(source))
            {
                var result = CreateGetterForMember<T>(source, member);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <summary>
        /// Returns a getter for the eligible first parameter with the specified name.
        /// </summary>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <param name="source">The object with parameters.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="comparer">The string comparer used for identifying the parameter name.</param>
        /// <returns>A function returning the value of the parameter, or <c>null</c> if there is no parameter with the specified name.</returns>
        public static Func<T> CreateGetter<T>(this object source, string name, IEqualityComparer<string> comparer = null)
        {
            foreach (var member in Reflection.GetNamedMembers(source, name, comparer))
            {
                var result = CreateGetterForMember<T>(source, member);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Create a setter for the principal parameter.
        /// </summary>
        /// <remarks>
        /// The principal parameter is a member flagged with a <seealso cref="ParameterInfoAttribute" />
        /// where the principal flag is set. It can be used to flag a class member as the default
        /// parameter.
        /// </remarks>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <param name="source">The object with parameters.</param>
        /// <returns>An action that can set the value of the principal parameter, or <c>null</c> if there is no principal parameter.</returns>
        public static Action<T> CreateSetter<T>(this object source)
        {
            foreach (var member in Reflection.GetPrincipalMembers(source))
            {
                var result = CreateSetterForMember<T>(source, member);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <summary>
        /// Returns a setter for the first eligible parameter with the specified name.
        /// </summary>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <param name="source">The object with parameters.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>A function returning the value of the parameter, or <c>null</c> if there is no parameter with the specified name.</returns>
        public static Action<T> CreateSetter<T>(this object source, string name, IEqualityComparer<string> comparer = null)
        {
            foreach (var member in Reflection.GetNamedMembers(source, name, comparer))
            {
                var result = CreateSetterForMember<T>(source, member);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Create a setter for a member.
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
                case PropertyInfo pi:
                    return CreateSetterForProperty<T>(source, pi);
                default:
                    return Reflection.CreateSetterForMember<T>(source, member);
            }
        }

        /// <summary>
        /// Create a getter for a member.
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
                case PropertyInfo pi:
                    return CreateGetterForProperty<T>(source, pi);
                default:
                    return Reflection.CreateGetterForMember<T>(source, member);
            }
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

            // Find out if the property implements a parameter
            var parameterType = IsParameter(property.PropertyType, typeof(T));
            if (parameterType != null)
            {
                // Build the expression (source).[member].Value = [parameter]
                var exprValue = Expression.Parameter(typeof(T), "value");
                var expression = Expression.Assign(
                        Expression.Property(
                            Expression.Property(
                                Expression.Constant(source), property), "Value"),
                        exprValue);
                return Expression.Lambda<Action<T>>(expression, exprValue).Compile();
            }

            return Reflection.CreateSetterForProperty<T>(source, property);
        }

        /// <summary>
        /// Creates a getter for a property. Parameters are accounted for.
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

            // Find out if the property implements a parameter
            var parameterType = IsParameter(property.PropertyType, typeof(T));
            if (parameterType != null)
            {
                // Build the expression (source).[member].Value = [parameter]
                var expression = Expression.Convert(
                    Expression.Property(
                        Expression.Property(
                            Expression.Constant(source), property), "Value"), typeof(T));
                return Expression.Lambda<Func<T>>(expression).Compile();
            }

            return Reflection.CreateGetterForProperty<T>(source, property);
        }
    }
}
