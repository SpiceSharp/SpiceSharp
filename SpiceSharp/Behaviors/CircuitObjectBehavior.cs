using System;
using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Represents a behaviour for a class
    /// </summary>
    public abstract class CircuitObjectBehavior
    {
        /// <summary>
        /// The component the behaviour acts upon
        /// </summary>
        protected CircuitObject Component { get; private set; }

        /// <summary>
        /// A table of named parameters
        /// </summary>
        protected Dictionary<string, Parameter> NamedParameters { get; set; } = null;

        /// <summary>
        /// Gets whether or not the behavior is already set up
        /// </summary>
        public bool DataOnly { get; protected set; } = false;

        /// <summary>
        /// Get a behavior of a specific type
        /// </summary>
        /// <typeparam name="T">The circuit behavior we want to ask</typeparam>
        /// <param name="co">The circuit object with the behaviors</param>
        /// <returns></returns>
        protected T GetBehavior<T>(CircuitObject co) where T : CircuitObjectBehavior
        {
            // Get base class
            var behavior = co.GetBehavior(typeof(T).BaseType) as T;
            if (behavior == null)
                throw new CircuitException($"{co.Name}: Could not find behavior \"{typeof(T).Name}\"");
            return behavior;
        }

        /// <summary>
        /// Setup the behaviour
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public virtual void Setup(CircuitObject component, Circuit ckt)
        {
            // Do nothing
        }

        /// <summary>
        /// Unsetup the behaviour
        /// </summary>
        public virtual void Unsetup()
        {
            // Do nothing
        }

        /// <summary>
        /// Create a getter method for extracting a parameter
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="parameter">Parameter</param>
        /// <returns></returns>
        public virtual Func<double> CreateGetter(Circuit ckt, string parameter)
        {
            // No gettable parameters by default
            return () => double.NaN;
        }

        /// <summary>
        /// Helper function for binding an extra equation in a circuit
        /// </summary>
        /// <param name="ckt">The circuit</param>
        /// <param name="type">The type</param>
        /// <returns></returns>
        protected CircuitNode CreateNode(Circuit ckt, CircuitIdentifier name, CircuitNode.NodeType type = CircuitNode.NodeType.Voltage)
        {
            // Map the extra equations
            return ckt.Nodes.Create(name, type);
        }
    }
}
