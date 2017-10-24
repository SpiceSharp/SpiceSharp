using SpiceSharp.Behaviours;

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
        /// Use initial conditions for the device
        /// </summary>
        /// <param name="ckt"></param>
        void SetIc(Circuit ckt);

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt"></param>
        // void Temperature(Circuit ckt);

        /// <summary>
        /// Accept the current timepoint as the solution
        /// </summary>
        /// <param name="ckt">The circuit</param>
        void Accept(Circuit ckt);

        /// <summary>
        /// Unsetup/destroy the component
        /// </summary>
        /// <param name="ckt">The circuit</param>
        void Unsetup(Circuit ckt);

        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timeStep">Timestep</param>
        void Truncate(Circuit ckt, ref double timeStep);
    }
}
