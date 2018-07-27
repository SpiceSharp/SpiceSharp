using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// Base class for any circuit object that can take part in simulations
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// Factories for behaviors
        /// </summary>
        protected BehaviorFactoryDictionary Behaviors { get; } = new BehaviorFactoryDictionary();

        /// <summary>
        /// Gets a collection of parameters
        /// </summary>
        public ParameterSetDictionary ParameterSets { get; } = new ParameterSetDictionary();

        /// <summary>
        /// Gets the name of the object
        /// </summary>
        public Identifier Name { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the object</param>
        protected Entity(Identifier name)
        {
            Name = name;
        }

        /// <summary>
        /// Sets a parameter
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Value</param>
        /// <returns>False if the parameter could not be found</returns>
        public bool SetParameter(string name, double value) => ParameterSets.SetParameter(name, value);

        /// <summary>
        /// Sets a parameter
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Value</param>
        /// <returns>False if the parameter could not be found</returns>
        public bool SetParameter(string name, object value) => ParameterSets.SetParameter(name, value);

        /// <summary>
        /// Create a behavior for a simulation
        /// </summary>
        /// <typeparam name="T">Behavior base type</typeparam>
        /// <param name="simulation"></param>
        /// <returns></returns>
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
                behavior.Setup(provider);
                return (T)behavior;
            }

            // None found
            return null;
        }

        /// <summary>
        /// Build the data provider for setting up a behavior for the entity
        /// The entity can control which parameters and behaviors are visible to behaviors in this way
        /// </summary>
        /// <returns></returns>
        protected virtual SetupDataProvider BuildSetupDataProvider(ParameterPool parameters, BehaviorPool behaviors)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));
            if (behaviors == null)
                throw new ArgumentNullException(nameof(behaviors));

            // By default, we include the parameters of this entity
            var result = new SetupDataProvider();
            result.Add("entity", parameters.GetEntityParameters(Name));
            result.Add("entity", behaviors.GetEntityBehaviors(Name));
            return result;
        }

        /// <summary>
        /// Gets the priority of this object
        /// </summary>
        public int Priority { get; protected set; } = 0;
    }
}
