using System.Collections.Generic;
using SpiceSharp.Parameters;
using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Represents a behaviour for a class
    /// </summary>
    public abstract class CircuitObjectBehavior : ICircuitObjectBehavior
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
        /// Gets the component this behaviour uses
        /// </summary>
        /// <typeparam name="T">The component type</typeparam>
        /// <returns></returns>
        protected T ComponentTyped<T>() where T : CircuitObject => Component as T;

        /// <summary>
        /// Build a dictionary of named parameters
        /// </summary>
        protected void BuildParameterDictionary()
        {
            NamedParameters = new Dictionary<string, Parameter>();

            // Reflection can be used to find all parameters in the behavior
            var properties = GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
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
        /// Setup the behaviour
        /// If this method returns false, then the behavior is assumed not to change during simulations. If the method
        /// returns true, then the behavior will be registered for execution during analysis. This can be used to flag
        /// a behavior for data-only.
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns>Returns false if the behavior is only describing data</returns>
        public virtual bool Setup(CircuitObject component, Circuit ckt)
        {
            Component = component;
            return true;
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
                BuildParameterDictionary();
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
                BuildParameterDictionary();
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
