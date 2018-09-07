using System;
using System.Linq.Expressions;
using System.Reflection;
using SpiceSharp.Attributes;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Represents a behavior for a class
    /// </summary>
    public abstract class Behavior : NamedParameterized
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
        /// <param name="simulation">The simulation that is setting up</param>
        /// <param name="provider">The data provider</param>
        public virtual void Setup(Simulation simulation, SetupDataProvider provider)
        {
            // Do nothing
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        public virtual void Unsetup(Simulation simulation)
        {
            // Do nothing
        }

        /// <summary>
        /// Create a delegate for extracting data for a specific simulation
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="propertyName">Parameter</param>
        /// <returns>Returns null if there is no export method</returns>
        public virtual Func<double> CreateGetter(Simulation simulation, string propertyName)
        {
            return CreateGetter<double>(simulation, propertyName);
        }

        /// <summary>
        /// Create a method for exporting a property using reflection. Supported:
        /// </summary>
        /// <typeparam name="T">Base value type</typeparam>
        /// <param name="simulation">Simulation</param>
        /// <param name="name">Property name</param>
        /// <returns></returns>
        protected Func<T> CreateGetter<T>(Simulation simulation, string name) where T : struct 
        {
            // Find methods to create the export
            var members = GetType().GetTypeInfo().GetMembers(BindingFlags.Instance | BindingFlags.Public);
            Func<T> result = null;
            foreach (var member in Named(name))
            {
                // Use methods
                if (member is MethodInfo mi)
                    result = CreateMethodGetter<T>(simulation, mi);

                // Use properties
                if (member is PropertyInfo pi)
                    result = CreateGetter<T>(pi);
                
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
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="simulation">Simulation</param>
        /// <param name="method">Method</param>
        /// <returns></returns>
        private Func<T> CreateMethodGetter<T>(Simulation simulation, MethodInfo method) where T : struct
        {
            // First make sure it is the right return type
            if (method.ReturnType != typeof(T))
                return null;
            var parameters = method.GetParameters();

            // Method: TResult Method()
            if (parameters.Length == 0)
                return (Func<T>) method.CreateDelegate(typeof(Func<T>), this);

            // Methods with one parameter
            if (parameters.Length == 1)
            {
                // Method: <T> <Method>(<Simulation>)
                if (parameters[0].ParameterType == typeof(Simulation))
                {
                    var simMethod = (Func<Simulation, T>)method.CreateDelegate(typeof(Func<Simulation, T>), this);
                    return () => simMethod(simulation);
                }

                // Method: TResult Method(State)
                // Works for any child class of State
                if (parameters[0].ParameterType.GetTypeInfo().IsSubclassOf(typeof(State)))
                {
                    // Get the state from the simulation that this method needs
                    if (!simulation.States.TryGetValue(parameters[0].ParameterType, out var state))
                        return null;

                    // Create the expression
                    var expression = Expression.Call(Expression.Constant(this), method, Expression.Constant(state));
                    return Expression.Lambda<Func<T>>(expression).Compile();
                }
            }

            return null;
        }
    }
}
