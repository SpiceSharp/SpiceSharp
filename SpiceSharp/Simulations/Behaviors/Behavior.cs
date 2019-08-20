using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Template for a behavior.
    /// </summary>
    public abstract class Behavior : IBehavior
    {
        /// <summary>
        /// Gets the identifier of the behavior.
        /// </summary>
        /// <remarks>
        /// This should be the same identifier as the entity that created it.
        /// </remarks>
        public string Name { get; }

        /// <summary>
        /// Gets the simulation this behavior is bound to.
        /// </summary>
        protected Simulation Simulation { get; private set; }

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
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">The binding context.</param>
        public virtual void Bind(Simulation simulation, BindingContext context)
        {
            Simulation = simulation.ThrowIfNull(nameof(simulation));
        }

        /// <summary>
        /// Destroy the behavior.
        /// </summary>
        public virtual void Unbind()
        {
            Simulation = null;
        }

        /// <summary>
        /// Create a getter for a behavior parameter (possibly requiring a simulation or simulation state).
        /// </summary>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <param name="simulation">The simulation.</param>
        /// <param name="name">The parameter name.</param>
        /// <param name="comparer">The comparer used to compare property names.</param>
        /// <returns></returns>
        public Func<T> CreateGetter<T>(Simulation simulation, string name, IEqualityComparer<string> comparer = null)
        {
            // First find the method
            comparer = comparer ?? EqualityComparer<string>.Default;
            var method = Reflection.GetNamedMembers(this, name, comparer).FirstOrDefault(m => m is MethodInfo) as MethodInfo;
            if (method == null || method.ReturnType != typeof(T))
            {
                // Fall back to any member
                return ParameterHelper.CreateGetter<T>(this, name, comparer);
            }
            var parameters = method.GetParameters();

            // Method: TResult Method()
            if (parameters.Length == 0)
                return Reflection.CreateGetterForMethod<T>(this, method);

            // Methods with one parameter
            if (parameters.Length == 1)
            {
                // Method: <T> <Method>(<Simulation>)
                if (parameters[0].ParameterType.GetTypeInfo().IsAssignableFrom(simulation.GetType()))
                {
                    var simMethod = (Func<Simulation, T>)method.CreateDelegate(typeof(Func<Simulation, T>), this);
                    return () => simMethod(simulation);
                }

                // Method: TResult Method(State)
                // Works for any child class of SimulationState
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
                    var state = (SimulationState)stateMember.GetValue(simulation);

                    // Create the expression
                    return () => (T)method.Invoke(this, new[] { state });
                }
            }

            return null;
        }
    }
}
