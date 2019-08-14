using SpiceSharp.Attributes;
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
        /// Holds information about a specified member of the class and its attributes.
        /// </summary>
        public class CachedMemberInfo
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CachedMemberInfo"/> class.
            /// </summary>
            /// <param name="member"></param>
            /// <param name="attributes"></param>
            public CachedMemberInfo(MemberInfo member, IEnumerable<Attribute> attributes)
            {
                Member = member;
                Attributes = new List<Attribute>(attributes);
            }

            /// <summary>
            /// Gets the reference to the member.
            /// </summary>
            public MemberInfo Member { get; private set; }

            /// <summary>
            /// Gets the cached list of attributes for the member.
            /// </summary>
            public List<Attribute> Attributes { get; private set; }

            /// <summary>
            /// Convert to a string.
            /// </summary>
            public override string ToString()
            {
                if (Member == null)
                    return "null";
                string result = Member.Name;
                string[] names = Attributes.Where(m => m is ParameterNameAttribute).Select(m => ((ParameterNameAttribute)m).Name).ToArray();
                if (names.Length > 0)
                    result += " (" + string.Join(", ", names) + ")";
                return result;
            }
        }

        private static readonly Dictionary<Type, List<CachedMemberInfo>> _membersDict = new Dictionary<Type, List<CachedMemberInfo>>();
        private static readonly ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        /// <summary>
        /// Get all the members in the class.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <returns></returns>
        public static IEnumerable<CachedMemberInfo> GetMembers(object source)
        {
            source.ThrowIfNull(nameof(source));

            cacheLock.EnterUpgradeableReadLock();
            try
            {
                var type = source.GetType();
                if (!_membersDict.ContainsKey(type))
                {
                    var members = type
                        .GetTypeInfo()
                        .GetMembers(BindingFlags.Public | BindingFlags.Instance)
                        .Select(m => new CachedMemberInfo(m, m.GetCustomAttributes().ToList())).ToList();

                    cacheLock.EnterWriteLock();
                    try
                    {
                        if (!_membersDict.ContainsKey(type))
                            _membersDict.Add(type, members);
                        return _membersDict[type];
                    }
                    finally
                    {
                        cacheLock.ExitWriteLock();
                    }
                }

                return _membersDict[type];
            }
            finally
            {
                cacheLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Get all members with a specified name.
        /// </summary>
        /// <remarks>
        /// You can specify a parameter name using the <seealso cref="ParameterNameAttribute" /> attribute.</remarks>
        /// <param name="source">The source object.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns></returns>
        public static IEnumerable<MemberInfo> GetNamedMembers(object source, string name, IEqualityComparer<string> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<string>.Default;
            return GetMembers(source)
                .Where(m => m.Attributes.Any(r => r is ParameterNameAttribute p && comparer.Equals(p.Name, name)))
                .Select(m => m.Member);
        }

        /// <summary>
        /// Get all principal members.
        /// </summary>
        /// <remarks>
        /// You can specify a parameter as principal using the <seealso cref="ParameterInfoAttribute" /> attribute, using the "IsPrincipal" flag.
        /// </remarks>
        /// <param name="source">The source object.</param>
        /// <returns></returns>
        public static IEnumerable<MemberInfo> GetPrincipalMembers(object source)
        {
            return GetMembers(source)
                .Where(m => m.Attributes.Any(r => r is ParameterInfoAttribute p && p.IsPrincipal))
                .Select(m => m.Member);
        }

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
                if (pi.CanWrite && pi.PropertyType.GetTypeInfo().IsAssignableFrom(typeof(T)))
                {
                    pi.SetValue(source, value);
                    return true;
                }
            }
            else if (member is FieldInfo fi)
            {
                if (fi.FieldType.GetTypeInfo().IsAssignableFrom(typeof(T)))
                {
                    fi.SetValue(source, value);
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
                throw new ArgumentException(@"Source and target are not of the same type.");

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
        /// Create a getter for a field.
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
    }
}
