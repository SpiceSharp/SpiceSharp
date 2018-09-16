using System;
using System.Reflection;
using SpiceSharp.Attributes;

namespace SpiceSharp
{
    /// <summary>
    /// Base class for parameters
    /// </summary>
    public abstract class ParameterSet : NamedParameterized
    {
        /// <summary>
        /// Get a parameter object
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns></returns>
        public Parameter<T> GetParameter<T>(string name) where T : struct
        {
            // Get the property by name
            foreach (var member in Named(name))
            {
                // Check for methods
                if (member is MethodInfo mi)
                {
                    if ((mi.ReturnType == typeof(Parameter<T>) || mi.ReturnType.GetTypeInfo().IsSubclassOf(typeof(Parameter<T>))) && mi.GetParameters().Length == 0)
                        return (Parameter<T>) mi.Invoke(this, null);
                }

                // Check for properties
                if (member is PropertyInfo pi)
                {
                    if ((pi.PropertyType == typeof(Parameter<T>) || pi.PropertyType.GetTypeInfo().IsSubclassOf(typeof(Parameter<T>))) && pi.CanRead)
                        return (Parameter<T>) pi.GetValue(this);
                }
            }

            // Not found
            return null;
        }

        /// <summary>
        /// Get a parameter object
        /// </summary>
        /// <returns></returns>
        public Parameter<T> GetParameter<T>() where T : struct
        {
            var member = Principal;
            if (member != null)
            {
                // Check for a methods
                if (member is MethodInfo mi)
                {
                    if ((mi.ReturnType == typeof(Parameter<T>) || mi.ReturnType.GetTypeInfo().IsSubclassOf(typeof(Parameter<T>))) && mi.GetParameters().Length == 0)
                        return (Parameter<T>)mi.Invoke(this, null);
                }

                // Check for properties
                if (member is PropertyInfo pi)
                {
                    if ((pi.PropertyType == typeof(Parameter<T>) || pi.PropertyType.GetTypeInfo().IsSubclassOf(typeof(Parameter<T>))) && pi.CanRead)
                        return (Parameter<T>)pi.GetValue(this);
                }
            }

            // Not found
            return null;
        }

        /// <summary>
        /// Set a parameter by name
        /// If multiple parameters have the same name, they will all be set
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="value">Value</param>
        /// <returns>True if the parameter was set</returns>
        public bool SetParameter<T>(string name, T value)
        {
            // Set the property if any
            var isset = false;
            foreach (var member in Named(name))
            {
                // Set the member
                if (SetMember(member, value))
                    isset = true;
            }
            return isset;
        }

        /// <summary>
        /// Calls a parameter method by name without arguments
        /// If multiple parameters by this name exist, all of them will be called
        /// </summary>
        /// <param name="name">Name of the method</param>
        /// <returns></returns>
        public bool SetParameter(string name)
        {
            // Set the property if any
            var isset = false;
            foreach (var member in Named(name))
            {
                // Set the member
                if (member is MethodInfo mi)
                {
                    var parameters = mi.GetParameters();
                    if (parameters.Length == 0)
                    {
                        mi.Invoke(this, null);
                        isset = true;
                    }
                }
            }
            return isset;
        }

        /// <summary>
        /// Sets the principal parameter of the set
        /// Only the first principal parameter is changed
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns></returns>
        public bool SetParameter<T>(T value) 
        {
            var p = Principal;
            if (p != null)
                return SetMember(p, value);
            return false;
        }

        /// <summary>
        /// Set a parameter by name
        /// Use for any value
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="value">Value</param>
        /// <returns>Returns true if the parameter was set</returns>
        public bool SetParameter(string name, object value)
        {
            // Set the property if any
            var isset = false;
            foreach (var member in Named(name))
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
                            mi.Invoke(this, new[] { value });
                            isset = true;
                        }
                    }
                }
            }
            return isset;
        }

        /// <summary>
        /// Calculate default parameter values that depend on other parameters
        /// </summary>
        /// <remarks>
        /// These calculations should not depend on temperature! Temperature-dependent calculations are
        /// part of the <see cref="SpiceSharp.Behaviors.BaseTemperatureBehavior"/>.
        /// </remarks>
        public virtual void CalculateDefaults()
        {
            // By default, there are no parameter values that depend on others
        }

        /// <summary>
        /// Creates a deep clone of the parameter set.
        /// </summary>
        /// <returns>
        /// A deep clone of the parameter set.
        /// </returns>
        public virtual ParameterSet DeepClone()
        {
            //1. Make new object
            var destinationObject = (ParameterSet)Activator.CreateInstance(GetType());

            //2. Copy properties of the current object
            Utility.CopyPropertiesAndFields(this, destinationObject);

            return destinationObject;
        }
    }
}
