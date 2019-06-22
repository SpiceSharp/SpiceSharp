using System;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Distributed;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DelayBehaviors
{
    /// <summary>
    /// Transient
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.BaseTransientBehavior" />
    /// <seealso cref="SpiceSharp.Components.IConnectedBehavior" />
    public class TransientBehavior : BiasingBehavior, ITimeBehavior
    {
        /// <summary>
        /// Nodes
        /// </summary>
        protected VectorElement<double> BranchPtr { get; private set; }

        /// <summary>
        /// Gets the delayed signal.
        /// </summary>
        /// <value>
        /// The delayed signal.
        /// </value>
        public DelayedSignal Signal { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public TransientBehavior(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Allocate elements in the Y-matrix and Rhs-vector to populate during loading. Additional
        /// equations can also be allocated here.
        /// </summary>
        /// <param name="solver">The solver.</param>
        public void GetEquationPointers(Solver<double> solver)
        {
            solver.ThrowIfNull(nameof(solver));
            BranchPtr = solver.GetRhsElement(BranchEq);
        }

        /// <summary>
        /// Creates all necessary states for the transient behavior.
        /// </summary>
        /// <param name="method">The integration method.</param>
        public void CreateStates(IntegrationMethod method)
        {
            Signal = new DelayedSignal(1, BaseParameters.Delay);
        }

        /// <summary>
        /// Calculates the state values from the current DC solution.
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        /// <remarks>
        /// In this method, the initial value is calculated based on the operating point solution,
        /// and the result is stored in each respective <see cref="T:SpiceSharp.IntegrationMethods.StateDerivative" /> or <see cref="T:SpiceSharp.IntegrationMethods.StateHistory" />.
        /// </remarks>
        public void GetDcState(TimeSimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));

            var sol = simulation.RealState.Solution;
            var input = sol[ContPosNode] - sol[ContNegNode];
            Signal.SetProbedValues(input);
        }

        /// <summary>
        /// Perform time-dependent calculations.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        public void Transient(TimeSimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));

            var sol = simulation.RealState.Solution;
            var input = sol[ContPosNode] - sol[ContNegNode];
            Signal.SetProbedValues(input);
            BranchPtr.Value += Signal.Values[0];
        }
    }
}
