using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SpiceSharp.Attributes;

namespace SpiceSharp
{
    /// <summary>
    /// A class that describes a set of parameters.
    /// </summary>
    /// <remarks>
    /// This class allows accessing parameters by their metadata. Metadata is specified by using 
    /// the <see cref="ParameterNameAttribute"/> and <see cref="ParameterInfoAttribute"/>.
    /// </remarks>
    /// <seealso cref="NamedParameterized" />
    public abstract class ParameterSet : NamedParameterized
    {
        /// <summary>
        /// Gets a parameter with a specified name.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>
        /// The parameter with the specified name.
        /// </returns>
        public Parameter<T> GetParameter<T>(string name, IEqualityComparer<string> comparer = null) where T : struct
        {
            var parameters = GetParameters<T>();

            if (comparer != null)
            {
                return parameters.FirstOrDefault(p => p.Item2.Any(r => comparer.Equals(name, r)))?.Item1;
            }
            else
            {
                return parameters.FirstOrDefault(p => p.Item2.Any(r => r == name))?.Item1;
            }
        }

        /// <summary>
        /// Gets a parameter with a specified name.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <returns>
        /// The parameter with the specified name.
        /// </returns>
        public IEnumerable<Tuple<Parameter<T>, List<string>>> GetParameters<T>() where T : struct
        {
            // Get the property by name
            foreach (var member in Named())
            {
                // Check for methods
                if (member.Item1 is MethodInfo mi)
                {
                    if ((mi.ReturnType == typeof(Parameter<T>) ||
                         mi.ReturnType.GetTypeInfo().IsSubclassOf(typeof(Parameter<T>))) &&
                        mi.GetParameters().Length == 0)
                        yield return new Tuple<Parameter<T>, List<string>>(
                            (Parameter<T>) mi.Invoke(this, null),
                            member.Item2
                        );
                }

                // Check for properties
                if (member.Item1 is PropertyInfo pi)
                {
                    if ((pi.PropertyType == typeof(Parameter<T>) ||
                         pi.PropertyType.GetTypeInfo().IsSubclassOf(typeof(Parameter<T>))) && pi.CanRead)
                        yield return new Tuple<Parameter<T>, List<string>>(
                            (Parameter<T>) pi.GetValue(this),
                            member.Item2);
                }
            }
        }

        /// <summary>
        /// Get the principal parameter.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <returns>The principal parameter.</returns>
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
        /// Sets a parameter with a specified name.
        /// If multiple parameters have the same name, they will all be set.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>
        ///   <c>true</c> if there was one or more parameters set; otherwise <c>false</c>.
        /// </returns>
        public bool SetParameter<T>(string name, T value, IEqualityComparer<string> comparer = null) where T : struct
        {
            // Set the property if any
            var isset = false;
            foreach (var member in Named(name, comparer))
            {
                // Set the member
                if (SetMember(member, value))
                    isset = true;
            }
            return isset;
        }

        /// <summary>
        /// Calls a method by name without arguments.
        /// If multiple parameters by this name exist, all of them will be called.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <returns>
        ///   <c>true</c> if there was one or more methods called; otherwise <c>false</c>.
        /// </returns>
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
        /// Calls a method by name without arguments.
        /// If multiple parameters by this name exist, all of them will be called.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>
        ///   <c>true</c> if there was one or more methods called; otherwise <c>false</c>.
        /// </returns>
        public bool SetParameter(string name, IEqualityComparer<string> comparer)
        {
            // Set the property if any
            var isset = false;
            foreach (var member in Named(name, comparer))
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
        /// Sets the value of the principal parameter. Only the first principal parameter is changed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if a principal parameter was set; otherwise <c>false</c>.
        /// </returns>
        public bool SetParameter<T>(T value) where T : struct
        {
            var p = Principal;
            if (p != null)
                return SetMember(p, value);
            return false;
        }

        /// <summary>
        /// Sets a parameter with a specified name. The type of the parameter
        /// can be anything but has to match the signature of the parameter.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>
        ///   <c>true</c> if one or more parameters are set; otherwise <c>false</c>.
        /// </returns>
        public bool SetParameter(string name, object value, IEqualityComparer<string> comparer = null)
        {
            // Set the property if any
            var isset = false;
            foreach (var member in Named(name, comparer))
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
        /// Method for calculating the default values of derived parameters.
        /// </summary>
        /// <remarks>
        /// These calculations should be run whenever a parameter has been changed.
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
            // 1. Make new object
            var destinationObject = (ParameterSet) Activator.CreateInstance(GetType());

            // 2. Copy properties of the current object
            Utility.CopyPropertiesAndFields(this, destinationObject);

            return destinationObject;
        }
    }
}
