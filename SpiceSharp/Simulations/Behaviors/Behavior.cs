using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using SpiceSharp.Attributes;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Represents a behavior for a class
    /// </summary>
    public abstract class Behavior
    {
        /// <summary>
        /// Gets the name of the behavior
        /// </summary>
        public Identifier Name { get; }

        /// <summary>
        /// Constructor
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
        /// Create a delegate for extracting data for a specific simulation
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="propertyName">Parameter</param>
        /// <returns>Returns null if there is no export method</returns>
        public virtual Func<double> CreateExport(Simulation simulation, string propertyName)
        {
            return CreateExport<double>(simulation, propertyName);
        }

        /// <summary>
        /// Create a method for exporting a property using reflection. Supported:
        /// </summary>
        /// <typeparam name="T">Input type</typeparam>
        /// <typeparam name="TResult">Return type</typeparam>
        /// <param name="simulation">Simulation</param>
        /// <param name="property">Property name</param>
        /// <returns></returns>
        protected Func<TResult> CreateExport<TResult>(Simulation simulation, string property)
        {
            // Find methods to create the export
            var members = GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public);
            Func<TResult> result = null;
            foreach (var member in members)
            {
                // Skip members that don't have the right name
                if (!HasProperty(member, property))
                    continue;

                // Use methods
                if (member is MethodInfo mi)
                    result = CreateMethodExport<TResult>(simulation, mi);

                // Use properties
                if (member is PropertyInfo pi)
                    result = CreatePropertyExport<TResult>(simulation, pi);
                
                // Return
                if (result != null)
                    return result;
            }

            // Not found
            return null;
        }

        /// <summary>
        /// Create an export method from a MethodInfo (reflection)
        /// </summary>
        /// <typeparam name="TResult">Return type</typeparam>
        /// <param name="simulation">Simulation</param>
        /// <param name="method">Method</param>
        /// <returns></returns>
        private Func<TResult> CreateMethodExport<TResult>(Simulation simulation, MethodInfo method)
        {
            // First make sure it is the right return type
            if (method.ReturnType != typeof(TResult))
                return null;
            var parameters = method.GetParameters();

            // Method: TResult Method()
            if (parameters.Length == 0)
                return (Func<TResult>) method.CreateDelegate(typeof(Func<TResult>), this);

            // Methods with one parameter
            if (parameters.Length == 1)
            {
                // Method: TResult Method(Simulation)
                if (parameters[0].ParameterType == typeof(Simulation))
                {
                    var simMethod = (Func<Simulation, TResult>)method.CreateDelegate(typeof(Func<Simulation, TResult>), this);
                    return () => simMethod(simulation);
                }

                // Method: TResult Method(State)
                // Works for any child class of State
                if (parameters[0].ParameterType.IsSubclassOf(typeof(State)))
                {
                    // Get the state from the simulation that this method needs
                    if (!simulation.States.TryGetValue(parameters[0].ParameterType, out var state))
                        return null;

                    // Create the expression
                    var expression = Expression.Call(Expression.Constant(this), method, Expression.Constant(state));
                    return Expression.Lambda<Func<TResult>>(expression).Compile();
                }
            }

            return null;
        }

        /// <summary>
        /// Create an export method from a PropertyInfo (reflection)
        /// </summary>
        /// <typeparam name="TResult">Return type</typeparam>
        /// <param name="simulation">Simulation</param>
        /// <param name="property">Property</param>
        /// <returns></returns>
        private Func<TResult> CreatePropertyExport<TResult>(Simulation simulation, PropertyInfo property)
        {
            // First make sure it is the correct return type
            if (property.PropertyType != typeof(TResult))
                return null;

            // Return the getter method
            return (Func<TResult>) property.GetGetMethod().CreateDelegate(typeof(Func<TResult>), this);
        }

        /// <summary>
        /// Find out if the member is our named property
        /// </summary>
        /// <param name="member">Member</param>
        /// <param name="property">Property name</param>
        /// <returns></returns>
        private bool HasProperty(MemberInfo member, string property)
        {
            var names = (ParameterNameAttribute[]) member.GetCustomAttributes(typeof(ParameterNameAttribute), true);
            foreach (var attribute in names)
            {
                if (attribute.Name == property)
                    return true;
            }
            return false;
        }
    }
}
