using System;
using SpiceSharp.Algebra;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Transient behavior
    /// </summary>
    public abstract class BaseTransientBehavior : Behavior
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        protected BaseTransientBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Register states
        /// </summary>
        /// <param name="method">The integration method</param>
        public virtual void CreateStates(IntegrationMethod method)
        {
			if (method == null)
				throw new ArgumentNullException(nameof(method));

            // Do nothing (for now)
        }

        /// <summary>
        /// Calculate the state values from the DC solution
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public virtual void GetDcState(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            // Do nothing (for now)
        }

        /// <summary>
        /// Setup the behavior for usage with a matrix
        /// </summary>
        /// <param name="solver">The matrix</param>
        public abstract void GetEquationPointers(Solver<double> solver);

        /// <summary>
        /// Transient calculations
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public abstract void Transient(TimeSimulation simulation);

        /// <summary>
        /// Truncate the timestep based on the LTE (Local Truncation Error)
        /// </summary>
        /// <returns>The timestep that should be used for this behavior</returns>
        public virtual double Truncate() => double.PositiveInfinity;
    }
}
