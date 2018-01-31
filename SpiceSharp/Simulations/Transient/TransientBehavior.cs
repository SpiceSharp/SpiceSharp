using System;
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
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        protected TransientBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Register states
        /// </summary>
        /// <param name="states">States</param>
        public virtual void CreateStates(StatePool states)
        {
			if (states == null)
				throw new ArgumentNullException(nameof(states));

            // Do nothing (for now)
        }

        /// <summary>
        /// Calculate the state values from the DC solution
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public virtual void GetDCState(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            // Do nothing (for now)
        }

        /// <summary>
        /// Setup the behavior for usage with a matrix
        /// </summary>
        /// <param name="matrix">The matrix</param>
        public virtual void GetMatrixPointers(Matrix matrix)
        {
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));

            // No pointers needed by default
        }

        /// <summary>
        /// Transient calculations
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public abstract void Transient(TimeSimulation simulation);

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
