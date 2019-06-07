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
            if (member is PropertyInfo pi)
            {
                if (pi.PropertyType == typeof(T) && pi.CanWrite)
                {
                    pi.SetValue(source, value);
                    return true;
                }
            }
            else if (member is FieldInfo fi)
            {
                if (fi.FieldType == typeof(T))
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
                    if (paraminfo.Length == 1 && paraminfo[0].ParameterType == typeof(T))
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
            if (member is PropertyInfo pi)
            {
                if (pi.CanRead)
                {
                    value = (T)pi.GetValue(source);
                    return true;
                }
            }
            else if (member is FieldInfo fi)
            {
                value = (T)fi.GetValue(source);
                return true;
            }
            else if (member is MethodInfo mi)
            {
                // Methods
                var paraminfo = mi.GetParameters();
                if (paraminfo.Length == 0)
                {
                    value = (T)mi.Invoke(source, new object[] { });
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Create a setter for a member.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="member">The member information.</param>
        /// <returns>An action that sets the member of this object.</returns>
        /// <exception cref="ArgumentNullException">member</exception>
        public static Action<T> CreateSetterForMember<T>(object source, MemberInfo member)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));
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
        /// <param name="member">The member information.</param>
        /// <returns>A function that gets the member of this object.</returns>
        /// <exception cref="ArgumentNullException">member</exception>
        public static Func<T> CreateGetterForMember<T>(object source, MemberInfo member)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));

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
        /// <param name="method">The method information.</param>
        /// <returns>
        /// An action that calls the method for this instance.
        /// </returns>
        public static Action<T> CreateSetterForMethod<T>(object source, MethodInfo method)
        {
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
        /// <param name="method">The method information.</param>
        /// <returns>
        /// A function that calls the method for this instance.
        /// </returns>
        public static Func<T> CreateGetterForMethod<T>(object source, MethodInfo method)
        {
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
        /// <param name="property">The property information.</param>
        /// <returns>
        /// An action that sets the property value for this instance.
        /// </returns>
        public static Action<T> CreateSetterForProperty<T>(object source, PropertyInfo property)
        {
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
        /// <param name="property">The property information.</param>
        /// <returns>
        /// A function that gets the property value for this instance.
        /// </returns>
        public static Func<T> CreateGetterForProperty<T>(object source, PropertyInfo property)
        {
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
        /// <param name="field">The field information.</param>
        /// <returns>
        /// An action that sets the field value for this instance.
        /// </returns>
        public static Action<T> CreateSetterForField<T>(object source, FieldInfo field)
        {
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
        /// <param name="field">The field information.</param>
        /// <returns>
        /// A function that gets the field value for this instance.
        /// </returns>
        public static Func<T> CreateGetterForField<T>(object source, FieldInfo field)
        {
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
