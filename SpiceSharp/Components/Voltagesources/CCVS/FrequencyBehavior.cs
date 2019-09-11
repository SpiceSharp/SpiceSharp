using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CurrentControlledVoltageSourceBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="CurrentControlledVoltageSource"/>
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
        /// Gets the (controlling branch, branch) element.
        /// </summary>
        protected MatrixElement<Complex> CBranchControlBranchPtr { get; private set; }

        /// <summary>
        /// Gets the voltage applied by the source.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        [ParameterName("v_c"), ParameterInfo("Complex voltage")]
        public Complex GetVoltage(ComplexSimulationState state)
        {
			state.ThrowIfNull(nameof(state));

            return state.Solution[PosNode] - state.Solution[NegNode];
        }

        /// <summary>
        /// Gets the current through the source.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        [ParameterName("i_c"), ParameterName("c_c"), ParameterInfo("Complex current")]
        public Complex GetCurrent(ComplexSimulationState state)
        {
			state.ThrowIfNull(nameof(state));

            return state.Solution[BranchEq];
        }

        /// <summary>
        /// Gets the power dissipated by the source.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        [ParameterName("p_c"), ParameterInfo("Complex power")]
        public Complex GetPower(ComplexSimulationState state)
        {
			state.ThrowIfNull(nameof(state));

            var v = state.Solution[PosNode] - state.Solution[NegNode];
            var i = state.Solution[BranchEq];
            return -v * Complex.Conjugate(i);
        }

        /// <summary>
        /// Gets the complex simulation state.
        /// </summary>
        /// <value>
        /// The complex simulation state.
        /// </value>
        protected ComplexSimulationState ComplexState { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);

            ComplexState = context.States.GetValue<ComplexSimulationState>();
            var solver = ComplexState.Solver;
            CPosBranchPtr = solver.GetMatrixElement(PosNode, BranchEq);
            CNegBranchPtr = solver.GetMatrixElement(NegNode, BranchEq);
            CBranchPosPtr = solver.GetMatrixElement(BranchEq, PosNode);
            CBranchNegPtr = solver.GetMatrixElement(BranchEq, NegNode);
            CBranchControlBranchPtr = solver.GetMatrixElement(BranchEq, ContBranchEq);
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            ComplexState = null;
            CPosBranchPtr = null;
            CNegBranchPtr = null;
            CBranchPosPtr = null;
            CBranchNegPtr = null;
            CBranchControlBranchPtr = null;
        }

        /// <summary>
        /// Initialize small-signal parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            // Load Y-matrix
            CPosBranchPtr.Value += 1.0;
            CBranchPosPtr.Value += 1.0;
            CNegBranchPtr.Value -= 1.0;
            CBranchNegPtr.Value -= 1.0;
            CBranchControlBranchPtr.Value -= BaseParameters.Coefficient.Value;
        }
    }
}
