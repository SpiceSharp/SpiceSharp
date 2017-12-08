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
        /// Gets the component this behaviour uses
        /// </summary>
        /// <typeparam name="T">The component type</typeparam>
        /// <returns></returns>
        protected T ComponentTyped<T>() where T : CircuitObject => Component as T;

        /// <summary>
        /// Setup the behaviour
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public virtual void Setup(CircuitObject component, Circuit ckt)
        {
            Component = component;
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
        /// <param name="parameter">Parameter name</param>
        /// <param name="value">Parameter value</param>
        public virtual bool Set(string parameter, double value)
        {
            // No parameter by default
            return false;
        }

        /// <summary>
        /// Ask a parameter of this behavior
        /// Returns true if this behavior has a parameter by that name
        /// </summary>
        /// <param name="parameter">Parameter name</param>
        /// <returns></returns>
        public virtual bool Ask(string parameter, out double value)
        {
            // No parameter by default
            value = double.NaN;
            return false;
        }
    }
}
