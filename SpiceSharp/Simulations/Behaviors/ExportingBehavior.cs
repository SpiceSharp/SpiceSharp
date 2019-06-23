using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations.Behaviors
{
    /// <summary>
    /// Base class for a behavior that exports properties by using attributes on properties and reflection.
    /// </summary>
    /// <seealso cref="SpiceSharp.Attributes.NamedParameterized" />
    /// <seealso cref="SpiceSharp.Behaviors.IPropertyExporter" />
    public abstract class ExportingBehavior : IBehavior, IPropertyExporter
    {
        /// <summary>
        /// Gets the name of the behavior.
        /// </summary>
        /// <value>
        /// The name of the behavior.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        protected ExportingBehavior(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Setup the behavior for the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The provider.</param>
        public abstract void Setup(Simulation simulation, SetupDataProvider provider);

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
        public bool CreateExportMethod<T>(Simulation simulation, string propertyName, out Func<T> function, IEqualityComparer<string> comparer = null)
        {
            simulation.ThrowIfNull(nameof(simulation));

            // Find methods to create the export
            Func<T> result = null;
            foreach (var member in Reflection.GetNamedMembers(this, propertyName, comparer))
            {
                // Use methods
                if (member is MethodInfo mi)
                    result = CreateGetterForMethod<T>(simulation, mi);

                // Use properties
                else if (member is PropertyInfo pi)
                    result = ParameterHelper.CreateGetterForProperty<T>(this, pi);
                
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
        /// Creates a getter for a MethodInfo using reflection. This method allows creating getters for methods that needs the simulation as a parameter.
        /// </summary>
        /// <typeparam name="T">The base value type.</typeparam>
        /// <param name="simulation">The simulation.</param>
        /// <param name="method">The method information.</param>
        /// <returns>
        /// A function that returns the value of the method, or <c>null</c> if the method doesn't exist.
        /// </returns>
        private Func<T> CreateGetterForMethod<T>(Simulation simulation, MethodInfo method)
        {
            simulation.ThrowIfNull(nameof(simulation));

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
                    return () => (T)method.Invoke(this, new[] { state });
                }
            }

            return null;
        }
    }
}
