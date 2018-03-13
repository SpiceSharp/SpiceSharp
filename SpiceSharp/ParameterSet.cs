using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SpiceSharp.Attributes;
using SpiceSharp.Diagnostics;

namespace SpiceSharp
{
    /// <summary>
    /// Base class for parameters
    /// </summary>
    public abstract class ParameterSet
    {
        /// <summary>
        /// Create a dictionary of setters for all parameters by their name
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, Action<double>> CreateAllSetters()
        {
            var result = new Dictionary<string, Action<double>>();

            // Get all properties with the SpiceName attribute
            var members = GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public);
            foreach (var member in members)
            {
                // Skip properties without a SpiceName attribute
                if (!member.IsDefined(typeof(ParameterNameAttribute), true))
                    continue;

                // Create setter
                Action<double> setter = null;
                if (member is PropertyInfo pi)
                    setter = CreateSetterForProperty(pi);
                else if (member is MethodInfo mi)
                    setter = CreateSetterForMethod(mi);
                else if (member is FieldInfo fi)
                    setter = CreateSetterForField(fi);

                // Skip if no setter could be created
                if (setter == null)
                    continue;

                // Store the setter
                var names = member.GetCustomAttributes<ParameterNameAttribute>();
                foreach (var name in names)
                    result[name.Name] = setter;
            }
            return result;
        }

        /// <summary>
        /// Get a parameter object
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns></returns>
        public Parameter GetParameter(string name)
        {
            // Get the property by name
            var members = GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public);
            foreach (var member in members)
            {
                // Check for valid naming
                if (!HasName(member, name))
                    continue;

                // Check for methods
                if (member is MethodInfo mi)
                {
                    if (mi.ReturnType == typeof(Parameter) && mi.GetParameters().Length == 0)
                        return (Parameter) mi.Invoke(this, null);
                }

                // Check for properties
                if (member is PropertyInfo pi)
                {
                    if (pi.PropertyType == typeof(Parameter) && pi.CanRead)
                        return (Parameter) pi.GetValue(this);
                }
            }

            // Not found
            return null;
        }

        /// <summary>
        /// Get a setter for a parameter
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <returns></returns>
        public Action<double> GetSetter(string name)
        {
            var members = GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public);
            foreach (var member in members)
            {
                // Skip members we're not interested in
                if (!HasName(member, name))
                    continue;

                // Create a setter
                Action<double> setter = null;
                if (member is PropertyInfo pi)
                    setter = CreateSetterForProperty(pi);
                else if (member is MethodInfo mi)
                    setter = CreateSetterForMethod(mi);
                else if (member is FieldInfo fi)
                    setter = CreateSetterForField(fi);

                // Return the created setter if successful
                if (setter != null)
                    return setter;
            }

            // Could not create a setter
            return null;
        }

        /// <summary>
        /// Set a parameter by name
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="value">Value</param>
        /// <returns>True if the parameter was set</returns>
        public bool Set(string name, double value)
        {
            // Get the property by name
            var members = GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public);

            // Set the property if any
            bool isset = false;
            foreach (var member in members)
            {
                // Skip members that are not interesting to use
                if (!HasName(member, name))
                    continue;

                if (member is PropertyInfo pi)
                {
                    // Properties
                    if (pi.PropertyType == typeof(Parameter) && pi.CanRead)
                    {
                        ((Parameter)pi.GetValue(this)).Set(value);
                        isset = true;
                    }
                    else if (pi.PropertyType == typeof(double) && pi.CanWrite)
                    {
                        pi.SetValue(this, value);
                        isset = true;
                    }
                }
                else if (member is MethodInfo mi)
                {
                    // Methods
                    if (mi.ReturnType == typeof(void))
                    {
                        var paraminfo = mi.GetParameters();
                        if (paraminfo.Length == 1 && paraminfo[0].ParameterType == typeof(double))
                        {
                            mi.Invoke(this, new object[] { value });
                            isset = true;
                        }
                    }
                }
            }
            return isset;
        }

        /// <summary>
        /// Set a parameter by name
        /// Use for non-double values, will ignore
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="value">Value</param>
        /// <returns>Returns true if the parameter was set</returns>
        public bool Set(string name, object value)
        {
            // Get the property by name
            var members = GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public);

            // Set the property if any
            bool isset = false;
            foreach (var member in members)
            {
                // Skip members that are not interesting to us
                if (!HasName(member, name))
                    continue;

                if (member is PropertyInfo pi)
                {
                    // Properties
                    if (pi.CanWrite)
                    {
                        pi.SetValue(this, value);
                        isset = true;
                    }
                }
                else if (member is MethodInfo mi)
                {
                    // Methods
                    if (mi.ReturnType == typeof(void))
                    {
                        var paraminfo = mi.GetParameters();
                        if (paraminfo.Length == 1)
                        {
                            mi.Invoke(this, new[] { value });
                            isset = true;
                        }
                    }
                }
            }
            return isset;
        }

        /// <summary>
        /// Find out if the member is our named property
        /// </summary>
        /// <param name="member">Member</param>
        /// <param name="property">Property name</param>
        /// <returns>True if the member has the property name as an attribute</returns>
        private bool HasName(MemberInfo member, string property)
        {
            var names = (ParameterNameAttribute[])member.GetCustomAttributes(typeof(ParameterNameAttribute), true);
            foreach (var attribute in names)
            {
                if (attribute.Name == property)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Create a setter delegate for methods
        /// </summary>
        /// <param name="method">Method information</param>
        private Action<double> CreateSetterForMethod(MethodInfo method)
        {
            // Match the return type
            if (method.ReturnType != typeof(void))
                return null;

            // Get parameters
            var parameters = method.GetParameters();
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(double))
                return (Action<double>) method.CreateDelegate(typeof(Action<double>), this);

            // Could not turn it into a setter
            return null;
        }

        /// <summary>
        /// Create a setter delegate for properties
        /// </summary>
        /// <param name="property">Property information</param>
        /// <returns></returns>
        private Action<double> CreateSetterForProperty(PropertyInfo property)
        {
            // Parameter objects are supported
            if (property.PropertyType == typeof(Parameter))
            {
                // We can use the setter of the parameter!
                Parameter p = (Parameter) property.GetValue(this);
                return p.Set;
            }

            // Double properties are supported
            if (property.PropertyType == typeof(double))
            {
                return (Action<double>) property.GetSetMethod()?.CreateDelegate(typeof(Action<double>), this);
            }

            // Could not turn it into a setter
            return null;
        }

        /// <summary>
        /// Create a setter delegate for fields
        /// </summary>
        /// <param name="field">Field information</param>
        /// <returns></returns>
        private Action<double> CreateSetterForField(FieldInfo field)
        {
            if (field.FieldType == typeof(double))
            {
                var constThis = Expression.Constant(this);
                var constField = Expression.Field(constThis, field);
                var paramValue = Expression.Parameter(typeof(double), "value");
                var assignField = Expression.Assign(constField, paramValue);
                return Expression.Lambda<Action<double>>(assignField, paramValue).Compile();
            }

            // Could not turn this into a setter
            return null;
        }
    }
}
