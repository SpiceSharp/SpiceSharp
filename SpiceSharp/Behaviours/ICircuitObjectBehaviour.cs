using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviours
{
    /// <summary>
    /// Interface for a behaviour
    /// </summary>
    public interface ICircuitObjectBehaviour
    {
        /// <summary>
        /// Setup the circuit object behaviour
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        void Setup(ICircuitObject component, Circuit ckt);

        /// <summary>
        /// Execute the circuit object behaviour
        /// </summary>
        /// <param name="ckt"></param>
        void Execute(Circuit ckt);
    }
}
