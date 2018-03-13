using System;
using SpiceSharp.Behaviors;

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
        /// Gets a behavior from the entity
        /// </summary>
        /// <typeparam name="T">Behavior base type</typeparam>
        /// <param name="pool">Pool of all behaviors</param>
        /// <returns></returns>
        public virtual T GetBehavior<T>(BehaviorPool pool) where T : Behavior
        {
            if (Behaviors.TryGetValue(typeof(T), out var factory))
            {
                // Create the behavior
                Behavior behavior = factory();

                // Setup the behavior
                SetupDataProvider provider = BuildSetupDataProvider(pool);
                behavior.Setup(provider);
                return (T)behavior;
            }
            return null;
        }
        
        /// <summary>
        /// Build the data provider for setting up a behavior for the entity
        /// The entity can control which parameters and behaviors are visible to behaviors in this way
        /// </summary>
        /// <param name="pool">All behaviors</param>
        /// <returns></returns>
        protected virtual SetupDataProvider BuildSetupDataProvider(BehaviorPool pool)
        {
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));

            // By default, we include the parameters of this entity
            SetupDataProvider result = new SetupDataProvider();
            result.Add("entity", ParameterSets);
            result.Add("entity", pool.GetEntityBehaviors(Name));
            return result;
        }

        /// <summary>
        /// Gets the priority of this object
        /// </summary>
        public int Priority { get; protected set; } = 0;

        /// <summary>
        /// Setup the component
        /// </summary>
        /// <param name="circuit">Circuit</param>
        public abstract void Setup(Circuit circuit);

        /// <summary>
        /// Unsetup/destroy the component
        /// </summary>
        /// <param name="circuit">Circuit</param>
        public virtual void Unsetup(Circuit circuit)
        {
            // Do nothing
        }
    }
}
