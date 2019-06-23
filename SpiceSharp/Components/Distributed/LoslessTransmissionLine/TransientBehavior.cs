using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Distributed;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.LosslessTransmissionLineBehaviors
{
    /// <summary>
    /// Transient behavior for a <see cref="LosslessTransmissionLine" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.BaseTransientBehavior" />
    public class TransientBehavior : BiasingBehavior, ITimeBehavior
    {
        /// <summary>
        /// Gets the delayed signals.
        /// </summary>
        /// <value>
        /// The signals.
        /// </value>
        public DelayedSignal Signals { get; private set; }

        /// <summary>
        /// Nodes
        /// </summary>
        protected VectorElement<double> Ibr1Ptr { get; private set; }
        protected VectorElement<double> Ibr2Ptr { get; private set; }
        
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
            Ibr1Ptr = solver.GetRhsElement(BranchEq1);
            Ibr2Ptr = solver.GetRhsElement(BranchEq2);
        }

        /// <summary>
        /// Creates all necessary states for the transient behavior.
        /// </summary>
        /// <param name="method">The integration method.</param>
        public void CreateStates(IntegrationMethod method)
        {
            Signals = new DelayedSignal(2, BaseParameters.Delay);
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

            // Calculate the inputs
            var input1 = sol[Pos2] - sol[Neg2] + BaseParameters.Impedance * sol[BranchEq2];
            var input2 = sol[Pos1] - sol[Neg1] + BaseParameters.Impedance * sol[BranchEq1];
            Signals.SetProbedValues(input1, input2);
        }

        /// <summary>
        /// Perform time-dependent calculations.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        public void Transient(TimeSimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));
            var sol = simulation.RealState.Solution;

            // Calculate inputs
            var input1 = sol[Pos2] - sol[Neg2] + BaseParameters.Impedance * sol[BranchEq2];
            var input2 = sol[Pos1] - sol[Neg1] + BaseParameters.Impedance * sol[BranchEq1];
            Signals.SetProbedValues(input1, input2);

            // Update the branch equations
            Ibr1Ptr.Value += Signals.Values[0];
            Ibr2Ptr.Value += Signals.Values[1];
        }
    }
}
