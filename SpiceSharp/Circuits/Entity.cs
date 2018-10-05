using System;
using System.Collections.Generic;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// Base class for any circuit object that can take part in simulations.
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// Factories for behaviors.
        /// </summary>
        protected BehaviorFactoryDictionary Behaviors { get; } = new BehaviorFactoryDictionary();

        /// <summary>
        /// Gets a collection of parameters.
        /// </summary>
        public ParameterSetDictionary ParameterSets { get; } = new ParameterSetDictionary();

        /// <summary>
        /// Gets the name of the entity.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        protected Entity(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Sets a parameter with a specific name.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>False if the parameter could not be found.</returns>
        public bool SetParameter(string name, double value, IEqualityComparer<string> comparer = null) => ParameterSets.SetParameter(name, value, comparer);

        /// <summary>
        /// Sets a parameter with a specific name.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>False if the parameter could not be found.</returns>
        public bool SetParameter(string name, object value, IEqualityComparer<string> comparer = null) => ParameterSets.SetParameter(name, value, comparer);

        /// <summary>
        /// Create a behavior of a specific base type for a simulation.
        /// </summary>
        /// <typeparam name="T">The behavior base type.</typeparam>
        /// <param name="simulation">The simulation that will use the behavior.</param>
        /// <returns>A behavior of the requested type, or null if it doesn't apply to this entity.</returns>
        /// <exception cref="ArgumentNullException">simulation</exception>
        public virtual T CreateBehavior<T>(Simulation simulation) where T : Behavior
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            // Try to extract the right behavior from our behavior factories
            if (Behaviors.TryGetValue(typeof(T), out var factory))
            {
                // Create the behavior
                var behavior = factory();

                // Setup the behavior
                var provider = BuildSetupDataProvider(simulation.EntityParameters, simulation.EntityBehaviors);
                behavior.Setup(simulation, provider);
                return (T)behavior;
            }

            // None found
            return null;
        }

        /// <summary>
        /// Build the data provider for setting up a behavior for the entity. The entity can control which parameters
        /// and behaviors are visible to behaviors using this method.
        /// </summary>
        /// <param name="parameters">The parameters in the simulation.</param>
        /// <param name="behaviors">The behaviors in the simulation.</param>
        /// <returns>A data provider for the behaviors.</returns>
        /// <exception cref="ArgumentNullException">
        /// parameters
        /// or
        /// behaviors
        /// </exception>
        protected virtual SetupDataProvider BuildSetupDataProvider(ParameterPool parameters, BehaviorPool behaviors)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));
            if (behaviors == null)
                throw new ArgumentNullException(nameof(behaviors));

            // By default, we include the parameters of this entity
            var result = new SetupDataProvider();
            result.Add("entity", parameters[Name]);
            result.Add("entity", behaviors[Name]);
            return result;
        }

        /// <summary>
        /// Gets the priority of this object.
        /// </summary>
        public int Priority { get; protected set; } = 0;
    }
}
