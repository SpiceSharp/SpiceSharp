using System.Numerics;
using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.VoltageControlledVoltageSourceBehaviors
{
    /// <summary>
    /// AC behavior for a <see cref="VoltageControlledVoltageSource"/>
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Gets the voltage applied by the source.
        /// </summary>
        [ParameterName("v"), ParameterName("v_c"), ParameterInfo("Complex voltage")]
        public Complex GetVoltage(ComplexSimulationState state)
        {
            return state.ThrowIfNotBound(this).Solution[PosNode] - state.Solution[NegNode];
        }

        /// <summary>
        /// Gets the current through the source.
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_c"), ParameterInfo("Complex current")]
        public Complex GetCurrent(ComplexSimulationState state)
        {
            return state.ThrowIfNotBound(this).Solution[BranchEq];
        }

        /// <summary>
        /// Gets the power dissipated by the source.
        /// </summary>
        [ParameterName("p"), ParameterName("p_c"), ParameterInfo("Complex power")]
        public Complex GetPower(ComplexSimulationState state)
        {
            state.ThrowIfNotBound(this);

            var v = state.Solution[PosNode] - state.Solution[NegNode];
            var i = state.Solution[BranchEq];
            return -v * Complex.Conjugate(i);
        }

        /// <summary>
        /// Gets the complex matrix elements.
        /// </summary>
        /// <value>
        /// The complex matrix elements.
        /// </value>
        protected ElementSet<Complex> ComplexElements { get; private set; }

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
            ComplexElements = new ElementSet<Complex>(ComplexState.Solver, new[] {
                new MatrixLocation(PosNode, BranchEq),
                new MatrixLocation(BranchEq, PosNode),
                new MatrixLocation(NegNode, BranchEq),
                new MatrixLocation(BranchEq, NegNode),
                new MatrixLocation(BranchEq, ContPosNode),
                new MatrixLocation(BranchEq, ContNegNode)
            });
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            ComplexState = null;
            ComplexElements?.Destroy();
            ComplexElements = null;
        }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            var value = BaseParameters.Coefficient.Value;
            ComplexElements.Add(1, 1, -1, -1, -value, value);
        }
    }
}
