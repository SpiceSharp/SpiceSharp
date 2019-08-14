using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DelayBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="VoltageDelay" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.BaseFrequencyBehavior" />
    /// <seealso cref="SpiceSharp.Components.IConnectedBehavior" />
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Gets the (positive, branch) element.
        /// </summary>
        protected MatrixElement<Complex> CPosBranchPtr { get; private set; }

        /// <summary>
        /// Gets the (negative, branch) element.
        /// </summary>
        protected MatrixElement<Complex> CNegBranchPtr { get; private set; }

        /// <summary>
        /// Gets the (branch, positive) element.
        /// </summary>
        protected MatrixElement<Complex> CBranchPosPtr { get; private set; }

        /// <summary>
        /// Gets the (branch, negative) element.
        /// </summary>
        protected MatrixElement<Complex> CBranchNegPtr { get; private set; }

        /// <summary>
        /// Gets the (branch, ctrlneg) element.
        /// </summary>
        protected MatrixElement<Complex> CBranchControlNegPtr { get; private set; }

        /// <summary>
        /// Gets the (branch, ctrlpos) element.
        /// </summary>
        protected MatrixElement<Complex> CBranchControlPosPtr { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public FrequencyBehavior(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        /// <param name="simulation">The frequency simulation.</param>
        public void InitializeParameters(FrequencySimulation simulation)
        {
        }

        /// <summary>
        /// Allocate elements in the Y-matrix and Rhs-vector to populate during loading.
        /// </summary>
        /// <param name="solver">The solver.</param>
        public void GetEquationPointers(Solver<Complex> solver)
        {
            solver.ThrowIfNull(nameof(solver));
            CPosBranchPtr = solver.GetMatrixElement(PosNode, BranchEq);
            CNegBranchPtr = solver.GetMatrixElement(NegNode, BranchEq);
            CBranchPosPtr = solver.GetMatrixElement(BranchEq, PosNode);
            CBranchNegPtr = solver.GetMatrixElement(BranchEq, NegNode);
            CBranchControlPosPtr = solver.GetMatrixElement(BranchEq, ContPosNode);
            CBranchControlNegPtr = solver.GetMatrixElement(BranchEq, ContNegNode);
        }

        /// <summary>
        /// Load the Y-matrix and right-hand side vector for frequency domain analysis.
        /// </summary>
        /// <param name="simulation">The frequency simulation.</param>
        public void Load(FrequencySimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));
            var laplace = simulation.ComplexState.Laplace;
            var factor = Complex.Exp(-laplace * BaseParameters.Delay);

            // Load the Y-matrix and RHS-vector
            CPosBranchPtr.Value += 1.0;
            CNegBranchPtr.Value -= 1.0;
            CBranchPosPtr.Value += 1.0;
            CBranchNegPtr.Value -= 1.0;
            CBranchControlPosPtr.Value -= factor;
            CBranchControlNegPtr.Value += factor;
        }
    }
}
