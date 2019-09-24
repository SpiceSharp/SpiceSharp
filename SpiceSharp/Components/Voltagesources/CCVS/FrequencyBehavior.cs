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
        /// Gets the complex matrix elements.
        /// </summary>
        /// <value>
        /// The complex matrix elements.
        /// </value>
        protected ComplexMatrixElementSet ComplexMatrixElements { get; private set; }

        /// <summary>
        /// Gets the voltage applied by the source.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        [ParameterName("v"), ParameterName("v_c"), ParameterInfo("Complex voltage")]
        public Complex GetVoltage(ComplexSimulationState state)
        {
            return state.ThrowIfNotBound(this).Solution[PosNode] - state.Solution[NegNode];
        }

        /// <summary>
        /// Gets the current through the source.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_c"), ParameterInfo("Complex current")]
        public Complex GetCurrent(ComplexSimulationState state)
        {
            return state.ThrowIfNotBound(this).Solution[BranchEq];
        }

        /// <summary>
        /// Gets the power dissipated by the source.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        [ParameterName("p"), ParameterName("p_c"), ParameterInfo("Complex power")]
        public Complex GetPower(ComplexSimulationState state)
        {
            state.ThrowIfNotBound(this);
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
            ComplexMatrixElements = new ComplexMatrixElementSet(ComplexState.Solver,
                new MatrixPin(PosNode, BranchEq),
                new MatrixPin(NegNode, BranchEq),
                new MatrixPin(BranchEq, PosNode),
                new MatrixPin(BranchEq, NegNode),
                new MatrixPin(BranchEq, ContBranchEq));
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            ComplexState = null;
            ComplexMatrixElements?.Destroy();
            ComplexMatrixElements = null;
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
            ComplexMatrixElements.Add(1, -1, 1, -1, -BaseParameters.Coefficient);
        }
    }
}
