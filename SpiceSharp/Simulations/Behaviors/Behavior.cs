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
        /// Get the name of the behavior
        /// </summary>
        public Identifier Name { get; }

        /// <summary>
        /// Constructor
        /// NOTE: remove default later
        /// </summary>
        /// <param name="name">Name of the behavior</param>
        public Behavior(Identifier name)
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
        /// <param name="property">Parameter</param>
        /// <returns>Returns null if there is no export method</returns>
        public virtual Func<State, double> CreateExport(string property)
        {
            return CreateExport<State, double>(property);
        }

        /// <summary>
        /// Create a method for exporting a property
        /// </summary>
        /// <typeparam name="S">Input type</typeparam>
        /// <typeparam name="R">Return type</typeparam>
        /// <param name="property">Property name</param>
        /// <returns></returns>
        protected Func<S, R> CreateExport<S, R>(string property)
        {
            // Find methods to create the export
            var members = GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public);
            foreach (var member in members)
            {
                // Use methods
                if (member is MethodInfo mi)
                {
                    // Check the return type (needs to be a double)
                    if (mi.ReturnType != typeof(R))
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
                    if (parameters[0].ParameterType != typeof(S))
                        continue;

                    // Return a delegate
                    return (Func<S, R>)mi.CreateDelegate(typeof(Func<S, R>), this);
                }

                // Use properties
                if (member is PropertyInfo pi)
                {
                    if (pi.PropertyType != typeof(R))
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
                    var getmethod = (Func<R>)pi.GetGetMethod().CreateDelegate(typeof(Func<R>), this);
                    return (S state) => getmethod();
                }
            }

            // Not found
            return null;
        }
    }
}
