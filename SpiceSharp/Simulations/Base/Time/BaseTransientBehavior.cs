using System;
using SpiceSharp.Algebra;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A template that describes transient (time-dependent) behavior.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.Behavior" />
    public abstract class BaseTransientBehavior : Behavior, ITimeBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTransientBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        protected BaseTransientBehavior(string name) : base(name) { }

        /// <summary>
        /// Creates all necessary states for the transient behavior.
        /// </summary>
        /// <param name="method">The integration method.</param>
        /// <exception cref="ArgumentNullException">method</exception>
        public virtual void CreateStates(IntegrationMethod method)
        {
            // Do nothing (for now)
        }

        /// <summary>
        /// Calculates the state values from the current DC solution.
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        /// <remarks>
        /// In this method, the initial value is calculated based on the operating point solution,
        /// and the result is stored in each respective <see cref="StateDerivative" /> or <see cref="StateHistory" />.
        /// </remarks>
        public virtual void GetDcState(TimeSimulation simulation)
        {
            // Do nothing (for now)
        }

        /// <summary>
        /// Allocate elements in the Y-matrix and Rhs-vector to populate during loading. Additional
        /// equations can also be allocated here.
        /// </summary>
        /// <param name="solver">The solver.</param>
        public abstract void GetEquationPointers(Solver<double> solver);

        /// <summary>
        /// Perform time-dependent calculations.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        public abstract void Transient(TimeSimulation simulation);
    }
}
