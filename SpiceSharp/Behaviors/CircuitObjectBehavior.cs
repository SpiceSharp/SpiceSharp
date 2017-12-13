using System.Collections.Generic;
using SpiceSharp.Parameters;
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
        /// Build a dictionary of named parameters for an object
        /// These objects can then be accessed using Set() and Ask()
        /// </summary>
        protected void BuildParameterDictionary(object obj)
        {
            NamedParameters = new Dictionary<string, Parameter>();

            // Reflection can be used to find all parameters in the behavior
            var properties = obj.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            foreach (var property in properties)
            {
                if (property.DeclaringType == typeof(Parameter))
                {
                    // Check for custom types
                    var names = (SpiceName[])property.GetCustomAttributes(typeof(SpiceName), false);
                    if (names.Length > 0)
                    {
                        var parameter = (Parameter)property.GetValue(this);
                        foreach (var name in names)
                        {
                            NamedParameters.Add(name.Name, parameter);
                        }
                    }
                }
            }
        }

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
        /// Set a parameter of this behavior
        /// Returns true if this behavior has a parameter by that name
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        public virtual bool Set(string name, double value)
        {
            if (NamedParameters == null)
                BuildParameterDictionary(this);
            if (NamedParameters.TryGetValue(name, out Parameter parameter))
            {
                parameter.Set(value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Ask a parameter from the behavior
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        /// <returns></returns>
        public virtual bool Ask(string name, out double value)
        {
            // No parameter by default
            if (NamedParameters == null)
                BuildParameterDictionary(this);
            if (NamedParameters.TryGetValue(name, out Parameter parameter))
            {
                value = parameter.Value;
                return true;
            }
            value = double.NaN;
            return false;
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
