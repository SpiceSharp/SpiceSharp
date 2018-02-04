using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Simulations;
using SpiceSharp.Attributes;
using System.Reflection;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Represents a behavior for a class
    /// </summary>
    public abstract class Behavior
    {
        /// <summary>
        /// The component the behavior acts upon
        /// </summary>
        protected Entity Component { get; private set; }
        
        /// <summary>
        /// Gets the name of the behavior
        /// </summary>
        public Identifier Name { get; }

        /// <summary>
        /// Constructor
        /// NOTE: remove default later
        /// </summary>
        /// <param name="name">Name of the behavior</param>
        protected Behavior(Identifier name)
        {
            Name = name;
        }
        
        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">The data provider</param>
        public virtual void Setup(SetupDataProvider provider)
        {
            // Do nothing
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public virtual void Unsetup()
        {
            // Do nothing
        }

        /// <summary>
        /// Create a delegate for extracting data
        /// </summary>
        /// <param name="propertyName">Parameter</param>
        /// <returns>Returns null if there is no export method</returns>
        public virtual Func<RealState, double> CreateExport(string propertyName)
        {
            return CreateExport<RealState, double>(propertyName);
        }

        /// <summary>
        /// Create a method for exporting a property
        /// </summary>
        /// <typeparam name="T">Input type</typeparam>
        /// <typeparam name="TResult">Return type</typeparam>
        /// <param name="property">Property name</param>
        /// <returns></returns>
        protected Func<T, TResult> CreateExport<T, TResult>(string property)
        {
            // Find methods to create the export
            var members = GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public);
            foreach (var member in members)
            {
                // Use methods
                if (member is MethodInfo mi)
                {
                    // Check the return type (needs to be a double)
                    if (mi.ReturnType != typeof(TResult))
                        continue;

                    // Check the name
                    var names = (PropertyNameAttribute[])member.GetCustomAttributes(typeof(PropertyNameAttribute), true);
                    bool found = false;
                    foreach (var name in names)
                    {
                        if (name.Name == property)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        continue;

                    // Check the parameters
                    var parameters = mi.GetParameters();
                    if (parameters.Length != 1)
                        continue;
                    if (parameters[0].ParameterType != typeof(T))
                        continue;

                    // Return a delegate
                    return (Func<T, TResult>)mi.CreateDelegate(typeof(Func<T, TResult>), this);
                }

                // Use properties
                if (member is PropertyInfo pi)
                {
                    if (pi.PropertyType != typeof(TResult))
                        continue;

                    // Check the name
                    var names = (PropertyNameAttribute[])member.GetCustomAttributes(typeof(PropertyNameAttribute), true);
                    bool found = false;
                    foreach (var name in names)
                    {
                        if (name.Name == property)
                        {
                            found = true;
                            continue;
                        }
                    }
                    if (!found)
                        continue;

                    // Return the getter!
                    var getmethod = (Func<TResult>)pi.GetGetMethod().CreateDelegate(typeof(Func<TResult>), this);
                    return (T state) => getmethod();
                }
            }

            // Not found
            return null;
        }
    }
}
