using System;
using System.Collections.Generic;
using SpiceSharp.Behaviors;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// Base class for any circuit object that can take part in simulations
    /// </summary>
    public abstract class CircuitObject
    {
        /// <summary>
        /// Available behaviors for the circuit object
        /// </summary>
        protected Dictionary<Type, CircuitObjectBehavior> Behaviors { get; } = new Dictionary<Type, CircuitObjectBehavior>();

        /// <summary>
        /// Get the name of the object
        /// </summary>
        public CircuitIdentifier Name { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the object</param>
        public CircuitObject(CircuitIdentifier name)
        {
            Name = name;
        }

        /// <summary>
        /// Register a behavior
        /// </summary>
        /// <param name="behavior">Behavior</param>
        public void RegisterBehavior(CircuitObjectBehavior behavior)
        {
            Type type = behavior.GetType().BaseType;
            Behaviors[type] = behavior;
        }

        /// <summary>
        /// Get the behavior of the object
        /// </summary>
        /// <param name="behaviorbase">The base class of the behavior</param>
        public CircuitObjectBehavior GetBehavior(Type behaviorbase) => Behaviors[behaviorbase];

        /// <summary>
        /// Try and get a behavior from the object
        /// </summary>
        /// <param name="behaviorbase">Base class type of the behavior</param>
        /// <param name="behavior">Behavior</param>
        /// <returns></returns>
        public bool TryGetBehavior(Type behaviorbase, out CircuitObjectBehavior behavior) => Behaviors.TryGetValue(behaviorbase, out behavior);

        /// <summary>
        /// Get the priority of this object
        /// </summary>
        public int Priority { get; protected set; } = 0;

        /// <summary>
        /// Setup the component
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public virtual void Setup(Circuit ckt)
        {
            // Do nothing
        }

        /// <summary>
        /// Unsetup/destroy the component
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public virtual void Unsetup(Circuit ckt)
        {
            // Do nothing
        }

        /// <summary>
        /// Set a parameter of all behaviors
        /// </summary>
        /// <param name="parameter">Parameter name</param>
        /// <param name="value">Parameter value</param>
        public virtual void Set(string parameter, double value)
        {
            bool found = false;
            foreach (var behavior in Behaviors.Values)
            {
                found |= behavior.Set(parameter, value);
            }
            if (!found)
                CircuitWarning.Warning(this, $"{Name}: Unrecognized parameter {parameter}");
        }

        /// <summary>
        /// Set the parameter for a specific behavior
        /// </summary>
        /// <param name="behaviorbase">Base type of the behavior</param>
        /// <param name="parameter">Parameter name</param>
        /// <param name="value">Parameter value</param>
        public virtual void Set(Type behaviorbase, string parameter, double value)
        {
            if (Behaviors.TryGetValue(behaviorbase, out CircuitObjectBehavior behavior))
            {
                if (behavior.Set(parameter, value))
                    return;
            }
            CircuitWarning.Warning(this, $"{Name}: Unrecognized parameter {parameter}");
        }

        /// <summary>
        /// Ask a parameter to a specific behavior
        /// </summary>
        /// <param name="behaviorbase">Base type of the behavior</param>
        /// <param name="parameter">Parameter name</param>
        /// <returns></returns>
        public virtual double Ask(Type behaviorbase, string parameter)
        {
            if (Behaviors.TryGetValue(behaviorbase, out CircuitObjectBehavior behavior))
            {
                if (behavior.Ask(parameter, out double value))
                    return value;
            }
            CircuitWarning.Warning(this, $"{Name}: Unrecognized parameter {parameter}");
            return double.NaN;
        }
    }
}
