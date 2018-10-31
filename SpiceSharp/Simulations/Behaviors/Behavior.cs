using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SpiceSharp.Attributes;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Template for a behavior.
    /// </summary>
    public abstract class Behavior : NamedParameterized, IBehavior, IPropertyExporter<double>
    {
        /// <summary>
        /// Gets the identifier of the behavior.
        /// </summary>
        /// <remarks>
        /// This should be the same identifier as the entity that created it.
        /// </remarks>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Behavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        protected Behavior(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Setup the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The data provider.</param>
        public virtual void Setup(Simulation simulation, SetupDataProvider provider)
        {
            // Do nothing
        }

        /// <summary>
        /// Destroy the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public virtual void Unsetup(Simulation simulation)
        {
            // Do nothing
        }

        /// <summary>
        /// Creates a getter for extracting data from the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation</param>
        /// <param name="propertyName">The name of the parameter.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>
        /// A getter that returns the value of the specified parameter, or <c>null</c> if no parameter was found.
        /// </returns>
        public virtual Func<double> CreateGetter(Simulation simulation, string propertyName, IEqualityComparer<string> comparer)
        {
            return CreateGetter<double>(simulation, propertyName, comparer);
        }

        /// <summary>
        /// Creates a getter for extracting data from the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation</param>
        /// <param name="propertyName">The name of the parameter.</param>
        /// <returns>
        /// A getter that returns the value of the specified parameter, or <c>null</c> if no parameter was found.
        /// </returns>
        public virtual Func<double> CreateGetter(Simulation simulation, string propertyName)
        {
            return CreateGetter<double>(simulation, propertyName, null);
        }

        /// <summary>
        /// Creates a getter for extracting data from the specified simulation.
        /// </summary>
        /// <typeparam name="T">The base value type</typeparam>
        /// <param name="simulation">The simulation.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>
        /// A getter that returns the value of the specified parameter, or <c>null</c> if no parameter was found.
        /// </returns>
        protected Func<T> CreateGetter<T>(Simulation simulation, string name, IEqualityComparer<string> comparer) where T : struct 
        {
            // Find methods to create the export
            Func<T> result = null;
            foreach (var member in Named(name, comparer))
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
        /// Creates a getter for a MethodInfo using reflection. This method allows creating getters for methods that needs the simulation as a parameter.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="simulation">The simulation.</param>
        /// <param name="method">The method information.</param>
        /// <returns>
        /// A function that returns the value of the method, or <c>null</c> if the method doesn't exist.
        /// </returns>
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
                var paramType = parameters[0].ParameterType;
                if (paramType.GetTypeInfo().IsSubclassOf(typeof(SimulationState)))
                {
                    // Try to find a property of the same type using reflection
                    var stateMember = simulation.GetType().GetTypeInfo()
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .FirstOrDefault(property => property.PropertyType == paramType);
                    if (stateMember == null)
                        return null;

                    // Get this state
                    var state = (SimulationState) stateMember.GetValue(simulation);

                    // Create the expression
                    var expression = Expression.Call(Expression.Constant(this), method, Expression.Constant(state));
                    return Expression.Lambda<Func<T>>(expression).Compile();
                }
            }

            return null;
        }
    }
}
