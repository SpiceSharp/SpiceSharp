using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Interface for a behaviour
    /// </summary>
    public interface ICircuitObjectBehavior
    {
        /// <summary>
        /// Setup the circuit object behaviour
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        void Setup(CircuitObject component, Circuit ckt);

        /// <summary>
        /// Unsetup the object behaviour
        /// </summary>
        void Unsetup();
    }
}
