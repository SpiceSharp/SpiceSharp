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
        protected ICircuitObject Component { get; private set; }

        /// <summary>
        /// Gets the component this behaviour uses
        /// </summary>
        /// <typeparam name="T">The component type</typeparam>
        /// <returns></returns>
        protected T ComponentTyped<T>() where T : class, ICircuitObject => Component as T;

        /// <summary>
        /// Setup the behaviour
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public virtual void Setup(ICircuitObject component, Circuit ckt)
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
    }
}
