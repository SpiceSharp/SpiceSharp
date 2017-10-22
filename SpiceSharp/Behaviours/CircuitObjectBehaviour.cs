using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviours
{
    /// <summary>
    /// Represents a behaviour for a class
    /// </summary>
    public abstract class CircuitObjectBehaviour : ICircuitObjectBehaviour
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
        protected T ComponentTyped<T>() where T : class, ICircuitObject =>  Component as T;

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
        /// Execute the behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public abstract void Execute(Circuit ckt);
    }
}
