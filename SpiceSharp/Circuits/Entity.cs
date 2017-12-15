using System;
using System.Reflection;
using System.Collections.Generic;
using SpiceSharp.Behaviors;
using SpiceSharp.Parameters;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// Base class for any circuit object that can take part in simulations
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// Available behaviors for the circuit object
        /// </summary>
        protected Dictionary<Type, Behavior> Behaviors { get; } = new Dictionary<Type, Behavior>();

        /// <summary>
        /// A table of named parameters
        /// </summary>
        protected Dictionary<string, Parameter> NamedParameters { get; } = new Dictionary<string, Parameter>();
        protected bool CollectedParameters = false;

        /// <summary>
        /// Get the name of the object
        /// </summary>
        public Identifier Name { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the object</param>
        public Entity(Identifier name)
        {
            Name = name;
        }

        /// <summary>
        /// Register a behavior
        /// </summary>
        /// <param name="behavior">Behavior</param>
        public void RegisterBehavior(Behavior behavior)
        {
            Type type = behavior.GetType().BaseType;
            Behaviors[type] = behavior;
        }

        /// <summary>
        /// Collect all named parameters from the behaviors
        /// </summary>
        protected void CollectNamedParameters()
        {
            foreach (var behavior in Behaviors.Values)
                CollectNamedParameters(behavior);
            CollectedParameters = true;
        }

        /// <summary>
        /// Collect named parameters of an object
        /// </summary>
        protected void CollectNamedParameters(object obj)
        {
            var properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in properties)
            {
                // Store parameters
                if (property.PropertyType == typeof(Parameter))
                {
                    // Get all names
                    var parameter = (Parameter)property.GetValue(obj);
                    var names = property.GetCustomAttributes<SpiceName>();
                    foreach (SpiceName name in names)
                        NamedParameters.Add(name.Name, parameter);
                }
            }
        }

        /// <summary>
        /// Get the behavior of the object
        /// </summary>
        /// <param name="behaviorbase">The base class of the behavior</param>
        public Behavior GetBehavior(Type behaviorbase) => Behaviors[behaviorbase];

        /// <summary>
        /// Try and get a behavior from the object
        /// </summary>
        /// <param name="behaviorbase">Base class type of the behavior</param>
        /// <param name="behavior">Behavior</param>
        /// <returns></returns>
        public bool TryGetBehavior(Type behaviorbase, out Behavior behavior) => Behaviors.TryGetValue(behaviorbase, out behavior);

        /// <summary>
        /// Get the priority of this object
        /// </summary>
        public int Priority { get; protected set; } = 0;

        /// <summary>
        /// Setup the component
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public abstract void Setup(Circuit ckt);

        /// <summary>
        /// Unsetup/destroy the component
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public virtual void Unsetup(Circuit ckt)
        {
            // Do nothing
        }
        
        /// <summary>
        /// Set a parameter
        /// </summary>
        /// <param name="parameter">Parameter name</param>
        /// <param name="value">Parameter value</param>
        public virtual void Set(string parameter, double value)
        {
            // Collect all parameters
            if (!CollectedParameters)
                CollectNamedParameters();

            // Set the parameters
            NamedParameters[parameter].Set(value);
        }

        /// <summary>
        /// Ask a parameter
        /// </summary>
        /// <param name="parameter">Parameter name</param>
        /// <returns></returns>
        public virtual double Ask(string parameter)
        {
            if (!CollectedParameters)
                CollectNamedParameters();

            // Ask the parameters
            return NamedParameters[parameter];
        }
    }
}
