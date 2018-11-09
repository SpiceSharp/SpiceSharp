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
    public abstract class Behavior : NamedParameterized, IBehavior, IPropertyExporter
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
        }

        /// <summary>
        /// Destroy the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public virtual void Unsetup(Simulation simulation)
        {
        }

        /// <summary>
        /// Creates a getter for a property.
        /// </summary>
        /// <typeparam name="T">The expected type.</typeparam>
        /// <param name="simulation">The simulation.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="comparer">The property name comparer.</param>
        /// <param name="function">The function that will return the value of the property.</param>
        /// <returns>
        /// <c>true</c> if the getter was created successfully; otherwise <c>false</c>.
        /// </returns>
        public bool CreateGetter<T>(Simulation simulation, string propertyName, IEqualityComparer<string> comparer, out Func<T> function) where T : struct
        {
            // Find methods to create the export
            Func<T> result = null;
            foreach (var member in Named(propertyName, comparer))
            {
                // Use methods
                if (member is MethodInfo mi)
                    result = CreateMethodGetter<T>(simulation, mi);

                // Use properties
                if (member is PropertyInfo pi)
                    result = CreateGetter<T>(pi);
                
                // Return
                if (result != null)
                {
                    function = result;
                    return true;
                }
            }

            // Not found
            function = null;
            return false;
        }

        /// <summary>
        /// Creates a getter for a property.
        /// </summary>
        /// <typeparam name="T">The expected type.</typeparam>
        /// <param name="simulation">The simulation.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="function">The function that will return the value of the property.</param>
        /// <returns>
        /// <c>true</c> if the getter was created successfully; otherwise <c>false</c>.
        /// </returns>
        public bool CreateGetter<T>(Simulation simulation, string propertyName, out Func<T> function) where T : struct
        {
            return CreateGetter(simulation, propertyName, EqualityComparer<string>.Default, out function);
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
