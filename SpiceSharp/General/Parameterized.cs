using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace SpiceSharp
{
    /// <summary>
    /// A template class with methods to address properties and fields using reflection.
    /// </summary>
    public abstract class Parameterized
    {
        private static readonly Dictionary<Type, List<Tuple<MemberInfo, List<Attribute>>>> _membersDict = new Dictionary<Type, List<Tuple<MemberInfo, List<Attribute>>>>();
        private static readonly ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);


        /// <summary>
        /// Gets all members in the class.
        /// </summary>
        /// <value>
        /// The members.
        /// </value>
        protected IEnumerable<MemberInfo> Members
        {
            get
            {
                return MembersExt.Select(m => m.Item1);
            }
        }

        /// <summary>
        /// Gets all members in the class with theirs attributes.
        /// </summary>
        /// <value>
        /// The members with theirs attributes.
        /// </value>
        protected IEnumerable<Tuple<MemberInfo, List<Attribute>>> MembersExt
        {
            get
            {
                var type = GetType();
                cacheLock.EnterUpgradeableReadLock();
                try
                {
                    if (!_membersDict.ContainsKey(type))
                    {
                        var members = type
                            .GetTypeInfo()
                            .GetMembers(BindingFlags.Public | BindingFlags.Instance)
                            .Select(m =>
                                new Tuple<MemberInfo, List<Attribute>>(m,
                                    m.GetCustomAttributes().ToList())).ToList();

                        cacheLock.EnterWriteLock();
                        try
                        {
                            if (!_membersDict.ContainsKey(type))
                            {
                                _membersDict.Add(type, members);
                            }

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
        }

        /// <summary>
        /// Create a setter for a member.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="member">The member information.</param>
        /// <returns>An action that sets the member of this object.</returns>
        /// <exception cref="ArgumentNullException">member</exception>
        protected Action<T> CreateSetter<T>(MemberInfo member) where T : struct
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));
            switch (member)
            {
                case MethodInfo mi:
                    return CreateSetterForMethod<T>(mi);
                case PropertyInfo pi:
                    return CreateSetterForProperty<T>(pi);
                case FieldInfo fi:
                    return CreateSetterForField<T>(fi);
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
        protected Func<T> CreateGetter<T>(MemberInfo member) where T : struct
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));

            switch (member)
            {
                case MethodInfo mi:
                    return CreateGetterForMethod<T>(mi);
                case PropertyInfo pi:
                    return CreateGetterForProperty<T>(pi);
                case FieldInfo fi:
                    return CreateGetterForField<T>(fi);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Sets the value of a member.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="member">The member information.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the member was set; otherwise <c>false</c>.
        /// </returns>
        protected bool SetMember<T>(MemberInfo member, T value) where T : struct
        { 
            if (member is PropertyInfo pi)
            {
                // Properties
                if ((pi.PropertyType == typeof(Parameter<T>) || pi.PropertyType.GetTypeInfo().IsSubclassOf(typeof(Parameter<T>))) && pi.CanRead)
                {
                    ((Parameter<T>) pi.GetValue(this)).Value = value;
                    return true;
                }

                if (pi.PropertyType == typeof(T) && pi.CanWrite)
                {
                    pi.SetValue(this, value);
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
                        mi.Invoke(this, new object[] { value });
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Creates a setter for a method.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="method">The method information.</param>
        /// <returns>
        /// An action that calls the method for this instance.
        /// </returns>
        private Action<T> CreateSetterForMethod<T>(MethodInfo method) where T : struct
        {
            // Match the return type
            if (method.ReturnType != typeof(void))
                return null;

            // Get parameters
            var parameters = method.GetParameters();
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(T))
                return (Action<T>) method.CreateDelegate(typeof(Action<T>), this);

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
        private Func<T> CreateGetterForMethod<T>(MethodInfo method) where T : struct
        {
            // Match the return type
            if (method.ReturnType != typeof(T))
                return null;

            // Get parameters
            var parameters = method.GetParameters();
            if (parameters.Length > 0)
                return null;

            // Turn it into a getter
            return (Func<T>) method.CreateDelegate(typeof(Func<T>), this);
        }

        /// <summary>
        /// Creates a setter for a property.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="property">The property information.</param>
        /// <returns>
        /// An action that sets the property value for this instance.
        /// </returns>
        private Action<T> CreateSetterForProperty<T>(PropertyInfo property) where T : struct
        {
            // Parameter objects are supported
            if (property.PropertyType == typeof(Parameter<T>) || property.PropertyType.GetTypeInfo().IsSubclassOf(typeof(Parameter<T>)))
            {
                // We can use the setter of the parameter!
                var p = (Parameter<T>) property.GetValue(this);
                return value => p.Value = value;
            }

            // Double properties are supported
            if (property.PropertyType == typeof(T))
            {
                return (Action<T>) property.GetSetMethod()?.CreateDelegate(typeof(Action<T>), this);
            }

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
        private Func<T> CreateGetterForProperty<T>(PropertyInfo property) where T : struct
        {
            // Parameter objects are supported
            if (property.PropertyType == typeof(Parameter<T>) ||
                property.PropertyType.GetTypeInfo().IsSubclassOf(typeof(Parameter<T>)))
            {
                // We can use the getter of the parameter!
                var p = (Parameter<T>) property.GetValue(this);
                return () => p.Value;
            }

            // Double properties are supported
            if (property.PropertyType == typeof(T))
                return (Func<T>) property.GetGetMethod()?.CreateDelegate(typeof(Func<T>), this);

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
        private Action<T> CreateSetterForField<T>(FieldInfo field) where T : struct
        {
            if (field.FieldType == typeof(T))
            {
                var constThis = Expression.Constant(this);
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
        private Func<T> CreateGetterForField<T>(FieldInfo field) where T : struct
        {
            if (field.FieldType == typeof(T))
            {
                var constThis = Expression.Constant(this);
                var constField = Expression.Field(constThis, field);
                var returnLabel = Expression.Label(typeof(T));
                return Expression.Lambda<Func<T>>(Expression.Label(returnLabel, constField)).Compile();
            }

            // Could not turn this into a getter
            return null;
        }
    }
}
