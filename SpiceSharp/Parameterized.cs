using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace SpiceSharp
{
    /// <summary>
    /// A class that can be used to address properties and fields using reflection
    /// </summary>
    public abstract class Parameterized
    {
        /// <summary>
        /// Gets all members in the class
        /// </summary>
        protected IEnumerable<MemberInfo> Members
        {
            get
            {
                var members = GetType().GetTypeInfo().GetMembers(BindingFlags.Public | BindingFlags.Instance);
                return members;
            }
        }

        /// <summary>
        /// Create a setter
        /// </summary>
        /// <typeparam name="T">Base value type</typeparam>
        /// <param name="member">Member information</param>
        /// <returns></returns>
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
        /// Create a getter
        /// </summary>
        /// <typeparam name="T">Base value type</typeparam>
        /// <param name="member">Member information</param>
        /// <returns></returns>
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
        /// Set the value of a member
        /// </summary>
        /// <typeparam name="T">Base value type</typeparam>
        /// <param name="member">Member information</param>
        /// <param name="value">Value</param>
        /// <returns>Returns true if the member was set</returns>
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
        /// Create a setter delegate for methods
        /// </summary>
        /// <typeparam name="T">Base value type</typeparam>
        /// <param name="method">Method information</param>
        /// <returns></returns>
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
        /// Create a getter delegate for methods
        /// </summary>
        /// <typeparam name="T">Base value type</typeparam>
        /// <param name="method">Method information</param>
        /// <returns></returns>
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
        /// Create a setter delegate for properties
        /// </summary>
        /// <typeparam name="T">Base value type</typeparam>
        /// <param name="property">Property information</param>
        /// <returns></returns>
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
        /// Create a getter for a property
        /// </summary>
        /// <typeparam name="T">Base value type</typeparam>
        /// <param name="property">Property information</param>
        /// <returns></returns>
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
        /// Create a setter delegate for fields
        /// </summary>
        /// <typeparam name="T">Base value type</typeparam>
        /// <param name="field">Field information</param>
        /// <returns></returns>
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
        /// Create a getter for fields
        /// </summary>
        /// <typeparam name="T">Base value type</typeparam>
        /// <param name="field">Field information</param>
        /// <returns></returns>
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
