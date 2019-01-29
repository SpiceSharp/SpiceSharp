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
        /// Factories for behaviors by type.
        /// </summary>
        private static Dictionary<Type, BehaviorFactoryDictionary> BehaviorFactories { get; } =
            new Dictionary<Type, BehaviorFactoryDictionary>();

        /// <summary>
        /// Registers a behavior factory for an entity type.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="dictionary">The dictionary.</param>
        protected static void RegisterBehaviorFactory(Type entityType, BehaviorFactoryDictionary dictionary)
        {
            // We do this to avoid anyone unregistering factories!
            BehaviorFactories.Add(entityType, dictionary);
        }

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
        /// Creates a behavior of the specified type.
        /// </summary>
        /// <param name="type">The type of the behavior</param>
        /// <param name="simulation">The simulation.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">simulation</exception>
        public virtual IBehavior CreateBehavior(Type type, Simulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            // Get the factory and generate it
            if (!BehaviorFactories.TryGetValue(GetType(), out var behaviors))
                return null;
            if (behaviors.TryGetValue(type, out var behavior))
                return behavior(Name);
            return null;
        }

        /// <summary>
        /// Sets up the behavior.
        /// </summary>
        /// <param name="behavior">The behavior that needs to be set up.</param>
        /// <param name="simulation">The simulation.</param>
        /// <exception cref="ArgumentNullException">simulation</exception>
        public virtual void SetupBehavior(IBehavior behavior, Simulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            // Build the setup behavior
            var provider = BuildSetupDataProvider(simulation.EntityParameters, simulation.EntityBehaviors);
            behavior.Setup(simulation, provider);
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
        /// Gets the priority of this entity.
        /// </summary>
        public int Priority { get; protected set; } = 0;

        /// <summary>
        /// Clones the entity with a new name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public virtual Entity Clone(string name)
        {
            var e = (Entity) Activator.CreateInstance(GetType(), name);

            return e;
        }
    }
}
