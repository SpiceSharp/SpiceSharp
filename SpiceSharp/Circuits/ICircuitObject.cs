using SpiceSharp.Behaviors;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// Supports circuit simulation methods.
    /// </summary>
    public interface ICircuitObject
    {
        /// <summary>
        /// Get the name of the object
        /// </summary>
        CircuitIdentifier Name { get; }

        /// <summary>
        /// Get the priority of this object
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Setup the component
        /// </summary>
        /// <param name="ckt">The circuit</param>
        void Setup(Circuit ckt);

        /// <summary>
        /// Unsetup/destroy the component
        /// </summary>
        /// <param name="ckt">The circuit</param>
        void Unsetup(Circuit ckt);
    }
}
