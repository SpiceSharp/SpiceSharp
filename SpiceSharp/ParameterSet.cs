using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using SpiceSharp.Attributes;

namespace SpiceSharp
{
    /// <summary>
    /// Base class for parameters
    /// </summary>
    public abstract class ParameterSet
    {
        /// <summary>
        /// Create a dictionary of setters for the parameters object using reflection
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, ParameterSetter> CreateSetters()
        {
            Dictionary<string, ParameterSetter> result = new Dictionary<string, ParameterSetter>();

            // Get all properties with the SpiceName attribute
            var properties = GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in properties)
            {
                // Skip properties without a SpiceName attribute
                if (!property.IsDefined(typeof(PropertyNameAttribute), true))
                    continue;

                // Get the names
                var names = property.GetCustomAttributes<PropertyNameAttribute>();

                // Create setter
                ParameterSetter setter = null;
                if (property is PropertyInfo pi)
                {
                    // Properties
                    if (pi.PropertyType == typeof(Parameter))
                    {
                        Parameter p = (Parameter)pi.GetValue(this);
                        setter = p.Set;
                    }
                    else if (pi.PropertyType == typeof(double))
                    {
                        setter = (ParameterSetter)Delegate.CreateDelegate(typeof(ParameterSetter), this, pi.GetSetMethod());
                    }
                }
                else if (property is MethodInfo mi)
                {
                    // Methods
                    if (mi.ReturnType == typeof(void))
                    {
                        var paraminfo = mi.GetParameters();
                        if (paraminfo.Length == 1 && paraminfo[0].ParameterType == typeof(double))
                        {
                            setter = (ParameterSetter)mi.CreateDelegate(typeof(ParameterSetter));
                        }
                    }
                }

                // Skip if no setter can be created
                if (setter == null)
                    continue;

                // Store the setter
                foreach (var name in names)
                    result.Add(name.Name, setter);
            }

            return result;
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
            var members = GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public)
                .Where((MemberInfo mi) => mi.IsDefined(typeof(PropertyNameAttribute)))
                .Where((MemberInfo mi) =>
                {
                    foreach (var sn in mi.GetCustomAttributes<PropertyNameAttribute>())
                    {
                        if (sn.Name == name)
                            return true;
                    }
                    return false;
                });

            // Set the property if any
            bool isset = false;
            foreach (var member in members)
            {
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
            var members = GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public)
                .Where((MemberInfo mi) => mi.IsDefined(typeof(PropertyNameAttribute)))
                .Where((MemberInfo mi) =>
                {
                    foreach (var sn in mi.GetCustomAttributes<PropertyNameAttribute>())
                    {
                        if (sn.Name == name)
                            return true;
                    }
                    return false;
                });

            // Set the property if any
            bool isset = false;
            foreach (var member in members)
            {
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
                            mi.Invoke(this, new object[] { value });
                            isset = true;
                        }
                    }
                }
            }
            return isset;
        }
    }

    /// <summary>
    /// Delegate for setting a parameter value
    /// </summary>
    /// <param name="value">Value</param>
    public delegate void ParameterSetter(double value);
}
