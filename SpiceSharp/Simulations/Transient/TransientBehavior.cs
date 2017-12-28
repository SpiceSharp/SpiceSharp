using SpiceSharp.Simulations;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Sparse;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Transient behavior
    /// </summary>
    public abstract class TransientBehavior : Behavior
    {
        /// <summary>
        /// Register states
        /// </summary>
        /// <param name="states">States</param>
        public virtual void CreateStates(StatePool states)
        {
            // Do nothing (for now)
        }

        /// <summary>
        /// Calculate the state values from the DC solution
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public virtual void GetDCstate(TimeSimulation sim)
        {
            // Do nothing (for now)
        }

        /// <summary>
        /// Setup the behavior for usage with a matrix
        /// </summary>
        /// <param name="matrix">The matrix</param>
        public virtual void GetMatrixPointers(Matrix matrix)
        {
            // No pointers needed by default
        }

        /// <summary>
        /// Transient calculations
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public abstract void Transient(TimeSimulation sim);

        /// <summary>
        /// Truncate the timestep based on the LTE (Local Truncation Error)
        /// </summary>
        /// <param name="timestep">Timestep</param>
        public virtual void Truncate(ref double timestep)
        {
            // Do nothing (yet)
        }
    }
}
