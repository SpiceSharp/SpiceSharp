using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DelayBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="VoltageDelay" />.
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
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
            var solver = _state.Solver;
            CPosBranchPtr = solver.GetMatrixElement(PosNode, BranchEq);
            CNegBranchPtr = solver.GetMatrixElement(NegNode, BranchEq);
            CBranchPosPtr = solver.GetMatrixElement(BranchEq, PosNode);
            CBranchNegPtr = solver.GetMatrixElement(BranchEq, NegNode);
            CBranchControlPosPtr = solver.GetMatrixElement(BranchEq, ContPosNode);
            CBranchControlNegPtr = solver.GetMatrixElement(BranchEq, ContNegNode);
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            CPosBranchPtr = null;
            CNegBranchPtr = null;
            CBranchPosPtr = null;
            CBranchNegPtr = null;
            CBranchControlPosPtr = null;
            CBranchControlNegPtr = null;
        }

        /// <summary>
        /// Initializes the small-signal parameters.
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
