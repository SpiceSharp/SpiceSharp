using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.LosslessTransmissionLineBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="LosslessTransmissionLine" />.
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Gets the left (positive, positive) element.
        /// </summary>
        protected MatrixElement<Complex> CPos1Pos1Ptr { get; private set; }

        /// <summary>
        /// Gets the left (positive, internal) element.
        /// </summary>
        protected MatrixElement<Complex> CPos1Int1Ptr { get; private set; }

        /// <summary>
        /// Gets the left (negative, branch) element.
        /// </summary>
        protected MatrixElement<Complex> CNeg1Ibr1Ptr { get; private set; }

        /// <summary>
        /// Gets the right (positive, positive) element.
        /// </summary>
        protected MatrixElement<Complex> CPos2Pos2Ptr { get; private set; }

        /// <summary>
        /// Gets the right (negative, branch) element.
        /// </summary>
        protected MatrixElement<Complex> CNeg2Ibr2Ptr { get; private set; }

        /// <summary>
        /// Gets the left (internal, positive) element.
        /// </summary>
        protected MatrixElement<Complex> CInt1Pos1Ptr { get; private set; }

        /// <summary>
        /// Gets the left (internal, internal) element.
        /// </summary>
        protected MatrixElement<Complex> CInt1Int1Ptr { get; private set; }

        /// <summary>
        /// Gets the left (internal, branch) element.
        /// </summary>
        protected MatrixElement<Complex> CInt1Ibr1Ptr { get; private set; }

        /// <summary>
        /// Gets the (left internal, right internal) element.
        /// </summary>
        protected MatrixElement<Complex> CInt2Int2Ptr { get; private set; }

        /// <summary>
        /// Gets the right (internal, branch) element.
        /// </summary>
        protected MatrixElement<Complex> CInt2Ibr2Ptr { get; private set; }

        /// <summary>
        /// Gets the left (branch, negative) element.
        /// </summary>
        protected MatrixElement<Complex> CIbr1Neg1Ptr { get; private set; }

        /// <summary>
        /// Gets the (left branch, right positive) element.
        /// </summary>
        protected MatrixElement<Complex> CIbr1Pos2Ptr { get; private set; }

        /// <summary>
        /// Gets the (left branch, right negative) element.
        /// </summary>
        protected MatrixElement<Complex> CIbr1Neg2Ptr { get; private set; }

        /// <summary>
        /// Gets the left (branch, internal) element.
        /// </summary>
        protected MatrixElement<Complex> CIbr1Int1Ptr { get; private set; }

        /// <summary>
        /// Gets the (left branch, right branch) element.
        /// </summary>
        protected MatrixElement<Complex> CIbr1Ibr2Ptr { get; private set; }

        /// <summary>
        /// Gets the (right branch, left positive) element.
        /// </summary>
        protected MatrixElement<Complex> CIbr2Pos1Ptr { get; private set; }

        /// <summary>
        /// Gets the (right branch, left negative) element.
        /// </summary>
        protected MatrixElement<Complex> CIbr2Neg1Ptr { get; private set; }

        /// <summary>
        /// Gets the right (branch, negative) element.
        /// </summary>
        protected MatrixElement<Complex> CIbr2Neg2Ptr { get; private set; }

        /// <summary>
        /// Gets the right (branch, internal) element.
        /// </summary>
        protected MatrixElement<Complex> CIbr2Int2Ptr { get; private set; }

        /// <summary>
        /// Gets the (right branch, left internal) element.
        /// </summary>
        protected MatrixElement<Complex> CIbr2Ibr1Ptr { get; private set; }

        /// <summary>
        /// Gets the right (positive, internal) element.
        /// </summary>
        protected MatrixElement<Complex> CPos2Int2Ptr { get; private set; }

        /// <summary>
        /// Gets the right (internal, positive) element.
        /// </summary>
        protected MatrixElement<Complex> CInt2Pos2Ptr { get; private set; }

        // Cache
        private ComplexSimulationState _state;

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
        /// Bind the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">The context.</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            base.Bind(simulation, context);

            _state = ((FrequencySimulation)simulation).ComplexState;
            var solver = _state.Solver.ThrowIfNull("solver");
            CPos1Pos1Ptr = solver.GetMatrixElement(Pos1, Pos1);
            CPos1Int1Ptr = solver.GetMatrixElement(Pos1, Internal1);
            CNeg1Ibr1Ptr = solver.GetMatrixElement(Neg1, BranchEq1);
            CPos2Pos2Ptr = solver.GetMatrixElement(Pos2, Pos2);
            CNeg2Ibr2Ptr = solver.GetMatrixElement(Neg2, BranchEq2);
            CInt1Pos1Ptr = solver.GetMatrixElement(Internal1, Pos1);
            CInt1Int1Ptr = solver.GetMatrixElement(Internal1, Internal1);
            CInt1Ibr1Ptr = solver.GetMatrixElement(Internal1, BranchEq1);
            CInt2Int2Ptr = solver.GetMatrixElement(Internal2, Internal2);
            CInt2Ibr2Ptr = solver.GetMatrixElement(Internal2, BranchEq2);
            CIbr1Neg1Ptr = solver.GetMatrixElement(BranchEq1, Neg1);
            CIbr1Pos2Ptr = solver.GetMatrixElement(BranchEq1, Pos2);
            CIbr1Neg2Ptr = solver.GetMatrixElement(BranchEq1, Neg2);
            CIbr1Int1Ptr = solver.GetMatrixElement(BranchEq1, Internal1);
            CIbr1Ibr2Ptr = solver.GetMatrixElement(BranchEq1, BranchEq2);
            CIbr2Pos1Ptr = solver.GetMatrixElement(BranchEq2, Pos1);
            CIbr2Neg1Ptr = solver.GetMatrixElement(BranchEq2, Neg1);
            CIbr2Neg2Ptr = solver.GetMatrixElement(BranchEq2, Neg2);
            CIbr2Int2Ptr = solver.GetMatrixElement(BranchEq2, Internal2);
            CIbr2Ibr1Ptr = solver.GetMatrixElement(BranchEq2, BranchEq1);
            CPos2Int2Ptr = solver.GetMatrixElement(Pos2, Internal2);
            CInt2Pos2Ptr = solver.GetMatrixElement(Internal2, Pos2);
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            CPos1Pos1Ptr = null;
            CPos1Int1Ptr = null;
            CNeg1Ibr1Ptr = null;
            CPos2Pos2Ptr = null;
            CNeg2Ibr2Ptr = null;
            CInt1Pos1Ptr = null;
            CInt1Int1Ptr = null;
            CInt1Ibr1Ptr = null;
            CInt2Int2Ptr = null;
            CInt2Ibr2Ptr = null;
            CIbr1Neg1Ptr = null;
            CIbr1Pos2Ptr = null;
            CIbr1Neg2Ptr = null;
            CIbr1Int1Ptr = null;
            CIbr1Ibr2Ptr = null;
            CIbr2Pos1Ptr = null;
            CIbr2Neg1Ptr = null;
            CIbr2Neg2Ptr = null;
            CIbr2Int2Ptr = null;
            CIbr2Ibr1Ptr = null;
            CPos2Int2Ptr = null;
            CInt2Pos2Ptr = null;
        }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <summary>
        /// Load the Y-matrix and right-hand side vector for frequency domain analysis.
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            var laplace = _state.Laplace;
            var factor = Complex.Exp(-laplace * BaseParameters.Delay.Value);

            var admittance = BaseParameters.Admittance;
            CPos1Pos1Ptr.Value += admittance;
            CPos1Int1Ptr.Value -= admittance;
            CNeg1Ibr1Ptr.Value -= 1;
            CPos2Pos2Ptr.Value += admittance;
            CNeg2Ibr2Ptr.Value -= 1;
            CInt1Pos1Ptr.Value -= admittance;
            CInt1Int1Ptr.Value += admittance;
            CInt1Ibr1Ptr.Value += 1;
            CInt2Int2Ptr.Value += admittance;
            CInt2Ibr2Ptr.Value += 1;
            CIbr1Neg1Ptr.Value -= 1;
            CIbr1Pos2Ptr.Value -= factor;
            CIbr1Neg2Ptr.Value += factor;
            CIbr1Int1Ptr.Value += 1;
            CIbr1Ibr2Ptr.Value -= factor * BaseParameters.Impedance;
            CIbr2Pos1Ptr.Value -= factor;
            CIbr2Neg1Ptr.Value += factor;
            CIbr2Neg2Ptr.Value -= 1;
            CIbr2Int2Ptr.Value += 1;
            CIbr2Ibr1Ptr.Value -= factor * BaseParameters.Impedance;
            CPos2Int2Ptr.Value -= admittance;
            CInt2Pos2Ptr.Value -= admittance;
        }
    }
}
