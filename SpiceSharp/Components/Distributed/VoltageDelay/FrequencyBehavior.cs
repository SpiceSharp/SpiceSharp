using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Algebra.Numerics;
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
        /// Nodes
        /// </summary>
        protected MatrixElement<PreciseComplex> CPosBranchPtr { get; private set; }
        protected MatrixElement<PreciseComplex> CNegBranchPtr { get; private set; }
        protected MatrixElement<PreciseComplex> CBranchPosPtr { get; private set; }
        protected MatrixElement<PreciseComplex> CBranchNegPtr { get; private set; }
        protected MatrixElement<PreciseComplex> CBranchControlNegPtr { get; private set; }
        protected MatrixElement<PreciseComplex> CBranchControlPosPtr { get; private set; }

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
        public void GetEquationPointers(Solver<PreciseComplex> solver)
        {
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
            var laplace = simulation.ComplexState.Laplace;
            var factor = PreciseComplex.Exp(-laplace * BaseParameters.Delay);

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
